using System.IO;

namespace CCE.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Returns a unique file path like the <param cref="path"/> param, with a unique number before
        /// the file extension if there's a naming conflict.
        /// </summary>
        /// <example>
        /// This code creates two files, one named test.txt and one named test-1.txt.
        /// <code>
        /// File.Create(GetUniqueFilePath("/test.txt"));
        /// File.Create(GetUniqueFilePath("/test.txt"));
        /// </code>
        /// </example>
        public static string GetUniqueFilePath(string path)
        {
            if (!File.Exists(path)) return path;
            int i = 1;
            string uniquePath = Path.Combine(Path.GetDirectoryName(path)!,
                $"{Path.GetFileNameWithoutExtension(path)}-{i}.{Path.GetExtension(path)}");
            
            while (File.Exists(uniquePath))
            {
                i++;
                uniquePath = Path.Combine(Path.GetDirectoryName(path)!,
                    $"{Path.GetFileNameWithoutExtension(path)}-{i}.{Path.GetExtension(path)}");
            }

            return uniquePath;
        }
        
        public static void CopyDirectory(string srcDirPath, string destDirPath)
        {
            Directory.CreateDirectory(destDirPath);
            foreach (string file in Directory.EnumerateFiles(srcDirPath))
            {
                File.Copy(file, Path.Combine(destDirPath, Path.GetFileName(file)));
            }

            foreach (string directory in Directory.EnumerateDirectories(srcDirPath))
            {
                CopyDirectory(directory, Path.Combine(destDirPath, Path.GetFileName(directory)));
            }
        }

        public static bool IsAudioFile(string file)
        {
            return Path.GetExtension(file) switch
            {
                ".wav" => true,
                ".ogg" => true,
                ".mp3" => true,
                _ => false
            };
        }

        public static bool IsLevelFile(string file)
        {
            return Path.GetExtension(file) switch
            {
                ".cytoidlevel" => true,
                ".cytoidpack" => true,
                _ => false
            };
        }

        public static bool IsImageFile(string file)
        {
            return Path.GetExtension(file) switch
            {
                ".jpeg" => true,
                ".jpg" => true,
                ".png" => true,
                _ => false
            };
        }
    }
}