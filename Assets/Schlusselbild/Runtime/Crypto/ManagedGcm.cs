using System;
using System.Security.Cryptography;

namespace Schlusselbild.Crypto
{
    internal static class ManagedGcm
    {
        private const int BlockSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const ulong Reduction = 0xE100000000000000UL;

        public static void Encrypt(byte[] key, byte[] nonce, byte[] plaintext, byte[] ciphertextOut, out byte[] tag)
        {
            ValidateKey(key);
            ValidateNonce(nonce);

            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            if (ciphertextOut == null)
            {
                throw new ArgumentNullException(nameof(ciphertextOut));
            }

            if (ciphertextOut.Length != plaintext.Length)
            {
                throw new ArgumentException(
                    $"ciphertextOut must be {plaintext.Length} bytes, got {ciphertextOut.Length}", nameof(ciphertextOut));
            }

            using (Aes aes = CreateAes(key))
            using (ICryptoTransform ecb = aes.CreateEncryptor())
            {
                byte[] j0 = BuildJ0(nonce);
                byte[] h = EncryptSingleBlock(ecb, new byte[BlockSize]);
                byte[] mask = EncryptSingleBlock(ecb, j0);

                byte[] ciphertext = Gctr(ecb, j0, plaintext);
                Buffer.BlockCopy(ciphertext, 0, ciphertextOut, 0, ciphertext.Length);

                byte[] s = GHash(h, ciphertext);
                tag = new byte[TagSize];
                for (int i = 0; i < TagSize; i++)
                {
                    tag[i] = (byte)(s[i] ^ mask[i]);
                }
            }
        }

        public static byte[] Decrypt(byte[] key, byte[] nonce, byte[] ciphertext, byte[] tag)
        {
            ValidateKey(key);
            ValidateNonce(nonce);

            if (ciphertext == null)
            {
                throw new ArgumentNullException(nameof(ciphertext));
            }

            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (tag.Length != TagSize)
            {
                throw new ArgumentException($"tag must be {TagSize} bytes, got {tag.Length}", nameof(tag));
            }

            using (Aes aes = CreateAes(key))
            using (ICryptoTransform ecb = aes.CreateEncryptor())
            {
                byte[] j0 = BuildJ0(nonce);
                byte[] h = EncryptSingleBlock(ecb, new byte[BlockSize]);
                byte[] mask = EncryptSingleBlock(ecb, j0);

                byte[] s = GHash(h, ciphertext);
                byte[] expected = new byte[TagSize];
                for (int i = 0; i < TagSize; i++)
                {
                    expected[i] = (byte)(s[i] ^ mask[i]);
                }

                if (!FixedTimeEquals(expected, tag))
                {
                    throw new CryptographicException("GCM tag mismatch: the ciphertext, nonce or key is not authentic.");
                }

                return Gctr(ecb, j0, ciphertext);
            }
        }

        private static Aes CreateAes(byte[] key)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.KeySize = 256;
            aes.Key = key;
            return aes;
        }

        private static byte[] BuildJ0(byte[] nonce)
        {
            byte[] j0 = new byte[BlockSize];
            Buffer.BlockCopy(nonce, 0, j0, 0, NonceSize);
            j0[BlockSize - 1] = 1;
            return j0;
        }

        private static byte[] Gctr(ICryptoTransform ecb, byte[] j0, byte[] input)
        {
            byte[] output = new byte[input.Length];
            int blockCount = (input.Length + BlockSize - 1) / BlockSize;
            if (blockCount == 0)
            {
                return output;
            }

            byte[] counters = new byte[blockCount * BlockSize];
            byte[] counter = (byte[])j0.Clone();
            for (int i = 0; i < blockCount; i++)
            {
                IncrementCounter(counter);
                Buffer.BlockCopy(counter, 0, counters, i * BlockSize, BlockSize);
            }

            byte[] keystream = TransformBlocks(ecb, counters);
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(input[i] ^ keystream[i]);
            }

            return output;
        }

        private static void IncrementCounter(byte[] counter)
        {
            for (int i = counter.Length - 1; i >= counter.Length - 4; i--)
            {
                if (++counter[i] != 0)
                {
                    break;
                }
            }
        }

        private static byte[] EncryptSingleBlock(ICryptoTransform ecb, byte[] block)
        {
            return TransformBlocks(ecb, block);
        }

        private static byte[] TransformBlocks(ICryptoTransform ecb, byte[] input)
        {
            byte[] output = new byte[input.Length];
            if (input.Length > 0)
            {
                ecb.TransformBlock(input, 0, input.Length, output, 0);
            }

            return output;
        }

        private static byte[] GHash(byte[] h, byte[] ciphertext)
        {
            byte[] y = new byte[BlockSize];
            byte[] block = new byte[BlockSize];

            int offset = 0;
            while (offset < ciphertext.Length)
            {
                int count = Math.Min(BlockSize, ciphertext.Length - offset);
                Array.Clear(block, 0, BlockSize);
                Buffer.BlockCopy(ciphertext, offset, block, 0, count);
                GHashBlock(h, y, block);
                offset += count;
            }

            Array.Clear(block, 0, BlockSize);
            WriteUInt64BigEndian(block, 8, (ulong)ciphertext.Length * 8UL);
            GHashBlock(h, y, block);
            return y;
        }

        private static void GHashBlock(byte[] h, byte[] y, byte[] block)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                y[i] ^= block[i];
            }

            byte[] product = new byte[BlockSize];
            Multiply(y, h, product);
            Buffer.BlockCopy(product, 0, y, 0, BlockSize);
        }

        private static void Multiply(byte[] x, byte[] y, byte[] result)
        {
            ulong xHigh = ReadUInt64BigEndian(x, 0);
            ulong xLow = ReadUInt64BigEndian(x, 8);
            ulong vHigh = ReadUInt64BigEndian(y, 0);
            ulong vLow = ReadUInt64BigEndian(y, 8);
            ulong zHigh = 0;
            ulong zLow = 0;

            for (int i = 0; i < 128; i++)
            {
                bool bitSet = i < 64
                    ? ((xHigh >> (63 - i)) & 1UL) != 0
                    : ((xLow >> (63 - (i - 64))) & 1UL) != 0;

                if (bitSet)
                {
                    zHigh ^= vHigh;
                    zLow ^= vLow;
                }

                bool carry = (vLow & 1UL) != 0;
                vLow = (vLow >> 1) | (vHigh << 63);
                vHigh >>= 1;
                if (carry)
                {
                    vHigh ^= Reduction;
                }
            }

            WriteUInt64BigEndian(result, 0, zHigh);
            WriteUInt64BigEndian(result, 8, zLow);
        }

        private static ulong ReadUInt64BigEndian(byte[] buffer, int offset)
        {
            ulong value = 0;
            for (int i = 0; i < 8; i++)
            {
                value = (value << 8) | buffer[offset + i];
            }

            return value;
        }

        private static void WriteUInt64BigEndian(byte[] buffer, int offset, ulong value)
        {
            for (int i = 7; i >= 0; i--)
            {
                buffer[offset + i] = (byte)(value & 0xFFUL);
                value >>= 8;
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }

        private static void ValidateKey(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length != 32)
            {
                throw new ArgumentException($"key must be 32 bytes, got {key.Length}", nameof(key));
            }
        }

        private static void ValidateNonce(byte[] nonce)
        {
            if (nonce == null)
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            if (nonce.Length != NonceSize)
            {
                throw new ArgumentException($"nonce must be {NonceSize} bytes, got {nonce.Length}", nameof(nonce));
            }
        }
    }
}
