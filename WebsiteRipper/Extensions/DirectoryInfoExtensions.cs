using System;
using System.IO;

namespace WebsiteRipper.Extensions
{
    static class DirectoryInfoExtensions
    {
        public static void Clear(this DirectoryInfo directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            foreach (var subDirectory in directory.EnumerateDirectories()) subDirectory.Delete(true);
            foreach (var file in directory.EnumerateFiles())
            {
                if (file.IsReadOnly) throw new UnauthorizedAccessException(string.Format("Access to the path '{0} is denied", file));
                file.Delete();
            }
        }
    }
}
