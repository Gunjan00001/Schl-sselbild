using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Schlusselbild.Page;

namespace Schlusselbild.Tests
{
    public class PageFixtureTests
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

        [Test]
        public void DecodePageShort_MatchesSecretShort()
        {
            byte[] key = KeyFromHex(File.ReadAllText(FixturePath("key.hex")));
            string expected = File.ReadAllText(FixturePath("secret_short.txt"));

            string actual = PageDecoder.Decode(LoadFixturePage("page_short.png"), key);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodePageMulti_MatchesSecretMulti()
        {
            byte[] key = KeyFromHex(File.ReadAllText(FixturePath("key.hex")));
            string expected = File.ReadAllText(FixturePath("secret_multi.txt"));

            string actual = PageDecoder.Decode(LoadFixturePage("page_multi.png"), key);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DecodePageShort_WithWrongKey_Fails()
        {
            byte[] key = KeyFromHex(File.ReadAllText(FixturePath("key.hex")));
            byte[] other = new byte[key.Length];
            Buffer.BlockCopy(key, 0, other, 0, key.Length);
            other[0] ^= 0xFF;

            Assert.Catch<System.Security.Cryptography.CryptographicException>(
                () => PageDecoder.Decode(LoadFixturePage("page_short.png"), other));
        }
    }
}
