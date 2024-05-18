using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using HopiBot.LCU;
using HopiBot.LCU.bo;
using InputManager;
using Application = System.Windows.Application;

namespace HopiBot.Game
{
    public static class Controller
    {
        public static readonly RatioPoint MiniMapUnderTurretMid = new RatioPoint(0.915, 0.89);
        public static RatioPoint MiniMapCenterMid = new RatioPoint(0.928, 0.875);
        public static RatioPoint MiniMapUnderOuterTurretMid = new RatioPoint(0.94, 0.85);
        public static RatioPoint MiniMapUnderInnderTurretMid = new RatioPoint(0.95, 0.84);
        public static RatioPoint MiniMapUnderInhibitorTurretMid = new RatioPoint(0.96, 0.82);
        public static RatioPoint MiniMapEnemyNexus = new RatioPoint(0.9787, 0.78);
        public static RatioPoint CenterOfScreen = new RatioPoint(0.5, 0.5);

        public static readonly List<RatioPoint> ShopItemButtons = new List<RatioPoint>
            { new RatioPoint(0.26, 0.4), new RatioPoint(0.37, 0.4), new RatioPoint(0.47, 0.4) };

        public const string MidOuterTurret = "Turret_T2_C_05_A";
        public const string MidInnerTurret = "Turret_T2_C_04_A";
        public const string MidInhibitorTurret = "Turret_T2_C_03_A";
        public const string NexusTurret1 = "Turret_T2_C_01_A";
        public const string NexusTurret2 = "Turret_T2_C_02_A";


        public static void RightClick(RatioPoint point)
        {
            Mouse.Move(point.X, point.Y);
            Thread.Sleep(50);
            Mouse.PressButton(Mouse.MouseKeys.Right, 100);
        }

        public static void LeftClick(RatioPoint point)
        {
            Mouse.Move(point.X, point.Y);
            Thread.Sleep(50);
            Mouse.PressButton(Mouse.MouseKeys.Left, 100);
        }

        public static void LeftClickClient(int x, int y)
        {
            var hWnd = FindWindow(null, "League of Legends");

            if (hWnd != IntPtr.Zero)
            {
                RECT rect;
                if (!GetWindowRect(hWnd, out rect))
                {
                    MessageBox.Show("Failed to get window rectangle!");
                    return;
                }

                // Calculate the absolute position to click
                var absoluteX = rect.Left + x;
                var absoluteY = rect.Top + y;

                SetForegroundWindow(hWnd);
                Thread.Sleep(100);
                Mouse.Move(absoluteX, absoluteY);
                Thread.Sleep(100);
                Mouse.PressButton(Mouse.MouseKeys.Left, 100);
            }
        }

        public static void LockScreen()
        {
            var hWnd = FindWindow(null, "League of Legends (TM) Client");

            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd);
                Thread.Sleep(100);
                Logger.Log("Locking Screen");
                Keyboard.KeyPress(Keys.Y, 50);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // RECT struct to store window coordinates
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
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
