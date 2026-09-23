using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using Schlusselbild.Capture;

namespace Schlusselbild.Tests
{
    public class FrameDecoderTests
    {
        private const string FixturesFolder = "/Schlusselbild/Tests/EditMode/Fixtures/";

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

        private static string FixturePath(string fileName)
        {
            return Application.dataPath + FixturesFolder + fileName;
        }

        private Texture2D LoadFixturePage(string fileName)
        {
            string path = FixturePath(fileName);
            Assert.That(File.Exists(path), Is.True, "fixture not found: " + path);

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            created.Add(texture);

            Assert.That(ImageConversion.LoadImage(texture, bytes), Is.True, "could not decode image: " + fileName);
            return texture;
        }

        private static byte[] KeyFromHex(string hex)
        {
            Assert.That(hex, Is.Not.Null, "key.hex is missing");
            string value = hex.Trim();
            Assert.That(value.Length, Is.EqualTo(64), "fixture key must be 64 hexadecimal characters");

            byte[] key = new byte[32];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }

            return key;
        }

        private static byte[] FixtureKey()
        {
            return KeyFromHex(File.ReadAllText(FixturePath("key.hex")));
        }

        private static Color32[] FlipVertical(Color32[] pixels, int width, int height)
        {
            Color32[] flipped = new Color32[pixels.Length];
            for (int y = 0; y < height; y++)
            {
                int sourceRow = (height - 1 - y) * width;
                int targetRow = y * width;
                for (int x = 0; x < width; x++)
                {
                    flipped[targetRow + x] = pixels[sourceRow + x];
                }
            }

            return flipped;
        }

        [Test]
        public void DecodeTexture_PageShort_MatchesSecretShort()
        {
            byte[] key = FixtureKey();
            string expected = File.ReadAllText(FixturePath("secret_short.txt"));

            string actual = FrameDecoder.Decode(LoadFixturePage("page_short.png"), key);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeTexture_PageMulti_MatchesSecretMulti()
        {
            byte[] key = FixtureKey();
            string expected = File.ReadAllText(FixturePath("secret_multi.txt"));

            string actual = FrameDecoder.Decode(LoadFixturePage("page_multi.png"), key);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeTexture_PageShort_AtMaxLongEdge900_StillDecodes()
        {
            byte[] key = FixtureKey();
            string expected = File.ReadAllText(FixturePath("secret_short.txt"));

            string actual = FrameDecoder.Decode(LoadFixturePage("page_short.png"), key, 900);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeTexture_PageShort_AtMaxLongEdge600_StillDecodes()
        {
            byte[] key = FixtureKey();
            string expected = File.ReadAllText(FixturePath("secret_short.txt"));

            string actual = FrameDecoder.Decode(LoadFixturePage("page_short.png"), key, 600);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeTexture_PageShort_WithWrongKey_ThrowsCryptographicException()
        {
            byte[] key = FixtureKey();
            byte[] other = new byte[key.Length];
            Buffer.BlockCopy(key, 0, other, 0, key.Length);
            other[0] ^= 0xFF;

            Assert.Catch<CryptographicException>(
                () => FrameDecoder.Decode(LoadFixturePage("page_short.png"), other));
        }

        [Test]
        public void DecodePixels_PageShort_MatchesSecretShort()
        {
            byte[] key = FixtureKey();
            string expected = File.ReadAllText(FixturePath("secret_short.txt"));
            Texture2D page = LoadFixturePage("page_short.png");

            string actual = FrameDecoder.Decode(page.GetPixels32(), page.width, page.height, key);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodePixels_PageShort_TopDown_MatchesSecretShort()
        {
            byte[] key = FixtureKey();
            string expected = File.ReadAllText(FixturePath("secret_short.txt"));
            Texture2D page = LoadFixturePage("page_short.png");
            Color32[] topDown = FlipVertical(page.GetPixels32(), page.width, page.height);

            string actual = FrameDecoder.Decode(topDown, page.width, page.height, key);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
