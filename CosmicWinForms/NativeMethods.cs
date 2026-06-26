using System;
using System.Runtime.InteropServices;

namespace CosmicWinForms
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
