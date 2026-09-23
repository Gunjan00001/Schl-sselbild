using System;
using System.Collections.Generic;
using UnityEngine;
using Schlusselbild.Crypto;
using Schlusselbild.Qr;

namespace Schlusselbild.Page
{
    public static class PageDecoder
    {
        public static string Decode(Texture2D page, byte[] key)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            IQrDecoder decoder = new ZxingQrDecoder();
            List<byte[]> envelopes = decoder.DecodeAll(page);
            byte[] blob = QrEnvelope.Reassemble(envelopes);
            return AesGcmCodec.Decrypt(blob, key);
        }
    }
}
