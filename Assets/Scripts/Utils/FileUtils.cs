using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CCE.Utils
{
    public static class FileUtils
    {
        /// <summary>
        ///     Returns a unique file path like the
        ///     <param cref="path" />
        ///     param, with a unique number before
        ///     the file extension if there's a naming conflict.
        /// </summary>
        /// <example>
        ///     This code creates two files, one named test.txt and one named test-1.txt.
        ///     <code>
        /// File.Create(GetUniqueFilePath("/test.txt"));
        /// File.Create(GetUniqueFilePath("/test.txt"));
        /// </code>
        /// </example>
        public static string GetUniqueFilePath(string path)
        {
            if (!File.Exists(path)) return path;
            var i = 1;
            var uniquePath = Path.Combine(Path.GetDirectoryName(path)!,
                $"{Path.GetFileNameWithoutExtension(path)}-{i}{Path.GetExtension(path)}");

            while (File.Exists(uniquePath))
            {
                i++;
                uniquePath = Path.Combine(Path.GetDirectoryName(path)!,
                    $"{Path.GetFileNameWithoutExtension(path)}-{i}{Path.GetExtension(path)}");
            }

            return uniquePath;
        }

        public static void CopyDirectory(string srcDirPath, string destDirPath)
        {
            if (Directory.Exists(destDirPath))
            {
                Directory.Delete(destDirPath, true);
            }

            Directory.CreateDirectory(destDirPath);
            foreach (var file in Directory.EnumerateFiles(srcDirPath))
            {
                File.Copy(file, Path.Combine(destDirPath, Path.GetFileName(file)));
            }

            foreach (var directory in Directory.EnumerateDirectories(srcDirPath))
            {
                CopyDirectory(directory, Path.Combine(destDirPath, Path.GetFileName(directory)));
            }
        }

        public static IEnumerable<string> GetFilesInDirectory(string dirPath)
        {
            var result = Directory.EnumerateFiles(Path.GetFullPath(dirPath)).ToList();

            foreach (var folder in Directory.EnumerateDirectories(Path.GetFullPath(dirPath)))
            {
                result.AddRange(GetFilesInDirectory(folder));
            }

            return result;
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