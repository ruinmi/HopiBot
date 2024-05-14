using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace HopiBot.Game
{
    public class GameService
    {
        public static GameService Instance = new GameService();

        public bool IsRunning = false;

        private GameService()
        {
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            for (int i = 50; i >= 0; i--)
            {
                Mouse.MouseRClick(2000, 1440 / 2);
                Thread.Sleep(100);
            }
            Keyboard.Press("Y");

            var random = new Random();
            IsRunning = true;
            while (IsRunning)
            {
                for (int i = 500; i >= 0; i--)
                {
                    Mouse.MouseRClick(2000 + random.Next(0, 100), 1440 / 2 + 2 + random.Next(-100, 100));
                    Thread.Sleep(100);
                }

                Thread.Sleep(10000);

                for (int i = 500; i >= 0; i--)
                {
                    Mouse.MouseRClick(300 - random.Next(0, 100), 1440 / 2 - 25 - random.Next(-50, 50));
                    Thread.Sleep(100);
                }

                Thread.Sleep(8000);
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }

    public static class Mouse
    {
        [Flags]
        private enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Wheel = 0x00000800,
            Absolute = 0x00008000
        }

        public static void MouseRClick(int x, int y)
        {
            SetCursorPos(x, y); // 设置鼠标位置
            mouse_event(MouseEventFlags.RightDown, x, y, 0, 0); // 模拟鼠标按下
            mouse_event(MouseEventFlags.RightUp, x, y, 0, 0); // 模拟鼠标释放
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlags dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        private static extern bool SetCursorPos(int x, int y);
    }

    public class Keyboard
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int KEYEVENTF_KEYDOWN = 0x0000; // Key down flag
        private const int KEYEVENTF_KEYUP = 0x0002;   // Key up flag

        private static readonly Dictionary<string, byte> KeyMap = new Dictionary<string, byte>
        {
            { "A", 0x41 }, { "B", 0x42 }, { "C", 0x43 }, { "D", 0x44 },
            { "E", 0x45 }, { "F", 0x46 }, { "G", 0x47 }, { "H", 0x48 },
            { "I", 0x49 }, { "J", 0x4A }, { "K", 0x4B }, { "L", 0x4C },
            { "M", 0x4D }, { "N", 0x4E }, { "O", 0x4F }, { "P", 0x50 },
            { "Q", 0x51 }, { "R", 0x52 }, { "S", 0x53 }, { "T", 0x54 },
            { "U", 0x55 }, { "V", 0x56 }, { "W", 0x57 }, { "X", 0x58 },
            { "Y", 0x59 }, { "Z", 0x5A },
            { "0", 0x30 }, { "1", 0x31 }, { "2", 0x32 }, { "3", 0x33 },
            { "4", 0x34 }, { "5", 0x35 }, { "6", 0x36 }, { "7", 0x37 },
            { "8", 0x38 }, { "9", 0x39 },
            { "ENTER", 0x0D }, { "SPACE", 0x20 }, { "ESC", 0x1B },
            { "LEFT", 0x25 }, { "UP", 0x26 }, { "RIGHT", 0x27 }, { "DOWN", 0x28 },
            // Add more keys as needed
        };

        public static void Press(string key)
        {
            if (KeyMap.TryGetValue(key.ToUpper(), out byte keyCode))
            {
                // Key down
                keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Thread.Sleep(100);
                // Key up
                keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }
    }
}
