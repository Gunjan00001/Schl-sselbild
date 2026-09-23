using System;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.Common;
using ZXing.Multi.QrCode;

namespace Schlusselbild.Qr
{
    public sealed class ZxingQrDecoder : IQrDecoder
    {
        private const int MinimumResultLength = QrEnvelope.HeaderSize;

        public List<byte[]> DecodeAll(Texture2D image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int width = image.width;
            int height = image.height;
            Color32[] topDown = TexturePixels.ReadTopDown(image);

            byte[] luminance = new byte[topDown.Length];
            for (int i = 0; i < topDown.Length; i++)
            {
                Color32 pixel = topDown[i];
                luminance[i] = (byte)((pixel.r * 77 + pixel.g * 150 + pixel.b * 29) >> 8);
            }

            RGBLuminanceSource source = new RGBLuminanceSource(
                luminance, width, height, RGBLuminanceSource.BitmapFormat.Gray8);

            List<byte[]> payloads = new List<byte[]>();
            Collect(DecodeWithReader(source), payloads);
            if (payloads.Count == 0)
            {
                Collect(DecodeWithMultiReader(source), payloads);
            }

            return payloads;
        }

        private static Result[] DecodeWithReader(LuminanceSource source)
        {
            BarcodeReaderGeneric reader = new BarcodeReaderGeneric();
            reader.Options.PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE };
            reader.Options.TryHarder = true;
            reader.Options.CharacterSet = ZxingQrEncoder.ByteModeCharacterSet;
            reader.AutoRotate = true;

            try
            {
                return reader.DecodeMultiple(source);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Result[] DecodeWithMultiReader(LuminanceSource source)
        {
            Dictionary<DecodeHintType, object> hints = new Dictionary<DecodeHintType, object>();
            hints[DecodeHintType.POSSIBLE_FORMATS] = new List<BarcodeFormat> { BarcodeFormat.QR_CODE };
            hints[DecodeHintType.TRY_HARDER] = true;
            hints[DecodeHintType.CHARACTER_SET] = ZxingQrEncoder.ByteModeCharacterSet;

            try
            {
                BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
                return new QRCodeMultiReader().decodeMultiple(bitmap, hints);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void Collect(Result[] results, List<byte[]> payloads)
        {
            if (results == null)
            {
                return;
            }

            for (int i = 0; i < results.Length; i++)
            {
                Result result = results[i];
                if (result == null || result.Text == null)
                {
                    continue;
                }

                byte[] payload = FromLatin1String(result.Text);
                if (payload.Length < MinimumResultLength)
                {
                    continue;
                }

                payloads.Add(payload);
            }
        }

        internal static byte[] FromLatin1String(string text)
        {
            byte[] payload = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                payload[i] = (byte)(text[i] & 0xFF);
            }

            return payload;
        }
    }
}
