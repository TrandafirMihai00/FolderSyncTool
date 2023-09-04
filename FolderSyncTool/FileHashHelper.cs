using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace FolderSyncTool
{
    internal class FileHashHelper
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Hash { get; set; }

        static public FileHashHelper GetHashFileInfo(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                string fileName = Path.GetFileName(filePath);

                return new FileHashHelper
                {
                    FilePath = filePath,
                    Hash = hash,
                    FileName = fileName
                };
            }
        }

         
    }
}
