using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Schlusselbild.Qr;

namespace Schlusselbild.Tests
{
    public class QrEnvelopeTests
    {
        private static byte[] MakeBlob(int length, byte seed)
        {
            byte[] blob = new byte[length];
            for (int i = 0; i < length; i++)
            {
                blob[i] = (byte)((seed + i * 31) & 0xFF);
            }

            return blob;
        }

        private static byte[] Envelope(int index, int total, byte[] chunk)
        {
            byte[] envelope = new byte[QrEnvelope.HeaderSize + chunk.Length];
            envelope[0] = (byte)index;
            envelope[1] = (byte)total;
            envelope[2] = (byte)((chunk.Length >> 8) & 0xFF);
            envelope[3] = (byte)(chunk.Length & 0xFF);
            Buffer.BlockCopy(chunk, 0, envelope, QrEnvelope.HeaderSize, chunk.Length);
            return envelope;
        }

        [Test]
        public void Constants_MatchFrozenFormat()
        {
            Assert.That(QrEnvelope.HeaderSize, Is.EqualTo(4));
            Assert.That(QrEnvelope.DataPerChunk, Is.EqualTo(256));
            Assert.That(QrEnvelope.MaxChunks, Is.EqualTo(255));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(255)]
        [TestCase(256)]
        [TestCase(257)]
        [TestCase(512)]
        [TestCase(1000)]
        [TestCase(5000)]
        [TestCase(65280)]
        public void SplitThenReassemble_RoundTrips(int length)
        {
            byte[] blob = MakeBlob(length, 7);

            List<byte[]> envelopes = QrEnvelope.Split(blob);

            CollectionAssert.AreEqual(blob, QrEnvelope.Reassemble(envelopes));
        }

        [Test]
        public void Split_EmptyBlob_YieldsOneEnvelope()
        {
            List<byte[]> envelopes = QrEnvelope.Split(new byte[0]);

            Assert.That(envelopes.Count, Is.EqualTo(1));
            Assert.That(envelopes[0].Length, Is.EqualTo(QrEnvelope.HeaderSize));
            Assert.That(envelopes[0][0], Is.EqualTo(0));
            Assert.That(envelopes[0][1], Is.EqualTo(1));
            Assert.That(envelopes[0][2], Is.EqualTo(0));
            Assert.That(envelopes[0][3], Is.EqualTo(0));
        }

        [Test]
        public void Split_Header_IsIndexTotalAndBigEndianLength()
        {
            byte[] blob = MakeBlob(600, 3);

            List<byte[]> envelopes = QrEnvelope.Split(blob);

            Assert.That(envelopes.Count, Is.EqualTo(3));
            Assert.That(envelopes[0][0], Is.EqualTo(0));
            Assert.That(envelopes[1][0], Is.EqualTo(1));
            Assert.That(envelopes[2][0], Is.EqualTo(2));
            Assert.That(envelopes[0][1], Is.EqualTo(3));
            Assert.That(envelopes[1][1], Is.EqualTo(3));
            Assert.That(envelopes[2][1], Is.EqualTo(3));
            Assert.That(envelopes[0][2], Is.EqualTo(1));
            Assert.That(envelopes[0][3], Is.EqualTo(0));
            Assert.That(envelopes[0].Length, Is.EqualTo(QrEnvelope.HeaderSize + 256));
            Assert.That(envelopes[2].Length, Is.EqualTo(QrEnvelope.HeaderSize + 88));

            byte[] firstChunk = new byte[256];
            Buffer.BlockCopy(blob, 0, firstChunk, 0, 256);
            byte[] carried = new byte[256];
            Buffer.BlockCopy(envelopes[0], QrEnvelope.HeaderSize, carried, 0, 256);
            CollectionAssert.AreEqual(firstChunk, carried);
        }

        [Test]
        public void Split_BlobAboveMaxChunks_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => QrEnvelope.Split(new byte[QrEnvelope.MaxChunks * QrEnvelope.DataPerChunk + 1]));
        }

        [Test]
        public void Split_NullBlob_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => QrEnvelope.Split(null));
        }

        [Test]
        public void Reassemble_IsOrderIndependent()
        {
            byte[] blob = MakeBlob(1000, 11);
            List<byte[]> envelopes = QrEnvelope.Split(blob);

            List<byte[]> reversed = new List<byte[]>(envelopes);
            reversed.Reverse();

            CollectionAssert.AreEqual(blob, QrEnvelope.Reassemble(reversed));

            List<byte[]> shuffled = new List<byte[]> { envelopes[2], envelopes[0], envelopes[3], envelopes[1] };
            CollectionAssert.AreEqual(blob, QrEnvelope.Reassemble(shuffled));
        }

        [Test]
        public void Reassemble_MissingIndex_IsRejected()
        {
            byte[] blob = MakeBlob(1000, 13);
            List<byte[]> envelopes = QrEnvelope.Split(blob);
            envelopes.RemoveAt(1);

            InvalidDataException error = Assert.Throws<InvalidDataException>(() => QrEnvelope.Reassemble(envelopes));
            StringAssert.Contains("missing", error.Message);
        }

        [Test]
        public void Reassemble_ConflictingDuplicateIndex_IsRejected()
        {
            List<byte[]> envelopes = new List<byte[]>
            {
                Envelope(0, 2, MakeBlob(4, 21)),
                Envelope(1, 2, MakeBlob(4, 22)),
                Envelope(0, 2, MakeBlob(4, 23))
            };

            InvalidDataException error = Assert.Throws<InvalidDataException>(() => QrEnvelope.Reassemble(envelopes));
            StringAssert.Contains("conflicting", error.Message);
        }

        [Test]
        public void Reassemble_IdenticalDuplicateIndex_IsAccepted()
        {
            byte[] chunk = MakeBlob(4, 21);
            List<byte[]> envelopes = new List<byte[]>
            {
                Envelope(0, 2, chunk),
                Envelope(1, 2, MakeBlob(4, 22)),
                Envelope(0, 2, chunk)
            };

            byte[] expected = new byte[8];
            Buffer.BlockCopy(chunk, 0, expected, 0, 4);
            Buffer.BlockCopy(MakeBlob(4, 22), 0, expected, 4, 4);

            CollectionAssert.AreEqual(expected, QrEnvelope.Reassemble(envelopes));
        }

        [Test]
        public void Reassemble_InconsistentTotal_IsRejected()
        {
            byte[] blob = MakeBlob(1000, 17);
            List<byte[]> envelopes = QrEnvelope.Split(blob);
            envelopes[3][1] = 5;

            InvalidDataException error = Assert.Throws<InvalidDataException>(() => QrEnvelope.Reassemble(envelopes));
            StringAssert.Contains("inconsistent total", error.Message);
        }

        [Test]
        public void Reassemble_OutOfRangeIndex_IsRejected()
        {
            List<byte[]> envelopes = new List<byte[]> { Envelope(5, 2, MakeBlob(4, 31)) };

            InvalidDataException error = Assert.Throws<InvalidDataException>(() => QrEnvelope.Reassemble(envelopes));
            StringAssert.Contains("out of range", error.Message);
        }

        [Test]
        public void Reassemble_ShortEnvelope_IsRejected()
        {
            List<byte[]> envelopes = new List<byte[]> { new byte[] { 0, 1, 0 } };

            Assert.Throws<InvalidDataException>(() => QrEnvelope.Reassemble(envelopes));
        }

        [Test]
        public void Reassemble_LengthFieldMismatch_IsRejected()
        {
            List<byte[]> envelopes = new List<byte[]> { new byte[] { 0, 1, 0, 5, 0xAA } };

            InvalidDataException error = Assert.Throws<InvalidDataException>(() => QrEnvelope.Reassemble(envelopes));
            StringAssert.Contains("declares", error.Message);
        }

        [Test]
        public void Reassemble_NoEnvelopes_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => QrEnvelope.Reassemble(new List<byte[]>()));
        }

        [Test]
        public void Reassemble_NullSequence_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => QrEnvelope.Reassemble(null));
        }
    }
}
