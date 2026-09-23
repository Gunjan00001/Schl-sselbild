using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace Schlusselbild.Crypto
{
    public static class KeyStore
    {
        public const int KeySize = 32;

        private const string FileName = "key.bin";

        private static string DefaultPath
        {
            get { return Path.Combine(Application.persistentDataPath, FileName); }
        }

        public static byte[] LoadOrCreate(string path = null)
        {
            string target = string.IsNullOrEmpty(path) ? DefaultPath : path;

            if (File.Exists(target))
            {
                return Load(target);
            }

            byte[] key = new byte[KeySize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            Save(key, target);
            return key;
        }

        public static byte[] Load(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            byte[] key = File.ReadAllBytes(path);
            if (key.Length != KeySize)
            {
                throw new InvalidDataException($"{path} is {key.Length} bytes, expected {KeySize}");
            }

            return key;
        }

        public static void Save(byte[] key, string path)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length != KeySize)
            {
                throw new ArgumentException($"key must be {KeySize} bytes, got {key.Length}", nameof(key));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, key);
        }
    }
}
