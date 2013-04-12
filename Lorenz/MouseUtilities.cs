using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Lorenz
{
   public static class MouseUtilities
   {
      #region Constants
      private const int MOUSEEVENTF_LEFTDOWN = 0x02;
      private const int MOUSEEVENTF_LEFTUP = 0x04;
      private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
      private const int MOUSEEVENTF_RIGHTUP = 0x10;
      #endregion Constants

      #region Imports
      [StructLayout(LayoutKind.Sequential)]
      internal struct Win32Point
      {
         public Int32 X;
         public Int32 Y;
      };

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool GetCursorPos(ref Win32Point pt);

      [DllImport("user32.dll")]
      private static extern bool SetCursorPos(int x, int y);

      [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
      private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
      #endregion Imports

      public static Point GetPosition(Visual relativeTo)
      {
         var w32Mouse = new Win32Point();
         GetCursorPos(ref w32Mouse);
         return new Point(w32Mouse.X, w32Mouse.Y);
      }

      public static void SetPosition(int x, int y)
      {
         SetCursorPos(x, y);
      }

      public static void RightClick(Point p)
      {
         mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, 0);
      }

      public static void Click(Point p)
      {
         mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)p.X, (int)p.Y, 0, 0);
      }

      public static void DoubleClick(Point p)
      {
         mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)p.X, (int)p.Y, 0, 0);

         Thread.Sleep(150);

         mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)p.X, (int)p.Y, 0, 0);
      }

      public static void RightDoubleClick(Point p)
      {
         mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, 0);

         Thread.Sleep(150);

         mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, 0);
      }

      public static void LeftButtonDown()
      {
         mouse_event(MOUSEEVENTF_LEFTDOWN, 50, 50, 0, 0);
      }

      public static void LeftButtonUp()
      {
         mouse_event(MOUSEEVENTF_LEFTUP, 50, 50, 0, 0);
      }
   }
}
