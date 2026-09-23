using UnityEngine;

namespace Schlusselbild.Qr
{
    public interface IQrEncoder
    {
        Texture2D Encode(byte[] payload);
    }
}
