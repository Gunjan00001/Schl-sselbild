using System;

namespace Schlusselbild.Crypto
{
    public static class KeyImporter
    {
        private const int KeySize = 32;
        private const int HexLength = KeySize * 2;

        public static bool TryParse(string text, out byte[] key, out string error)
        {
            key = null;
            error = null;

            if (text == null)
            {
                error = "Key text is null.";
                return false;
            }

            string value = text.Trim();
            if (value.Length == 0)
            {
                error = "Key text is empty.";
                return false;
            }

            if (value.Length == HexLength)
            {
                byte[] hex = ParseHex(value);
                if (hex != null)
                {
                    key = hex;
                    return true;
                }

                error = $"Key must be {HexLength} hexadecimal characters or base64 for {KeySize} bytes.";
                return false;
            }

            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                error = $"Key must be {HexLength} hexadecimal characters or base64 for {KeySize} bytes.";
                return false;
            }

            if (decoded.Length != KeySize)
            {
                error = $"Base64 key decodes to {decoded.Length} bytes, expected {KeySize}.";
                return false;
            }

            key = decoded;
            return true;
        }

        private static byte[] ParseHex(string value)
        {
            byte[] result = new byte[value.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                int high = HexDigit(value[i * 2]);
                int low = HexDigit(value[i * 2 + 1]);
                if (high < 0 || low < 0)
                {
                    return null;
                }

                result[i] = (byte)((high << 4) | low);
            }

            return result;
        }

        private static int HexDigit(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return value - '0';
            }

            if (value >= 'a' && value <= 'f')
            {
                return value - 'a' + 10;
            }

            if (value >= 'A' && value <= 'F')
            {
                return value - 'A' + 10;
            }

            return -1;
        }
    }
}
