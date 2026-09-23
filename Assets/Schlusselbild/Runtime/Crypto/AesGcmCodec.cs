using System;
using System.Security.Cryptography;
using System.Text;

namespace Schlusselbild.Crypto
{
    public static class AesGcmCodec
    {
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int HeaderSize = NonceSize + TagSize;

        public static byte[] Encrypt(string plaintext, byte[] key)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            ValidateKey(key);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] nonce = CreateRandomBytes(NonceSize);
            byte[] tag;
            byte[] ciphertext = new byte[plaintextBytes.Length];

            ManagedGcm.Encrypt(key, nonce, plaintextBytes, ciphertext, out tag);

            byte[] blob = new byte[HeaderSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, blob, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, blob, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, blob, HeaderSize, ciphertext.Length);
            return blob;
        }

        public static string Decrypt(byte[] blob, byte[] key)
        {
            if (blob == null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            ValidateKey(key);

            if (blob.Length < HeaderSize)
            {
                throw new ArgumentException(
                    $"blob must be at least {HeaderSize} bytes, got {blob.Length}", nameof(blob));
            }

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[blob.Length - HeaderSize];
            Buffer.BlockCopy(blob, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(blob, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(blob, HeaderSize, ciphertext, 0, ciphertext.Length);

            byte[] plaintextBytes = ManagedGcm.Decrypt(key, nonce, ciphertext, tag);

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        private static byte[] CreateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        private static void ValidateKey(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length != KeySize)
            {
                throw new ArgumentException($"key must be {KeySize} bytes, got {key.Length}", nameof(key));
            }
        }
    }
}
