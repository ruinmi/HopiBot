using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HopiBot.Hack.Utils;
using OpenCvSharp;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


namespace HopiBot.Hack
{
    public class OverlayForm : Form
    {
        private RECT _rect;

        // private const string WindowName = "League of Legends (TM) Client";
        private const string WindowName = "League of Legends";
        private IntPtr _handle = NativeMethods.FindWindow(null, WindowName);

        private Image _overlayImage;
        private Point _imagePosition;
        private Size _imageSize;

        protected override void OnLoad(EventArgs e)
        {
            BackColor = Color.Wheat;
            TransparencyKey = Color.Wheat;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;

            var initStyle = NativeMethods.GetWindowLong(Handle, -20);
            NativeMethods.SetWindowLong(Handle, -20, initStyle | 0x80000 | 0x20);

            NativeMethods.GetWindowRect(_handle, out _rect);
            Size = new Size(_rect.Right - _rect.Left, _rect.Bottom - _rect.Top);
            Top = _rect.Top;
            Left = _rect.Left;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制图片并调整大小
            if (_overlayImage != null)
            {
                e.Graphics.DrawImage(_overlayImage, new Rectangle(_imagePosition, _imageSize));
            }
        }

        // 公共方法来更新图片、位置和大小
        public void UpdateImage(Image newImage, Point newPosition, Size newSize)
        {
            _overlayImage = newImage;
            _imagePosition = newPosition;
            _imageSize = newSize;
            Invalidate(); // 强制重绘窗口
        }
    }

    public class Program
    {
        public static void Mini()
        {
            var overlay = new OverlayForm();
            overlay.Show();
            var screenCapture = new ScreenCapture();
            _ = screenCapture.CaptureScreenPeriodically(200, (bitmap) =>
            {
                overlay.UpdateImage(bitmap, new Point(0, 0), new Size(100, 100));
            });

            var l = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "123.png");
            var s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "champion", "KogMaw.png");

        }
    }
}
