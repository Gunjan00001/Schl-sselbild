using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Schlusselbild.Qr;

namespace Schlusselbild.Tests
{
    public class ZxingQrRoundTripTests
    {
        private readonly List<Texture2D> created = new List<Texture2D>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(created[i]);
                }
            }

            created.Clear();
        }

        private Texture2D Track(Texture2D texture)
        {
            created.Add(texture);
            return texture;
        }

        private static byte[] MakeBytes(int length, int seed)
        {
            byte[] bytes = new byte[length];
            new System.Random(seed).NextBytes(bytes);
            return bytes;
        }

        private byte[] RoundTripSingleQr(byte[] payload)
        {
            IQrEncoder encoder = new ZxingQrEncoder();
            IQrDecoder decoder = new ZxingQrDecoder();

            Texture2D texture = Track(encoder.Encode(payload));
            List<byte[]> results = decoder.DecodeAll(texture);

            Assert.That(results.Count, Is.EqualTo(1), "one symbol expected for a single-QR texture");
            return results[0];
        }

        private byte[] RoundTripTransport(byte[] payload)
        {
            IQrEncoder encoder = new ZxingQrEncoder();
            IQrDecoder decoder = new ZxingQrDecoder();

            List<byte[]> envelopes = QrEnvelope.Split(payload);
            List<byte[]> decoded = new List<byte[]>(envelopes.Count);
            for (int i = 0; i < envelopes.Count; i++)
            {
                Texture2D texture = Track(encoder.Encode(envelopes[i]));
                List<byte[]> results = decoder.DecodeAll(texture);
                Assert.That(results.Count, Is.EqualTo(1), "one symbol expected per envelope");
                decoded.Add(results[0]);
            }

            return QrEnvelope.Reassemble(decoded);
        }

        [TestCase(4)]
        [TestCase(8)]
        [TestCase(32)]
        [TestCase(255)]
        [TestCase(256)]
        [TestCase(260)]
        [TestCase(1000)]
        public void SingleQr_ByteArray_RoundTrips(int length)
        {
            byte[] payload = MakeBytes(length, 100 + length);

            CollectionAssert.AreEqual(payload, RoundTripSingleQr(payload));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(256)]
        [TestCase(257)]
        [TestCase(1000)]
        [TestCase(5000)]
        public void Transport_ByteArray_RoundTrips(int length)
        {
            byte[] payload = MakeBytes(length, 1000 + length);

            CollectionAssert.AreEqual(payload, RoundTripTransport(payload));
        }

        [Test]
        public void Transport_Utf8Strings_RoundTrips()
        {
            string[] secrets =
            {
                "meet at dawn - 0400 pier 7",
                "Schlüsselbild – naïve café – 日本語 – 🔑 – Ключ",
                string.Empty,
                new string('x', 600)
            };

            for (int i = 0; i < secrets.Length; i++)
            {
                byte[] payload = Encoding.UTF8.GetBytes(secrets[i]);

                CollectionAssert.AreEqual(payload, RoundTripTransport(payload), "secret " + i);
            }
        }

        [TestCase(4)]
        [TestCase(58)]
        [TestCase(260)]
        public void EncodeMatrix_ProducesStandardSymbolWithQuietZone(int length)
        {
            byte[] payload = MakeBytes(length, 77 + length);

            bool[,] matrix = ZxingQrEncoder.EncodeModules(payload);
            int matrixWidth = matrix.GetLength(0);
            int matrixHeight = matrix.GetLength(1);

            Assert.That(matrixWidth, Is.EqualTo(matrixHeight));
            Assert.That(matrixWidth, Is.GreaterThan(2 * ZxingQrEncoder.QuietZoneModules));

            int quiet = ZxingQrEncoder.QuietZoneModules;
            int modules = matrixWidth - 2 * quiet;
            Assert.That(modules, Is.GreaterThanOrEqualTo(21));
            Assert.That((modules - 21) % 4, Is.EqualTo(0), "QR symbols are 21 + 4k modules wide");

            for (int i = 0; i < matrixWidth; i++)
            {
                Assert.That(matrix[i, 0], Is.False, "top quiet zone must be light");
                Assert.That(matrix[i, matrixHeight - 1], Is.False, "bottom quiet zone must be light");
                Assert.That(matrix[0, i], Is.False, "left quiet zone must be light");
                Assert.That(matrix[matrixWidth - 1, i], Is.False, "right quiet zone must be light");
            }

            AssertFinderPattern(matrix, quiet, quiet);
            AssertFinderPattern(matrix, quiet + modules - 7, quiet);
            AssertFinderPattern(matrix, quiet, quiet + modules - 7);
        }

        private static void AssertFinderPattern(bool[,] matrix, int left, int top)
        {
            for (int i = 0; i < 7; i++)
            {
                Assert.That(matrix[left + i, top], Is.True, "finder ring must be dark");
                Assert.That(matrix[left + i, top + 6], Is.True, "finder ring must be dark");
                Assert.That(matrix[left, top + i], Is.True, "finder ring must be dark");
                Assert.That(matrix[left + 6, top + i], Is.True, "finder ring must be dark");
            }

            for (int y = 1; y < 6; y++)
            {
                for (int x = 1; x < 6; x++)
                {
                    bool expected = x >= 2 && x <= 4 && y >= 2 && y <= 4;
                    Assert.That(matrix[left + x, top + y], Is.EqualTo(expected));
                }
            }
        }
    }
}
