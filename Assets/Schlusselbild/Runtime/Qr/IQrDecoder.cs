using System.Collections.Generic;
using UnityEngine;

namespace Schlusselbild.Qr
{
    public interface IQrDecoder
    {
        List<byte[]> DecodeAll(Texture2D image);
    }
}
