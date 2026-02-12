using System;
using System.IO;
using System.Reflection;

namespace RegexDialog
{
    public static class PathUtils
    {
        /// <summary>
        /// The subfolder name used under AppData for this application.
        /// Set this before accessing Config or any AppData path to use a different folder per host IDE.
        /// </summary>
        public static string AppDataFolderName { get; set; } = "CSharpRegexTools4Npp";

        /// <summary>
        /// The directory where the current application started
        /// </summary>
        public static string StartupPath
        {
            get
            {
                return Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
            }
        }

        /// <summary>
        /// The directory of the application in AppData\Roaming
        /// </summary>
        public static string AppDataRoamingPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
            }
        }

        /// <summary>
        /// The directory of the application in AppData\Local
        /// </summary>
        public static string AppDataLocalPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolderName);
            }
        }

        /// <summary>
        /// Search for the specified file in the specified directory if not found go up a directory at a time until it reach the root path.
        /// </summary>
        /// <param name="startDirectory">The first directory where to start searching</param>
        /// <param name="fileName">The file name (without full path) of the file to find</param>
        /// <returns>If found return the directory path where it is found. It return null otherwise.</returns>
        public static string FindFileInDirectoryAncestors(string startDirectory, string fileName)
        {
            if (File.Exists(Path.Combine(startDirectory, fileName)))
            {
                return startDirectory;
            }
            else if (startDirectory.Equals(Path.GetPathRoot(startDirectory)))
            {
                return null;
            }
            else
            {
                return FindFileInDirectoryAncestors(Path.GetDirectoryName(startDirectory), fileName);
            }
        }

        /// <summary>
        /// Clean the specified filename of all invalids characters
        /// </summary>
        /// <param name="fileName">The file name to clean</param>
        /// <param name="replacement">The text to use to replace all invalids characters (by default : "_")</param>
        public static string GetSafeFilename(string filename, string replacement = "_")
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
