using NUnit.Framework;
using Schlusselbild.Ui;

namespace Schlusselbild.Tests
{
    public class KeypadModelTests
    {
        [Test]
        public void Append_AddsTextAndRaisesChanged()
        {
            KeypadModel model = new KeypadModel();
            string reported = null;
            int changes = 0;
            model.TextChanged += delegate(string value)
            {
                reported = value;
                changes++;
            };

            Assert.That(model.Append("A"), Is.True);
            Assert.That(model.Text, Is.EqualTo("A"));
            Assert.That(reported, Is.EqualTo("A"));
            Assert.That(changes, Is.EqualTo(1));

            Assert.That(model.Append(KeypadModel.SpaceKey), Is.True);
            Assert.That(model.Append("b"), Is.True);
            Assert.That(model.Text, Is.EqualTo("A b"));
            Assert.That(changes, Is.EqualTo(3));
        }

        [Test]
        public void Append_IgnoresEmptyValues()
        {
            KeypadModel model = new KeypadModel();
            int changes = 0;
            model.TextChanged += delegate { changes++; };

            Assert.That(model.Append((string)null), Is.False);
            Assert.That(model.Append(string.Empty), Is.False);
            Assert.That(model.Length, Is.EqualTo(0));
            Assert.That(changes, Is.EqualTo(0));
        }

        [Test]
        public void Append_StopsAtMaxLength()
        {
            KeypadModel model = new KeypadModel(4);

            Assert.That(model.Append("abcdef"), Is.True);
            Assert.That(model.Text, Is.EqualTo("abcd"));
            Assert.That(model.Length, Is.EqualTo(4));
            Assert.That(model.IsFull, Is.True);

            int changes = 0;
            model.TextChanged += delegate { changes++; };
            Assert.That(model.Append("z"), Is.False);
            Assert.That(changes, Is.EqualTo(0));
        }

        [Test]
        public void Backspace_RemovesLastCharacter()
        {
            KeypadModel model = new KeypadModel();
            model.Append("abc");

            Assert.That(model.Backspace(), Is.True);
            Assert.That(model.Text, Is.EqualTo("ab"));

            Assert.That(model.Backspace(), Is.True);
            Assert.That(model.Backspace(), Is.True);
            Assert.That(model.Text, Is.EqualTo(string.Empty));
            Assert.That(model.Backspace(), Is.False);
        }

        [Test]
        public void Clear_EmptiesTheBufferOnce()
        {
            KeypadModel model = new KeypadModel();
            model.Append("secret");

            Assert.That(model.Clear(), Is.True);
            Assert.That(model.Text, Is.EqualTo(string.Empty));
            Assert.That(model.Clear(), Is.False);
        }

        [Test]
        public void ToggleMode_SwitchesLettersAndDigits()
        {
            KeypadModel model = new KeypadModel();

            Assert.That(model.Mode, Is.EqualTo(KeypadMode.Letters));
            Assert.That(model.ToggleMode(), Is.EqualTo(KeypadMode.Digits));
            Assert.That(model.ToggleMode(), Is.EqualTo(KeypadMode.Letters));
        }

        [Test]
        public void CurrentKeys_FollowTheMode()
        {
            KeypadModel model = new KeypadModel();

            string[] letters = model.CurrentKeys();
            CollectionAssert.Contains(letters, "A");
            CollectionAssert.Contains(letters, "Z");
            CollectionAssert.Contains(letters, ".");
            CollectionAssert.DoesNotContain(letters, "0");
            Assert.That(letters.Length, Is.EqualTo(KeypadModel.LetterKeys.Length + KeypadModel.SymbolKeys.Length));

            model.ToggleMode();
            string[] digits = model.CurrentKeys();
            CollectionAssert.Contains(digits, "0");
            CollectionAssert.Contains(digits, "9");
            CollectionAssert.DoesNotContain(digits, "A");
            Assert.That(digits.Length, Is.EqualTo(KeypadModel.DigitKeys.Length + KeypadModel.SymbolKeys.Length));
        }

        [Test]
        public void SetText_ReplacesAndClampsToMaxLength()
        {
            KeypadModel model = new KeypadModel(6);
            model.Append("old");

            Assert.That(model.SetText("hello"), Is.True);
            Assert.That(model.Text, Is.EqualTo("hello"));

            Assert.That(model.SetText("toolongvalue"), Is.True);
            Assert.That(model.Text, Is.EqualTo("toolon"));

            Assert.That(model.SetText("toolon"), Is.False);
        }

        [Test]
        public void DefaultMaxLength_IsLargeEnoughForASecret()
        {
            Assert.That(new KeypadModel().MaxLength, Is.EqualTo(KeypadModel.DefaultMaxLength));
            Assert.That(new KeypadModel().MaxLength, Is.GreaterThanOrEqualTo(1024));
        }
    }
}