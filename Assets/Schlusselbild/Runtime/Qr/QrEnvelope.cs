using System;
using System.Collections.Generic;
using System.IO;

namespace Schlusselbild.Qr
{
    public static class QrEnvelope
    {
        public const int HeaderSize = 4;
        public const int DataPerChunk = 256;
        public const int MaxChunks = 255;

        public static List<byte[]> Split(byte[] blob)
        {
            if (blob == null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            int total = blob.Length == 0 ? 1 : (blob.Length + DataPerChunk - 1) / DataPerChunk;
            if (total > MaxChunks)
            {
                throw new ArgumentException(
                    $"payload needs {total} chunks but the envelope supports at most {MaxChunks}", nameof(blob));
            }

            List<byte[]> envelopes = new List<byte[]>(total);
            for (int index = 0; index < total; index++)
            {
                int offset = index * DataPerChunk;
                int length = blob.Length - offset;
                if (length < 0)
                {
                    length = 0;
                }
                else if (length > DataPerChunk)
                {
                    length = DataPerChunk;
                }

                byte[] envelope = new byte[HeaderSize + length];
                envelope[0] = (byte)index;
                envelope[1] = (byte)total;
                envelope[2] = (byte)((length >> 8) & 0xFF);
                envelope[3] = (byte)(length & 0xFF);
                if (length > 0)
                {
                    Buffer.BlockCopy(blob, offset, envelope, HeaderSize, length);
                }

                envelopes.Add(envelope);
            }

            return envelopes;
        }

        public static byte[] Reassemble(IEnumerable<byte[]> envelopes)
        {
            if (envelopes == null)
            {
                throw new ArgumentNullException(nameof(envelopes));
            }

            Dictionary<int, byte[]> frames = new Dictionary<int, byte[]>();
            int total = -1;
            foreach (byte[] envelope in envelopes)
            {
                if (envelope == null)
                {
                    throw new InvalidDataException("envelope is null");
                }

                if (envelope.Length < HeaderSize)
                {
                    throw new InvalidDataException(
                        $"envelope is {envelope.Length} bytes, shorter than the {HeaderSize}-byte header");
                }

                int index = envelope[0];
                int declaredTotal = envelope[1];
                int chunkLength = (envelope[2] << 8) | envelope[3];
                int actualLength = envelope.Length - HeaderSize;
                if (actualLength != chunkLength)
                {
                    throw new InvalidDataException(
                        $"chunk {index}: header declares {chunkLength} bytes but the envelope carries {actualLength}");
                }

                if (total < 0)
                {
                    total = declaredTotal;
                }
                else if (declaredTotal != total)
                {
                    throw new InvalidDataException(
                        $"chunk {index}: inconsistent total ({declaredTotal} vs {total})");
                }

                if (declaredTotal == 0)
                {
                    throw new InvalidDataException("envelope declares a total of zero chunks");
                }

                if (index >= declaredTotal)
                {
                    throw new InvalidDataException(
                        $"chunk {index}: index out of range for total {declaredTotal}");
                }

                byte[] chunk = new byte[actualLength];
                if (actualLength > 0)
                {
                    Buffer.BlockCopy(envelope, HeaderSize, chunk, 0, actualLength);
                }

                byte[] existing;
                if (frames.TryGetValue(index, out existing))
                {
                    if (!AreEqual(existing, chunk))
                    {
                        throw new InvalidDataException($"conflicting data for chunk index {index}");
                    }

                    continue;
                }

                frames.Add(index, chunk);
            }

            if (total < 0)
            {
                throw new ArgumentException("no envelopes supplied", nameof(envelopes));
            }

            List<int> missing = new List<int>();
            for (int index = 0; index < total; index++)
            {
                if (!frames.ContainsKey(index))
                {
                    missing.Add(index);
                }
            }

            if (missing.Count > 0)
            {
                throw new InvalidDataException($"missing chunk indices {string.Join(", ", missing)}");
            }

            int blobLength = 0;
            for (int index = 0; index < total; index++)
            {
                blobLength += frames[index].Length;
            }

            byte[] blob = new byte[blobLength];
            int offset = 0;
            for (int index = 0; index < total; index++)
            {
                byte[] chunk = frames[index];
                Buffer.BlockCopy(chunk, 0, blob, offset, chunk.Length);
                offset += chunk.Length;
            }

            return blob;
        }

        private static bool AreEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
