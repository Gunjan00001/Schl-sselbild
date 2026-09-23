using System;
using System.Security.Cryptography;
using NUnit.Framework;
using Schlusselbild.Crypto;

namespace Schlusselbild.Tests.Crypto
{
    public class GcmVectorsTests
    {
        private const string ZeroKeyHex =
            "0000000000000000000000000000000000000000000000000000000000000000";

        private const string ZeroNonceHex = "000000000000000000000000";

        private const string SpecKeyHex =
            "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308";

        private const string SpecNonceHex = "cafebabefacedbaddecaf888";

        private const string SpecFourBlockPlaintextHex =
            "d9313225f88406e5a55909c5aff5269a86a7a9531534f7da2e4c303d8a318a72" +
            "1c3c0c95956809532fcf0e2449a6b525b16aedf5aa0de657ba637b391aafd255";

        private const string SpecFourBlockCiphertextHex =
            "522dc1f099567d07f47f37a32a84427d643a8cdcbfe5c0c97598a2bd2555d1aa" +
            "8cb08e48590dbb3da7b08b1056828838c5f61e6393ba7a0abcc9f662898015ad";

        private const string NonAlignedPlaintextHex =
            "4f8b2e91aa3c5d7f0e1425364758697a8b9cadbecfe01f2132435465768798a9" +
            "b1c2d3e4f5061728394a5b6c7d8e9fa0b1c2d3e4f50617";

        private const string NonAlignedCiphertextHex =
            "c497dd44cbee269d5f321b50c2290d9d69018831653128326997c6e5d9e3c371" +
            "214e51393963a546b135de5862aaa2bdc55e2072ccb18b";

        private static byte[] Hex(string value)
        {
            if (value.Length % 2 != 0)
            {
                throw new ArgumentException($"hex literal must have an even length, got {value.Length}", nameof(value));
            }

            byte[] bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private static void AssertKnownAnswer(string keyHex, string nonceHex, string plaintextHex,
            string ciphertextHex, string tagHex, int expectedPlaintextLength)
        {
            byte[] key = Hex(keyHex);
            byte[] nonce = Hex(nonceHex);
            byte[] plaintext = Hex(plaintextHex);
            byte[] expectedCiphertext = Hex(ciphertextHex);
            byte[] expectedTag = Hex(tagHex);

            Assert.That(key.Length, Is.EqualTo(32));
            Assert.That(nonce.Length, Is.EqualTo(12));
            Assert.That(expectedTag.Length, Is.EqualTo(16));
            Assert.That(plaintext.Length, Is.EqualTo(expectedPlaintextLength));
            Assert.That(expectedCiphertext.Length, Is.EqualTo(expectedPlaintextLength));

            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag;
            ManagedGcm.Encrypt(key, nonce, plaintext, ciphertext, out tag);

            CollectionAssert.AreEqual(expectedCiphertext, ciphertext);
            CollectionAssert.AreEqual(expectedTag, tag);
            CollectionAssert.AreEqual(plaintext, ManagedGcm.Decrypt(key, nonce, ciphertext, tag));
        }

        [Test]
        public void Kat_NistSp80038dAes256_EmptyPlaintext()
        {
            AssertKnownAnswer(
                ZeroKeyHex,
                ZeroNonceHex,
                string.Empty,
                string.Empty,
                "530f8afbc74536b9a963b4f1c4cb738b",
                0);
        }

        [Test]
        public void Kat_NistSp80038dAes256_SingleBlockPlaintext()
        {
            AssertKnownAnswer(
                ZeroKeyHex,
                ZeroNonceHex,
                "00000000000000000000000000000000",
                "cea7403d4d606b6e074ec5d3baf39d18",
                "d0d1c8a799996bf0265b98b5d48ab919",
                16);
        }

        [Test]
        public void Kat_NistSp80038dAes256_FourBlockPlaintextWithoutAad()
        {
            AssertKnownAnswer(
                SpecKeyHex,
                SpecNonceHex,
                SpecFourBlockPlaintextHex,
                SpecFourBlockCiphertextHex,
                "b094dac5d93471bdec1a502270e3cc6c",
                64);
        }

        [Test]
        public void Kat_NistCavpAes256_EmptyPlaintextWith96BitIv()
        {
            AssertKnownAnswer(
                "b52c505a37d78eda5dd34f20c22540ea1b58963cf8e5bf8ffa85f9f2492505b4",
                "516c33929df5a3284ff463d7",
                string.Empty,
                string.Empty,
                "bdc1ac884d332457a1d2664f168c76f0",
                0);
        }

        [Test]
        public void Kat_OpenSslCrossCheckAes256_NonBlockAlignedPlaintext()
        {
            AssertKnownAnswer(
                SpecKeyHex,
                SpecNonceHex,
                NonAlignedPlaintextHex,
                NonAlignedCiphertextHex,
                "780801a7427cca00079f561f0bdb403d",
                55);
        }

        [Test]
        public void Decrypt_FlippedTagBit_ThrowsCryptographicException()
        {
            byte[] key = Hex(SpecKeyHex);
            byte[] nonce = Hex(SpecNonceHex);
            byte[] ciphertext = Hex(SpecFourBlockCiphertextHex);
            byte[] tag = Hex("b094dac5d93471bdec1a502270e3cc6c");

            tag[0] ^= 0x01;

            Assert.Throws<CryptographicException>(() => ManagedGcm.Decrypt(key, nonce, ciphertext, tag));
        }

        [Test]
        public void Decrypt_FlippedCiphertextBit_ThrowsCryptographicException()
        {
            byte[] key = Hex(SpecKeyHex);
            byte[] nonce = Hex(SpecNonceHex);
            byte[] ciphertext = Hex(SpecFourBlockCiphertextHex);
            byte[] tag = Hex("b094dac5d93471bdec1a502270e3cc6c");

            ciphertext[ciphertext.Length - 1] ^= 0x80;

            Assert.Throws<CryptographicException>(() => ManagedGcm.Decrypt(key, nonce, ciphertext, tag));
        }

        [Test]
        public void Decrypt_ShortTag_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ManagedGcm.Decrypt(
                Hex(SpecKeyHex), Hex(SpecNonceHex), new byte[8], new byte[15]));
        }

        [Test]
        public void Decrypt_Non96BitNonce_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ManagedGcm.Decrypt(
                Hex(SpecKeyHex), new byte[8], new byte[8], new byte[16]));
        }

        [Test]
        public void Encrypt_CiphertextBufferLengthMismatch_ThrowsArgumentException()
        {
            byte[] tag;
            Assert.Throws<ArgumentException>(() => ManagedGcm.Encrypt(
                Hex(SpecKeyHex), Hex(SpecNonceHex), new byte[16], new byte[15], out tag));
        }

        [Test]
        public void Decrypt_FrozenBlobLayout_MatchesOpenSslAes256Gcm()
        {
            byte[] key = Hex("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
            byte[] blob = Hex(
                "000102030405060708090a0bf206ad97a3cfdebfc02f05a9ee43d07b225369fbbdae126e63" +
                "49db2d00b8a6df09f4d5f410f11f10f7681bcf33af5755a6cedd922ae48d01543b6a4ed9");
            const string plaintext = "Schlusselbild QR channel: frozen format check";

            Assert.That(blob.Length, Is.EqualTo(12 + 16 + 45));
            Assert.That(AesGcmCodec.Decrypt(blob, key), Is.EqualTo(plaintext));
        }
    }
}
