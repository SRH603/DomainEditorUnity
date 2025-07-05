using System;
using System.IO;

namespace Battlehub.Storage
{
    public static class PathUtils
    {
        public static string NormalizePath(string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static string GetFilePathWithoutExtension(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                // Empty strings are not allowed,
                // Occurs when the folder name begins with a dot (for example, .Cache)

                fileNameWithoutExtension = Path.GetFileName(filePath);
            }

            return Path.Combine(directory, fileNameWithoutExtension);
        }
    }

}
