using System;
using System.IO;
using System.Security.Cryptography;
using Schlusselbild.Crypto;
using UnityEngine;

namespace Schlusselbild.Capture
{
    public sealed class PassthroughScanner : MonoBehaviour
    {
        private const string ScanningStatus = "Scanning\u2026";
        private const string WaitingStatus = "Waiting for the passthrough camera\u2026";
        private const string UnavailableStatus = "Passthrough camera not available.";
        private const string InvalidStatus = "Page found but the key is wrong.";
        private const string DecodedStatus = "Decoded.";
        private const string StoppedStatus = "Stopped.";
        private const string KeyMissingStatus = "Encryption key unavailable.";

        [SerializeField] private int decodeEvery = 2;
        [SerializeField] private int maxLongEdge = 900;
        [SerializeField] private byte[] key;
        [SerializeField] private string keyPath = "";
        [SerializeField] private Meta.XR.PassthroughCameraAccess cameraAccess;

        public event Action<string> StatusChanged;
        public event Action<string> Decoded;
        public event Action Invalid;
        public event Action<byte[]> FrameReady;

        private RenderTexture readback;
        private Texture2D readbackTexture;
        private int frameCounter;

        public bool IsScanning { get; private set; }

        public byte[] Key
        {
            get { return key; }
            set { key = value; }
        }

        public Meta.XR.PassthroughCameraAccess CameraAccess
        {
            get { return cameraAccess; }
            set { cameraAccess = value; }
        }

        private void Start()
        {
            ResolveKey();

            if (cameraAccess == null)
            {
                cameraAccess = UnityEngine.Object.FindFirstObjectByType<Meta.XR.PassthroughCameraAccess>(
                    FindObjectsInactive.Include);
            }

            if (cameraAccess == null)
            {
                Debug.LogWarning("[PassthroughScanner] No " + nameof(Meta.XR.PassthroughCameraAccess) +
                    " found in the scene. Scanning stays idle until one is available.");
            }

            BeginScan();
        }

        private void OnDestroy()
        {
            ReleaseReadback();
        }

        public void BeginScan()
        {
            IsScanning = true;
            frameCounter = 0;
            SetStatus(cameraAccess == null ? UnavailableStatus : ScanningStatus);
        }

        public void StopScan()
        {
            IsScanning = false;
            SetStatus(StoppedStatus);
        }

        private void Update()
        {
            if (!IsScanning)
            {
                return;
            }

            if (cameraAccess == null)
            {
                SetStatus(UnavailableStatus);
                return;
            }

            if (!cameraAccess.IsPlaying)
            {
                SetStatus(WaitingStatus);
                return;
            }

            frameCounter++;
            if (decodeEvery > 1 && frameCounter % decodeEvery != 0)
            {
                return;
            }

            try
            {
                Scan();
            }
            catch (Exception error)
            {
                SetStatus(ScanningStatus);
                Debug.LogWarning("[PassthroughScanner] " + error.Message);
            }
        }

        private void Scan()
        {
            if (key == null)
            {
                SetStatus(KeyMissingStatus);
                return;
            }

            Texture source = cameraAccess.GetTexture();
            if (source == null)
            {
                SetStatus(WaitingStatus);
                return;
            }

            Vector2Int resolution = cameraAccess.CurrentResolution;
            int width = resolution.x > 0 ? resolution.x : source.width;
            int height = resolution.y > 0 ? resolution.y : source.height;
            if (width <= 0 || height <= 0)
            {
                SetStatus(WaitingStatus);
                return;
            }

            Color32[] pixels = ReadPixels(source, width, height);
            if (pixels == null)
            {
                SetStatus(WaitingStatus);
                return;
            }

            Action<byte[]> frameReady = FrameReady;
            if (frameReady != null)
            {
                frameReady(ToRgbaBytes(pixels));
            }

            try
            {
                string text = FrameDecoder.Decode(pixels, width, height, key, maxLongEdge);
                IsScanning = false;
                SetStatus(DecodedStatus);

                Action<string> decoded = Decoded;
                if (decoded != null)
                {
                    decoded(text);
                }
            }
            catch (CryptographicException)
            {
                SetStatus(InvalidStatus);

                Action invalid = Invalid;
                if (invalid != null)
                {
                    invalid();
                }
            }
            catch (Exception)
            {
                SetStatus(ScanningStatus);
            }
        }

        private Color32[] ReadPixels(Texture source, int width, int height)
        {
            EnsureReadback(width, height);

            RenderTexture previous = RenderTexture.active;
            try
            {
                Graphics.Blit(source, readback);
                RenderTexture.active = readback;
                readbackTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                readbackTexture.Apply(false, false);
                return readbackTexture.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
            }
        }

        private void EnsureReadback(int width, int height)
        {
            if (readback != null && readbackTexture != null &&
                readback.width == width && readback.height == height)
            {
                return;
            }

            ReleaseReadback();

            readback = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                useMipMap = false,
                autoGenerateMips = false
            };
            readback.Create();
            readbackTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        private void ReleaseReadback()
        {
            if (readbackTexture != null)
            {
                DestroySafely(readbackTexture);
                readbackTexture = null;
            }

            if (readback != null)
            {
                readback.Release();
                DestroySafely(readback);
                readback = null;
            }
        }

        private void ResolveKey()
        {
            if (key != null && key.Length == KeyStore.KeySize)
            {
                return;
            }

            string path = string.IsNullOrEmpty(keyPath)
                ? Path.Combine(Application.persistentDataPath, "key.bin")
                : keyPath;

            try
            {
                key = KeyStore.LoadOrCreate(path);
            }
            catch (Exception error)
            {
                Debug.LogWarning("[PassthroughScanner] Could not load or create the key: " + error.Message);
            }
        }

        private void SetStatus(string message)
        {
            Action<string> handler = StatusChanged;
            if (handler != null)
            {
                handler(message);
            }
        }

        private static byte[] ToRgbaBytes(Color32[] pixels)
        {
            byte[] bytes = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                int offset = i * 4;
                bytes[offset] = pixel.r;
                bytes[offset + 1] = pixel.g;
                bytes[offset + 2] = pixel.b;
                bytes[offset + 3] = pixel.a;
            }

            return bytes;
        }

        private static void DestroySafely(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
