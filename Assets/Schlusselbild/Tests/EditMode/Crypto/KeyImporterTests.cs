using System;
using NUnit.Framework;
using Schlusselbild.Crypto;

namespace Schlusselbild.Tests.Crypto
{
    public class KeyImporterTests
    {
        private const string UpperHex = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        private const string LowerHex = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";

        [Test]
        public void TryParse_64CharHex_ParsesTo32Bytes()
        {
            bool parsed = KeyImporter.TryParse(UpperHex, out byte[] key, out string error);

            Assert.That(parsed, Is.True);
            Assert.That(error, Is.Null);
            Assert.That(key, Is.Not.Null);
            Assert.That(key.Length, Is.EqualTo(32));
            for (int i = 0; i < key.Length; i++)
            {
                Assert.That(key[i], Is.EqualTo((byte)i), $"byte {i}");
            }
        }

        [Test]
        public void TryParse_Hex_IsCaseInsensitive()
        {
            Assert.That(KeyImporter.TryParse(UpperHex, out byte[] upper, out _), Is.True);
            Assert.That(KeyImporter.TryParse(LowerHex, out byte[] lower, out _), Is.True);

            CollectionAssert.AreEqual(upper, lower);
        }

        [Test]
        public void TryParse_Base64_ParsesTo32Bytes()
        {
            byte[] expected = new byte[32];
            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = (byte)(255 - i);
            }

            string base64 = Convert.ToBase64String(expected);

            bool parsed = KeyImporter.TryParse(base64, out byte[] key, out string error);

            Assert.That(parsed, Is.True);
            Assert.That(error, Is.Null);
            Assert.That(key, Is.Not.Null);
            CollectionAssert.AreEqual(expected, key);
        }

        [Test]
        public void TryParse_WrongLength_IsRejected()
        {
            const string shortHex = "000102030405060708090A0B0C0D0E0F";
            string shortBase64 = Convert.ToBase64String(new byte[16]);

            Assert.That(KeyImporter.TryParse(shortHex, out byte[] hexKey, out string hexError), Is.False);
            Assert.That(hexKey, Is.Null);
            Assert.That(hexError, Is.Not.Null);
            Assert.That(hexError, Is.Not.Empty);

            Assert.That(KeyImporter.TryParse(shortBase64, out byte[] base64Key, out string base64Error), Is.False);
            Assert.That(base64Key, Is.Null);
            Assert.That(base64Error, Is.Not.Null);
            Assert.That(base64Error, Is.Not.Empty);
        }

        [Test]
        public void TryParse_InvalidCharacters_IsRejected()
        {
            const string notABase64Key = "not-a-valid-key-string!";
            string longNonHex = new string('g', 64);

            Assert.That(KeyImporter.TryParse(notABase64Key, out byte[] key, out string error), Is.False);
            Assert.That(key, Is.Null);
            Assert.That(error, Is.Not.Null);
            Assert.That(error, Is.Not.Empty);

            Assert.That(KeyImporter.TryParse(longNonHex, out byte[] nonHexKey, out string nonHexError), Is.False);
            Assert.That(nonHexKey, Is.Null);
            Assert.That(nonHexError, Is.Not.Null);
            Assert.That(nonHexError, Is.Not.Empty);
        }

        [Test]
        public void TryParse_SurroundingWhitespace_IsTrimmed()
        {
            string paddedHex = "\r\n\t   " + UpperHex + "  \n";
            string paddedBase64 = " \t " + Convert.ToBase64String(new byte[32]) + "\r\n";

            Assert.That(KeyImporter.TryParse(paddedHex, out byte[] hexKey, out string hexError), Is.True);
            Assert.That(hexError, Is.Null);
            Assert.That(hexKey.Length, Is.EqualTo(32));
            Assert.That(hexKey[31], Is.EqualTo(0x1F));

            Assert.That(KeyImporter.TryParse(paddedBase64, out byte[] base64Key, out string base64Error), Is.True);
            Assert.That(base64Error, Is.Null);
            Assert.That(base64Key.Length, Is.EqualTo(32));
        }
    }
}
