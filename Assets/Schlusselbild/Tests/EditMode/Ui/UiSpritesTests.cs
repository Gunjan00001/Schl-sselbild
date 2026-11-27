using NUnit.Framework;
using Schlusselbild.Ui;
using UnityEngine;

namespace Schlusselbild.Tests
{
    public class UiSpritesTests
    {
        [Test]
        public void VerticalGradient_FadesFromTopToBottom()
        {
            Sprite sprite = UiSprites.VerticalGradient();
            Assert.That(sprite, Is.Not.Null);

            Texture2D texture = sprite.texture;
            Color top = texture.GetPixel(0, texture.height - 1);
            Color bottom = texture.GetPixel(0, 0);

            Assert.That(top.a, Is.GreaterThan(bottom.a), "the sheen must be brighter at the top");
            Assert.That(bottom.a, Is.EqualTo(UiSprites.GradientBottomAlpha).Within(0.01f));
        }
    }
}
