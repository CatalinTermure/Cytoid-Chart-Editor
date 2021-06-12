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
    }
}