using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Shared.LocalCache 
{
    public static class Cacher
    {
        public static bool IsCached(this string fileName) 
        {
            DirtyHackWithPlayerPrefs();
            var file = ConvertPath(fileName);
            return File.Exists(file);
        }

        public static Texture2D TextureFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            Texture2D image = new(0, 0);
            ImageConversion.LoadImage(image, rawData);
            return image;
        }

        public static string TextFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            return Encoding.UTF8.GetString(rawData);
        }

        private static byte[] FromCache(this string fileName) 
        {
            DirtyHackWithPlayerPrefs();

            var file = ConvertPath(fileName);

            using (var fs = File.OpenRead(file)) 
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static Texture2D ToCache(this Texture2D data, string fileName) 
        {
            var rawData = data.EncodeToPNG();
            rawData.ToCache(fileName);
            return fileName.TextureFromCache();
        }

        public static string ToCache(this string data, string fileName) 
        {
            var rawData = Encoding.UTF8.GetBytes(data);;
            rawData.ToCache(fileName);
            return fileName.TextFromCache();
        }

        private static byte[] ToCache(this byte[] data, string fileName) 
        {
            DirtyHackWithPlayerPrefs();
            var file = ConvertPath(fileName);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }

            return data;
        }

        private static void DirtyHackWithPlayerPrefs() 
        {
            PlayerPrefs.SetString("Cacher", DateTime.UtcNow.ToString());
        }

        private static string ConvertPath(string fileName) 
        {
            var localFilesPath = $"{Application.persistentDataPath}/CachedFiles";
#if !UNITY_EDITOR && UNITY_WEBGL
            localFilesPath = "idbfs/CachedFiles";
#endif

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            return $"{localFilesPath}/{fileName}";
        }
    }
}

