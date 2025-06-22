using Microsoft.Win32;

void Copy(string srcRegFullPath, string dstRegFullPath)
{
    try
    {
        string[] srcPathSplit = srcRegFullPath.Split('\\');
        string[] dstPathSplit = dstRegFullPath.Split('\\');

        string srcKeyPath = string.Join("\\", srcPathSplit.Skip(1).Take(srcPathSplit.Length - 1).ToArray());
        string dstKeyPath = string.Join("\\", dstPathSplit.Skip(1).Take(dstPathSplit.Length - 1).ToArray());

        using (RegistryKey srcBaseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
        using (RegistryKey dstBaseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
        using (RegistryKey srcKey = srcBaseKey.OpenSubKey(srcKeyPath))
        using (RegistryKey dstKey = dstBaseKey.OpenSubKey(dstKeyPath))
        {
            if (srcKey == null)
                return;

            foreach (string strSrcSubKey in srcKey.GetSubKeyNames())
            {
                using (RegistryKey srcSubKey = srcKey.OpenSubKey(strSrcSubKey))
                {
                    string relaPath = srcSubKey.Name.Replace(srcRegFullPath, string.Empty);

                    Console.WriteLine($"Src: {srcSubKey.Name}");
                    Console.WriteLine($"Dst: {dstRegFullPath + relaPath}");

                    Console.WriteLine("----------------------");

                    Copy(srcSubKey.Name, dstRegFullPath+ relaPath);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

string srcRegFullPath = "HKEY_CURRENT_USER\\AppEvents\\testing";
string dstRegFullPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\testing";

Copy(srcRegFullPath, dstRegFullPath);