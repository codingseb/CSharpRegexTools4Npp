using System.IO;

namespace RegexDialog.Utils
{
    public static class PotentiallyLockedFileCopier
    {
        public static string MakeCopyIfLocked(this string sourceFile)
        {
            if(IsFileLocked(new FileInfo(sourceFile)))
            {
                string destinationFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(sourceFile));

                if (!File.Exists(destinationFile) || File.GetLastWriteTime(sourceFile) != File.GetLastWriteTime(destinationFile))
                {
                    File.Copy(sourceFile, destinationFile, true);                    
                }

                return destinationFile;

            }
            else
            {
                return sourceFile;
            }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }
    }
}