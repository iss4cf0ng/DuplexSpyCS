using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PELoader48
{
    class Program
    {
        // ===================================================================
        // PASTE YOUR BASE64 x86 PE HERE
        // ===================================================================

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            Console.WriteLine(args[0]);
            string peAsString = Convert.ToBase64String(File.ReadAllBytes(args[0]));
            Console.WriteLine(peAsString);

            if (string.IsNullOrEmpty(peAsString))
            {
                Console.WriteLine("Error: Please provide a Base64 encoded x86 PE string.");
                return;
            }

            byte[] unpacked = Convert.FromBase64String(peAsString);
            PELoader pe = new PELoader(unpacked);

            if (!pe.Is32Bit)
            {
                Console.WriteLine("Error: This loader is configured for x86, but the PE provided is x64.");
                return;
            }

            // 1. Allocate memory for the image
            IntPtr codebase = NativeDeclarations.VirtualAlloc(IntPtr.Zero, pe.OptionalHeader32.SizeOfImage,
                NativeDeclarations.MEM_COMMIT | NativeDeclarations.MEM_RESERVE, NativeDeclarations.PAGE_EXECUTE_READWRITE);

            Console.WriteLine($"Allocated memory at: 0x{codebase.ToInt32():X8}");

            // 2. Copy Sections
            foreach (var section in pe.ImageSectionHeaders)
            {
                IntPtr sectionDest = IntPtr.Add(codebase, (int)section.VirtualAddress);
                if (section.SizeOfRawData > 0)
                {
                    Marshal.Copy(pe.RawBytes, (int)section.PointerToRawData, sectionDest, (int)section.SizeOfRawData);
                    Console.WriteLine($"Mapped section {new string(section.Name)} to 0x{sectionDest.ToInt32():X8}");
                }
            }

            // 3. Perform Base Relocation
            int delta = (int)(codebase.ToInt32() - (int)pe.OptionalHeader32.ImageBase);
            if (delta != 0 && pe.OptionalHeader32.BaseRelocationTable.Size > 0)
            {
                Console.WriteLine($"Relocating image. Delta: 0x{delta:X8}");
                IntPtr relocationTable = IntPtr.Add(codebase, (int)pe.OptionalHeader32.BaseRelocationTable.VirtualAddress);
                int relocationSize = (int)pe.OptionalHeader32.BaseRelocationTable.Size;
                int bytesRead = 0;

                while (bytesRead < relocationSize)
                {
                    var block = (NativeDeclarations.IMAGE_BASE_RELOCATION)Marshal.PtrToStructure(IntPtr.Add(relocationTable, bytesRead), typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));
                    if (block.SizeOfBlock == 0) break;

                    int entryCount = (int)((block.SizeOfBlock - Marshal.SizeOf(typeof(NativeDeclarations.IMAGE_BASE_RELOCATION))) / 2);
                    IntPtr entryAddress = IntPtr.Add(relocationTable, bytesRead + Marshal.SizeOf(typeof(NativeDeclarations.IMAGE_BASE_RELOCATION)));

                    for (int i = 0; i < entryCount; i++)
                    {
                        ushort value = (ushort)Marshal.ReadInt16(entryAddress, i * 2);
                        int type = value >> 12;
                        int offset = value & 0xfff;

                        if (type == 3) // IMAGE_REL_BASED_HIGHLOW
                        {
                            IntPtr patchAddr = IntPtr.Add(codebase, (int)(block.VirtualAdress + offset));
                            int originalAddr = Marshal.ReadInt32(patchAddr);
                            Marshal.WriteInt32(patchAddr, originalAddr + delta);
                        }
                    }
                    bytesRead += (int)block.SizeOfBlock;
                }
            }

            // 4. Resolve Imports
            if (pe.OptionalHeader32.ImportTable.Size > 0)
            {
                IntPtr importDirAddr = IntPtr.Add(codebase, (int)pe.OptionalHeader32.ImportTable.VirtualAddress);
                int sizeofImportDesc = Marshal.SizeOf(typeof(NativeDeclarations.IMAGE_IMPORT_DESCRIPTOR));
                int importIdx = 0;

                while (true)
                {
                    IntPtr currentImportDescAddr = IntPtr.Add(importDirAddr, importIdx * sizeofImportDesc);
                    var importDesc = (NativeDeclarations.IMAGE_IMPORT_DESCRIPTOR)Marshal.PtrToStructure(currentImportDescAddr, typeof(NativeDeclarations.IMAGE_IMPORT_DESCRIPTOR));

                    if (importDesc.Name == 0) break;

                    string dllName = Marshal.PtrToStringAnsi(IntPtr.Add(codebase, (int)importDesc.Name));
                    IntPtr hDll = NativeDeclarations.LoadLibrary(dllName);
                    Console.WriteLine($"Loaded DLL: {dllName}");

                    IntPtr thunkRef = IntPtr.Add(codebase, (int)importDesc.FirstThunk);
                    while (Marshal.ReadInt32(thunkRef) != 0)
                    {
                        IntPtr funcNameAddr = IntPtr.Add(codebase, Marshal.ReadInt32(thunkRef) + 2);
                        string funcName = Marshal.PtrToStringAnsi(funcNameAddr);
                        IntPtr funcPtr = NativeDeclarations.GetProcAddress(hDll, funcName);

                        Marshal.WriteInt32(thunkRef, funcPtr.ToInt32());
                        thunkRef = IntPtr.Add(thunkRef, 4);
                    }
                    importIdx++;
                }
            }

            // 5. Execute OEP
            Console.WriteLine("Executing Entry Point...");
            IntPtr oep = IntPtr.Add(codebase, (int)pe.OptionalHeader32.AddressOfEntryPoint);
            IntPtr hThread = NativeDeclarations.CreateThread(IntPtr.Zero, 0, oep, IntPtr.Zero, 0, IntPtr.Zero);
            NativeDeclarations.WaitForSingleObject(hThread, 0xFFFFFFFF);
        }
    }

    public class PELoader
    {
        public NativeDeclarations.IMAGE_FILE_HEADER FileHeader;
        public NativeDeclarations.IMAGE_OPTIONAL_HEADER32 OptionalHeader32;
        public NativeDeclarations.IMAGE_SECTION_HEADER[] ImageSectionHeaders;
        public byte[] RawBytes;
        public bool Is32Bit;

        public PELoader(byte[] fileBytes)
        {
            RawBytes = fileBytes;
            using (MemoryStream stream = new MemoryStream(fileBytes))
            {
                BinaryReader reader = new BinaryReader(stream);
                var dosHeader = FromBinaryReader<NativeDeclarations.IMAGE_DOS_HEADER>(reader);
                stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
                reader.ReadUInt32(); // Skip NT Signature
                FileHeader = FromBinaryReader<NativeDeclarations.IMAGE_FILE_HEADER>(reader);

                ushort magic = reader.ReadUInt16();
                stream.Seek(-2, SeekOrigin.Current);

                if (magic == 0x10B) // PE32
                {
                    Is32Bit = true;
                    OptionalHeader32 = FromBinaryReader<NativeDeclarations.IMAGE_OPTIONAL_HEADER32>(reader);
                }

                ImageSectionHeaders = new NativeDeclarations.IMAGE_SECTION_HEADER[FileHeader.NumberOfSections];
                for (int i = 0; i < FileHeader.NumberOfSections; i++)
                {
                    ImageSectionHeaders[i] = FromBinaryReader<NativeDeclarations.IMAGE_SECTION_HEADER>(reader);
                }
            }
        }

        private static T FromBinaryReader<T>(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return theStructure;
        }
    }

    public unsafe class NativeDeclarations
    {
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DOS_HEADER { public ushort e_magic; [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)] public ushort[] e_res2; public int e_lfanew; }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_FILE_HEADER { public ushort Machine; public ushort NumberOfSections; public uint TimeDateStamp; public uint PointerToSymbolTable; public uint NumberOfSymbols; public ushort SizeOfOptionalHeader; public ushort Characteristics; }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY { public uint VirtualAddress; public uint Size; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public ushort Magic; public byte MajorLinkerVersion; public byte MinorLinkerVersion; public uint SizeOfCode; public uint SizeOfInitializedData; public uint SizeOfUninitializedData; public uint AddressOfEntryPoint; public uint BaseOfCode; public uint BaseOfData; public uint ImageBase; public uint SectionAlignment; public uint FileAlignment; public ushort MajorOSVer; public ushort MinorOSVer; public ushort MajorImgVer; public ushort MinorImgVer; public ushort MajorSubVer; public ushort MinorSubVer; public uint Win32Ver; public uint SizeOfImage; public uint SizeOfHeaders; public uint CheckSum; public ushort Subsystem; public ushort DllCharacteristics; public uint SizeStackRes; public uint SizeStackCom; public uint SizeHeapRes; public uint SizeHeapCom; public uint LoaderFlags; public uint NumberOfRvaAndSizes;
            public IMAGE_DATA_DIRECTORY ExportTable; public IMAGE_DATA_DIRECTORY ImportTable; public IMAGE_DATA_DIRECTORY ResourceTable; public IMAGE_DATA_DIRECTORY ExceptionTable; public IMAGE_DATA_DIRECTORY CertificateTable; public IMAGE_DATA_DIRECTORY BaseRelocationTable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_SECTION_HEADER { [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public char[] Name; public uint VirtualSize; public uint VirtualAddress; public uint SizeOfRawData; public uint PointerToRawData; public uint PointerToRelocations; public uint PointerToLinenumbers; public ushort NumberOfRelocations; public ushort NumberOfLinenumbers; public uint Characteristics; }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_BASE_RELOCATION { public uint VirtualAdress; public uint SizeOfBlock; }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_IMPORT_DESCRIPTOR { public uint OriginalFirstThunk; public uint TimeDateStamp; public uint ForwarderChain; public uint Name; public uint FirstThunk; }

        [DllImport("kernel32")] public static extern IntPtr VirtualAlloc(IntPtr lpAddr, uint size, uint type, uint protect);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)] public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")] public static extern IntPtr CreateThread(IntPtr lpAttribs, uint stackSize, IntPtr startAddr, IntPtr param, uint flags, IntPtr threadId);
        [DllImport("kernel32")] public static extern uint WaitForSingleObject(IntPtr hHandle, uint ms);
    }
}
