using System;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Schlusselbild.Qr
{
    public sealed class ZxingQrEncoder : IQrEncoder
    {
        public const int DefaultModuleSize = 10;
        public const int QuietZoneModules = 4;
        public const string ByteModeCharacterSet = "ISO-8859-1";

        private static readonly Color32 Module = new Color32(0, 0, 0, 255);
        private static readonly Color32 Empty = new Color32(255, 255, 255, 255);

        private readonly int moduleSize;

        public ZxingQrEncoder()
            : this(DefaultModuleSize)
        {
        }

        public ZxingQrEncoder(int moduleSize)
        {
            this.moduleSize = moduleSize < 1 ? 1 : moduleSize;
        }

        public int ModuleSize
        {
            get { return moduleSize; }
        }

        public Texture2D Encode(byte[] payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            BitMatrix matrix = EncodeMatrix(payload);
            return Render(matrix, moduleSize);
        }

        internal static BitMatrix EncodeMatrix(byte[] payload)
        {
            Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            hints[EncodeHintType.ERROR_CORRECTION] = ErrorCorrectionLevel.H;
            hints[EncodeHintType.CHARACTER_SET] = ByteModeCharacterSet;
            hints[EncodeHintType.MARGIN] = QuietZoneModules;
            hints[EncodeHintType.DISABLE_ECI] = true;

            QRCodeWriter writer = new QRCodeWriter();
            return writer.encode(ToLatin1String(payload), BarcodeFormat.QR_CODE, 0, 0, hints);
        }

        internal static bool[,] EncodeModules(byte[] payload)
        {
            BitMatrix matrix = EncodeMatrix(payload);
            bool[,] modules = new bool[matrix.Width, matrix.Height];
            for (int y = 0; y < matrix.Height; y++)
            {
                for (int x = 0; x < matrix.Width; x++)
                {
                    modules[x, y] = matrix[x, y];
                }
            }

            return modules;
        }

        internal static string ToLatin1String(byte[] payload)
        {
            char[] characters = new char[payload.Length];
            for (int i = 0; i < payload.Length; i++)
            {
                characters[i] = (char)payload[i];
            }

            return new string(characters);
        }

        internal static Texture2D Render(BitMatrix matrix, int moduleSize)
        {
            int width = matrix.Width * moduleSize;
            int height = matrix.Height * moduleSize;
            Color32[] topDown = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                int moduleRow = y / moduleSize;
                int rowStart = y * width;
                for (int x = 0; x < width; x++)
                {
                    topDown[rowStart + x] = matrix[x / moduleSize, moduleRow] ? Module : Empty;
                }
            }

            return TexturePixels.CreateTopDown(width, height, topDown);
        }
    }
}
