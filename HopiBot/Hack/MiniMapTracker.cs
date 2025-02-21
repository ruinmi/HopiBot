using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HopiBot.Game;
using HopiBot.Hack.Utils;
using HopiBot.LCU;
using OpenCvSharp;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


namespace HopiBot.Hack
{
    public class OverlayForm : Form
    {
        private RECT _rect;

        private const string WindowName = "League of Legends (TM) Client";
        private IntPtr _handle = NativeMethods.FindWindow(null, WindowName);

        private Image _overlayImage;
        private Point _imagePosition;
        private Size _imageSize;

        private List<Point> _rectPosList;
        private Size _rectSize;

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

            if (_rectPosList != null)
            {
                Console.WriteLine(_rectPosList.Count);
                foreach (var pos in _rectPosList)
                {
                    if (pos == Point.Empty) continue;
                    e.Graphics.DrawRectangle(new Pen(Color.Chartreuse, 1), new Rectangle(pos, _rectSize));
                }
            }
        }

        public void UpdateRect(List<Point> posList, Size size)
        {
            _rectPosList = posList;
            _rectSize = size;
            Invalidate();
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

    public class MiniMapTracker
    {
        private const int MaximumMiniMapScale = 3;


        private void Calculate(out double miniMapWidth, out double championIconWidth, out Point miniMapPosition)
        {
            var gameWidth = double.Parse(Config.Instance.GetValue("Width"));
            var gameHeight = double.Parse(Config.Instance.GetValue("Height"));
            var minimapScale = double.Parse(Config.Instance.GetValue("MinimapScale"));
            var noScaleMiniMapWidth = 280 * (gameWidth / 2560);
            var noScaleChampionIconWidth = 20.5 * (gameWidth / 2560);
            var miniMapScaleFactor = noScaleMiniMapWidth / MaximumMiniMapScale;
            var championIconScaleFactor = noScaleChampionIconWidth / MaximumMiniMapScale;

            miniMapWidth = noScaleMiniMapWidth + minimapScale * miniMapScaleFactor;
            championIconWidth = noScaleChampionIconWidth + minimapScale * championIconScaleFactor;
            miniMapPosition = new Point((int)(gameWidth - miniMapWidth), (int)(gameHeight - miniMapWidth));
        }

        public void Track()
        {
            Calculate(out var miniMapWidth, out var championIconWidth, out var miniMapPosition);

            var enemyChampionIcons = new List<Bitmap>();
            var enemyChampions = GameApi.GetEnemyChampions();
            foreach (var enemyChampion in enemyChampions)
            {
                var icon = Image.FromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "champion", $"{enemyChampion}.png")) as Bitmap;
                icon = ImageHelper.ResizeBitmap(icon, new Size((int)championIconWidth, (int)championIconWidth));
                icon = ImageHelper.CropToCircle(icon);
                enemyChampionIcons.Add(icon);
            }

            var overlay = new OverlayForm();
            overlay.Show();
            var screenCapture = new ScreenCapture();
            var posList = new List<Point>();
            _ = screenCapture.CaptureScreenPeriodically(1000, (bitmap) =>
            {
                var croppedBitMap = ImageHelper.CropBitmap(bitmap, miniMapPosition, new Size((int)miniMapWidth, (int)miniMapWidth));
                posList.Clear();
                foreach (var icon in enemyChampionIcons)
                {
                    var iconPos = ImageHelper.LocatePattern(croppedBitMap, icon);
                    if (iconPos == Point.Empty) continue;
                    var pos = new Point(iconPos.X + miniMapPosition.X, iconPos.Y + miniMapPosition.Y);
                    posList.Add(pos);
                }

                overlay.UpdateRect(posList, new Size((int)championIconWidth, (int)championIconWidth));
            });
        }
    }
}
