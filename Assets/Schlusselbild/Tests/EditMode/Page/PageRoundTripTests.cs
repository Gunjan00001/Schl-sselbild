using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Schlusselbild.Page;

namespace Schlusselbild.Tests
{
    public class PageRoundTripTests
    {
        private static readonly byte[] Key = CreateKey();

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

        private static byte[] CreateKey()
        {
            byte[] key = new byte[32];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (byte)((i * 7 + 3) & 0xFF);
            }

            return key;
        }

        private static string BuildSecret(int length)
        {
            StringBuilder builder = new StringBuilder();
            while (builder.Length < length)
            {
                builder.Append("Schlusselbild payload block 0123456789 - ");
            }

            return builder.ToString(0, length);
        }

        private Texture2D Compose(string secret)
        {
            Texture2D page = PageComposer.Compose(secret, Key);
            created.Add(page);
            return page;
        }

        [Test]
        public void ComposeThenDecode_ShortSecret_RoundTrips()
        {
            const string secret = "meet at dawn - 0400 pier 7";

            Assert.That(PageDecoder.Decode(Compose(secret), Key), Is.EqualTo(secret));
        }

        [Test]
        public void ComposeThenDecode_UnicodeSecret_RoundTrips()
        {
            const string secret = "Schlüsselbild – naïve café – 日本語 – 🔑 – Ключ – صفحه";

            Assert.That(PageDecoder.Decode(Compose(secret), Key), Is.EqualTo(secret));
        }

        [Test]
        public void ComposeThenDecode_EmptySecret_RoundTrips()
        {
            Assert.That(PageDecoder.Decode(Compose(string.Empty), Key), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ComposeThenDecode_MultiChunkSecret_RoundTrips()
        {
            string secret = BuildSecret(1024);

            Assert.That(PageDecoder.Decode(Compose(secret), Key), Is.EqualTo(secret));
        }

        [Test]
        public void Compose_CustomTitle_RoundTrips()
        {
            const string secret = "title is cosmetic but must not break the grid";

            Texture2D page = PageComposer.Compose(secret, Key, "Kitsune");
            created.Add(page);

            Assert.That(PageDecoder.Decode(page, Key), Is.EqualTo(secret));
        }

        [Test]
        public void Compose_SingleChunkSecret_ProducesTallerThanWidePage()
        {
            Texture2D page = Compose("meet at dawn - 0400 pier 7");

            Assert.That(page.width, Is.LessThan(page.height));
            AssertWhite(page, 0, 0);
            AssertWhite(page, page.width - 1, 0);
            AssertWhite(page, 0, page.height - 1);
            AssertWhite(page, page.width - 1, page.height - 1);
        }

        [Test]
        public void Compose_MultiChunkSecret_ProducesWiderThanTallGrid()
        {
            Texture2D page = Compose(BuildSecret(1024));

            Assert.That(page.width, Is.GreaterThan(page.height));
            AssertWhite(page, 0, 0);
            AssertWhite(page, page.width - 1, page.height - 1);
        }

        private static void AssertWhite(Texture2D page, int x, int y)
        {
            Color32 pixel = page.GetPixels32()[y * page.width + x];
            Assert.That(pixel.r, Is.EqualTo(255));
            Assert.That(pixel.g, Is.EqualTo(255));
            Assert.That(pixel.b, Is.EqualTo(255));
        }
    }
}
