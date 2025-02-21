using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace HopiBot.Hack.Utils
{
    public class ScreenCapture
    {
        private readonly Device _device;
        private readonly OutputDuplication _outputDuplication;
        private readonly Texture2DDescription _textureDesc;

        public ScreenCapture()
        {
            var adapter = new Factory1().GetAdapter1(0);
            _device = new Device(adapter);
            var output = adapter.GetOutput(0);
            var output1 = output.QueryInterface<Output1>();
            var desktopBounds = output.Description.DesktopBounds;
            int width = desktopBounds.Right - desktopBounds.Left;
            int height = desktopBounds.Bottom - desktopBounds.Top;
            _textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            _outputDuplication = output1.DuplicateOutput(_device);
        }

        public async Task CaptureScreenPeriodically(int intervalInMilliseconds, Action<Bitmap> onCapture)
        {
            while (true)
            {
                var bitmap = await CaptureScreenAsync();
                onCapture?.Invoke(bitmap);
                await Task.Delay(intervalInMilliseconds);
            }
        }

        private Task<Bitmap> CaptureScreenAsync()
        {
            return Task.Run(CaptureScreen);
        }

        private Bitmap CaptureScreen()
        {
            SharpDX.DXGI.Resource desktopResource;

            _outputDuplication.AcquireNextFrame(1000, out _, out desktopResource);
            using (var screenTexture = desktopResource.QueryInterface<Texture2D>())
            {
                var stagingTexture = new Texture2D(_device, _textureDesc);
                _device.ImmediateContext.CopyResource(screenTexture, stagingTexture);

                var mapSource = _device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

                var bitmap = new Bitmap(_textureDesc.Width, _textureDesc.Height, PixelFormat.Format32bppArgb);
                var boundsRect = new Rectangle(0, 0, _textureDesc.Width, _textureDesc.Height);

                var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;

                for (int y = 0; y < _textureDesc.Height; y++)
                {
                    Utilities.CopyMemory(destPtr, sourcePtr, _textureDesc.Width * 4);
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                bitmap.UnlockBits(mapDest);
                _device.ImmediateContext.UnmapSubresource(stagingTexture, 0);

                _outputDuplication.ReleaseFrame();
                return bitmap;
            }
        }
    }
}
