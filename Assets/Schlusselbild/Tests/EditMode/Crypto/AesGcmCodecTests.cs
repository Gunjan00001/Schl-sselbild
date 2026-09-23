using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Schlusselbild.Crypto;

namespace Schlusselbild.Tests.Crypto
{
    public class AesGcmCodecTests
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int HeaderSize = NonceSize + TagSize;

        private static readonly byte[] Key = CreateKey(1);
        private static readonly byte[] OtherKey = CreateKey(2);

        private static byte[] CreateKey(byte seed)
        {
            byte[] key = new byte[32];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (byte)(seed + i);
            }

            return key;
        }

        [Test]
        public void EncryptDecrypt_Ascii_RoundTrips()
        {
            const string plaintext = "Schlusselbild QR channel: hello, world! 0123456789";

            byte[] blob = AesGcmCodec.Encrypt(plaintext, Key);

            Assert.That(AesGcmCodec.Decrypt(blob, Key), Is.EqualTo(plaintext));
        }

        [Test]
        public void EncryptDecrypt_Unicode_RoundTrips()
        {
            const string plaintext = "Schlüsselbild – naïve café – 日本語 – 🔑 – Ключ";

            byte[] blob = AesGcmCodec.Encrypt(plaintext, Key);

            Assert.That(AesGcmCodec.Decrypt(blob, Key), Is.EqualTo(plaintext));
        }

        [Test]
        public void EncryptDecrypt_EmptyString_RoundTrips()
        {
            byte[] blob = AesGcmCodec.Encrypt(string.Empty, Key);

            Assert.That(blob.Length, Is.EqualTo(HeaderSize));
            Assert.That(AesGcmCodec.Decrypt(blob, Key), Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_LongPlaintext_RoundTrips()
        {
            StringBuilder builder = new StringBuilder();
            while (builder.Length < 5000)
            {
                builder.Append("Schlüsselbild-0123456789-äöü中文-");
            }

            string plaintext = builder.ToString(0, 5000);
            Assert.That(plaintext.Length, Is.EqualTo(5000));

            byte[] blob = AesGcmCodec.Encrypt(plaintext, Key);

            Assert.That(AesGcmCodec.Decrypt(blob, Key), Is.EqualTo(plaintext));
        }

        [Test]
        public void Decrypt_WithWrongKey_ThrowsCryptographicException()
        {
            byte[] blob = AesGcmCodec.Encrypt("authenticated payload", Key);

            Assert.Catch<CryptographicException>(() => AesGcmCodec.Decrypt(blob, OtherKey));
        }

        [Test]
        public void Decrypt_WithTamperedCiphertextByte_ThrowsCryptographicException()
        {
            byte[] blob = AesGcmCodec.Encrypt("authenticated payload", Key);

            blob[blob.Length - 1] ^= 0xFF;

            Assert.Catch<CryptographicException>(() => AesGcmCodec.Decrypt(blob, Key));
        }

        [Test]
        public void Encrypt_BlobLength_IsNoncePlusTagPlusCiphertext()
        {
            const string plaintext = "known plaintext length";

            byte[] blob = AesGcmCodec.Encrypt(plaintext, Key);

            Assert.That(plaintext.Length, Is.EqualTo(22));
            Assert.That(blob.Length, Is.EqualTo(NonceSize + TagSize + Encoding.UTF8.GetByteCount(plaintext)));
            Assert.That(blob.Length, Is.EqualTo(12 + 16 + 22));

            const string multibyte = "ä";
            byte[] multibyteBlob = AesGcmCodec.Encrypt(multibyte, Key);

            Assert.That(multibyte.Length, Is.EqualTo(1));
            Assert.That(multibyteBlob.Length, Is.EqualTo(NonceSize + TagSize + 2));
        }

        [Test]
        public void Encrypt_EncryptingTwice_ProducesDifferentBlobs()
        {
            const string plaintext = "nonce must be random";

            byte[] first = AesGcmCodec.Encrypt(plaintext, Key);
            byte[] second = AesGcmCodec.Encrypt(plaintext, Key);

            CollectionAssert.AreNotEqual(first, second);
            Assert.That(AesGcmCodec.Decrypt(first, Key), Is.EqualTo(plaintext));
            Assert.That(AesGcmCodec.Decrypt(second, Key), Is.EqualTo(plaintext));
        }

        [Test]
        public void Encrypt_With16ByteKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => AesGcmCodec.Encrypt("payload", new byte[16]));
        }

        [Test]
        public void Decrypt_WithShortBlob_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => AesGcmCodec.Decrypt(new byte[HeaderSize - 1], Key));
        }

        [Test]
        public void Decrypt_With16ByteKey_ThrowsArgumentException()
        {
            byte[] blob = AesGcmCodec.Encrypt("payload", Key);

            Assert.Throws<ArgumentException>(() => AesGcmCodec.Decrypt(blob, new byte[16]));
        }
    }
}
