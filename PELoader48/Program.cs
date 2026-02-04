using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PELoader48
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Base64 encoded x86 EXE
            string peBase64 = Convert.ToBase64String(File.ReadAllBytes(args[0]));

            byte[] peBytes = Convert.FromBase64String(peBase64);
            PELoader pe = new PELoader(peBytes);

            if (!pe.Is32Bit)
                throw new Exception("This loader only supports x86 PE");

            IntPtr imageBase = Native.VirtualAlloc(
                IntPtr.Zero,
                pe.OptionalHeader.SizeOfImage,
                Native.MEM_COMMIT | Native.MEM_RESERVE,
                Native.PAGE_EXECUTE_READWRITE
            );

            // copy headers
            Marshal.Copy(pe.RawBytes, 0, imageBase, (int)pe.OptionalHeader.SizeOfHeaders);

            // copy sections
            foreach (var sec in pe.Sections)
            {
                IntPtr dest = IntPtr.Add(imageBase, (int)sec.VirtualAddress);
                Marshal.Copy(
                    pe.RawBytes,
                    (int)sec.PointerToRawData,
                    dest,
                    (int)sec.SizeOfRawData
                );
            }

            // relocation
            long delta = imageBase.ToInt64() - pe.OptionalHeader.ImageBase;
            if (delta != 0)
                ApplyRelocation(pe, imageBase, delta);

            // imports
            ResolveImports(pe, imageBase);

            // jump to OEP
            IntPtr entry = IntPtr.Add(imageBase, (int)pe.OptionalHeader.AddressOfEntryPoint);
            IntPtr hThread = Native.CreateThread(IntPtr.Zero, 0, entry, IntPtr.Zero, 0, IntPtr.Zero);
            Native.WaitForSingleObject(hThread, 0xFFFFFFFF);
        }

        static void ApplyRelocation(PELoader pe, IntPtr baseAddr, long delta)
        {
            var dir = pe.OptionalHeader.BaseRelocationTable;
            if (dir.Size == 0) return;

            IntPtr relocBase = IntPtr.Add(baseAddr, (int)dir.VirtualAddress);
            int offset = 0;

            while (true)
            {
                IMAGE_BASE_RELOCATION block =
                    Marshal.PtrToStructure<IMAGE_BASE_RELOCATION>(IntPtr.Add(relocBase, offset));

                if (block.SizeOfBlock == 0) break;

                int count = (int)((block.SizeOfBlock - 8) / 2);
                IntPtr fixupBase = IntPtr.Add(baseAddr, (int)block.VirtualAddress);

                for (int i = 0; i < count; i++)
                {
                    ushort value = (ushort)Marshal.ReadInt16(
                        relocBase, offset + 8 + i * 2);

                    ushort type = (ushort)(value >> 12);
                    ushort rva = (ushort)(value & 0xFFF);

                    if (type == 0x3) // IMAGE_REL_BASED_HIGHLOW
                    {
                        IntPtr patch = IntPtr.Add(fixupBase, rva);
                        int original = Marshal.ReadInt32(patch);
                        Marshal.WriteInt32(patch, original + (int)delta);
                    }
                }

                offset += (int)block.SizeOfBlock;
            }
        }

        static void ResolveImports(PELoader pe, IntPtr baseAddr)
        {
            var dir = pe.OptionalHeader.ImportTable;
            if (dir.Size == 0) return;

            int descSize = Marshal.SizeOf<IMAGE_IMPORT_DESCRIPTOR>();
            IntPtr descPtr = IntPtr.Add(baseAddr, (int)dir.VirtualAddress);

            while (true)
            {
                IMAGE_IMPORT_DESCRIPTOR desc =
                    Marshal.PtrToStructure<IMAGE_IMPORT_DESCRIPTOR>(descPtr);

                if (desc.Name == 0) break;

                string dllName = Marshal.PtrToStringAnsi(
                    IntPtr.Add(baseAddr, (int)desc.Name));

                IntPtr hDll = Native.LoadLibrary(dllName);

                IntPtr thunkRef = IntPtr.Add(baseAddr,
                    (int)(desc.OriginalFirstThunk != 0
                        ? desc.OriginalFirstThunk
                        : desc.FirstThunk));

                IntPtr funcRef = IntPtr.Add(baseAddr, (int)desc.FirstThunk);

                while (true)
                {
                    int thunkData = Marshal.ReadInt32(thunkRef);
                    if (thunkData == 0) break;

                    IntPtr funcAddr;

                    if ((thunkData & 0x80000000) != 0)
                    {
                        // ordinal
                        funcAddr = Native.GetProcAddress(hDll, (IntPtr)(thunkData & 0xFFFF));
                    }
                    else
                    {
                        IntPtr namePtr = IntPtr.Add(baseAddr, thunkData);
                        string name = Marshal.PtrToStringAnsi(IntPtr.Add(namePtr, 2));
                        funcAddr = Native.GetProcAddress(hDll, name);
                    }

                    Marshal.WriteInt32(funcRef, funcAddr.ToInt32());

                    thunkRef = IntPtr.Add(thunkRef, 4);
                    funcRef = IntPtr.Add(funcRef, 4);
                }

                descPtr = IntPtr.Add(descPtr, descSize);
            }
        }
    }

    class PELoader
    {
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        public IMAGE_SECTION_HEADER[] Sections;
        public byte[] RawBytes;
        public bool Is32Bit => OptionalHeader.Magic == 0x10B;

        public PELoader(byte[] bytes)
        {
            RawBytes = bytes;
            using (var br = new BinaryReader(new MemoryStream(bytes)))
            {
                br.ReadBytes(0x3C);
                int ntOffset = br.ReadInt32();
                br.BaseStream.Position = ntOffset + 4;

                IMAGE_FILE_HEADER fh = Read<IMAGE_FILE_HEADER>(br);
                OptionalHeader = Read<IMAGE_OPTIONAL_HEADER32>(br);

                Sections = new IMAGE_SECTION_HEADER[fh.NumberOfSections];
                for (int i = 0; i < Sections.Length; i++)
                    Sections[i] = Read<IMAGE_SECTION_HEADER>(br);
            }
        }

        static T Read<T>(BinaryReader br)
        {
            byte[] data = br.ReadBytes(Marshal.SizeOf<T>());
            GCHandle h = GCHandle.Alloc(data, GCHandleType.Pinned);
            T obj = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());
            h.Free();
            return obj;
        }
    }

    static class Native
    {
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32")]
        public static extern IntPtr VirtualAlloc(
            IntPtr lpAddress, uint size, uint type, uint protect);

        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr ordinal);

        [DllImport("kernel32")]
        public static extern IntPtr CreateThread(
            IntPtr attr, uint stack, IntPtr start, IntPtr param,
            uint flags, IntPtr tid);

        [DllImport("kernel32")]
        public static extern uint WaitForSingleObject(
            IntPtr hHandle, uint ms);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_BASE_RELOCATION
    {
        public uint VirtualAddress;
        public uint SizeOfBlock;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_IMPORT_DESCRIPTOR
    {
        public uint OriginalFirstThunk;
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;
        public uint FirstThunk;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_OPTIONAL_HEADER32
    {
        public UInt16 Magic;
        public Byte MajorLinkerVersion;
        public Byte MinorLinkerVersion;
        public UInt32 SizeOfCode;
        public UInt32 SizeOfInitializedData;
        public UInt32 SizeOfUninitializedData;
        public UInt32 AddressOfEntryPoint;
        public UInt32 BaseOfCode;
        public UInt32 BaseOfData;
        public UInt32 ImageBase;
        public UInt32 SectionAlignment;
        public UInt32 FileAlignment;
        public UInt16 MajorOperatingSystemVersion;
        public UInt16 MinorOperatingSystemVersion;
        public UInt16 MajorImageVersion;
        public UInt16 MinorImageVersion;
        public UInt16 MajorSubsystemVersion;
        public UInt16 MinorSubsystemVersion;
        public UInt32 Win32VersionValue;
        public UInt32 SizeOfImage;
        public UInt32 SizeOfHeaders;
        public UInt32 CheckSum;
        public UInt16 Subsystem;
        public UInt16 DllCharacteristics;
        public UInt32 SizeOfStackReserve;
        public UInt32 SizeOfStackCommit;
        public UInt32 SizeOfHeapReserve;
        public UInt32 SizeOfHeapCommit;
        public UInt32 LoaderFlags;
        public UInt32 NumberOfRvaAndSizes;

        public IMAGE_DATA_DIRECTORY ExportTable;
        public IMAGE_DATA_DIRECTORY ImportTable;
        public IMAGE_DATA_DIRECTORY ResourceTable;
        public IMAGE_DATA_DIRECTORY ExceptionTable;
        public IMAGE_DATA_DIRECTORY CertificateTable;
        public IMAGE_DATA_DIRECTORY BaseRelocationTable;
        public IMAGE_DATA_DIRECTORY Debug;
        public IMAGE_DATA_DIRECTORY Architecture;
        public IMAGE_DATA_DIRECTORY GlobalPtr;
        public IMAGE_DATA_DIRECTORY TLSTable;
        public IMAGE_DATA_DIRECTORY LoadConfigTable;
        public IMAGE_DATA_DIRECTORY BoundImport;
        public IMAGE_DATA_DIRECTORY IAT;
        public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
        public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
        public IMAGE_DATA_DIRECTORY Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DATA_DIRECTORY
    {
        public UInt32 VirtualAddress;
        public UInt32 Size;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_SECTION_HEADER
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] Name;
        [FieldOffset(8)]
        public UInt32 VirtualSize;
        [FieldOffset(12)]
        public UInt32 VirtualAddress;
        [FieldOffset(16)]
        public UInt32 SizeOfRawData;
        [FieldOffset(20)]
        public UInt32 PointerToRawData;
        [FieldOffset(24)]
        public UInt32 PointerToRelocations;
        [FieldOffset(28)]
        public UInt32 PointerToLinenumbers;
        [FieldOffset(32)]
        public UInt16 NumberOfRelocations;
        [FieldOffset(34)]
        public UInt16 NumberOfLinenumbers;
        [FieldOffset(36)]
        public DataSectionFlags Characteristics;

        public string Section
        {
            get { return new string(Name); }
        }
    }

    [Flags]
    public enum DataSectionFlags : uint
    {

        Stub = 0x00000000,

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_FILE_HEADER
    {
        public UInt16 Machine;
        public UInt16 NumberOfSections;
        public UInt32 TimeDateStamp;
        public UInt32 PointerToSymbolTable;
        public UInt32 NumberOfSymbols;
        public UInt16 SizeOfOptionalHeader;
        public UInt16 Characteristics;
    }
}
