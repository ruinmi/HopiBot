using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using InputManager;

namespace HopiBot.Game
{
    public static class Controller
    {
        public static void RightClick(RatioPoint point)
        {
            Mouse.Move(point.X, point.Y);
            Thread.Sleep(100);
            Mouse.PressButton(Mouse.MouseKeys.Right, 100);
        }

        public static void LeftClick(RatioPoint point)
        {
            Mouse.Move(point.X, point.Y);
            Thread.Sleep(100);
            Mouse.PressButton(Mouse.MouseKeys.Left, 100);
        }

        public static void LockScreen()
        {
            var hWnd = FindWindow(null, "League of Legends (TM) Client");

            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd);
                Logger.Log("Locking screen");
                Keyboard.KeyPress(Keys.Y, 50);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }

    public class RatioPoint
    {
        public int X;
        public int Y;

        public RatioPoint(double ratioX, double ratioY)
        {
            X = (int)(ratioX * Screen.PrimaryScreen.Bounds.Width);
            Y = (int)(ratioY * Screen.PrimaryScreen.Bounds.Height);
        }

        public RatioPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public RatioPoint Add(int x, int y)
        {
            return new RatioPoint(X + x, Y + y);
        }
    }
}
