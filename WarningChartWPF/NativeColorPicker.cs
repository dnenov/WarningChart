using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Native Windows color picker using the ChooseColor dialog
    /// </summary>
    public static class NativeColorPicker
    {
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool ChooseColor(ref CHOOSECOLOR cc);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CHOOSECOLOR
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public uint rgbResult;
            public IntPtr lpCustColors;
            public uint Flags;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
        }

        private const int CC_ANYCOLOR = 0x00000100;
        private const int CC_FULLOPEN = 0x00000002;
        private const int CC_RGBINIT = 0x00000001;

        private static uint[] customColors = new uint[16];

        /// <summary>
        /// Show the native Windows color picker dialog
        /// </summary>
        /// <param name="initialColor">Initial color to display</param>
        /// <param name="owner">Owner window handle</param>
        /// <returns>Selected color, or null if cancelled</returns>
        public static Color? ShowColorPicker(Color initialColor, IntPtr owner = default)
        {
            var chooseColor = new CHOOSECOLOR();
            chooseColor.lStructSize = Marshal.SizeOf(chooseColor);
            chooseColor.hwndOwner = owner;
            chooseColor.rgbResult = (uint)(initialColor.R | (initialColor.G << 8) | (initialColor.B << 16));
            
            // Allocate memory for custom colors array
            IntPtr customColorsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)) * 16);
            Marshal.Copy(Array.ConvertAll(customColors, x => (int)x), 0, customColorsPtr, 16);
            chooseColor.lpCustColors = customColorsPtr;
            
            chooseColor.Flags = CC_ANYCOLOR | CC_FULLOPEN | CC_RGBINIT;

            bool result = false;
            try
            {
                result = ChooseColor(ref chooseColor);
            }
            finally
            {
                // Copy back custom colors and free memory
                int[] customColorsInt = new int[16];
                Marshal.Copy(chooseColor.lpCustColors, customColorsInt, 0, 16);
                customColors = Array.ConvertAll(customColorsInt, x => (uint)x);
                Marshal.FreeHGlobal(customColorsPtr);
            }

            if (result)
            {
                uint rgb = chooseColor.rgbResult;
                byte r = (byte)(rgb & 0xFF);
                byte g = (byte)((rgb >> 8) & 0xFF);
                byte b = (byte)((rgb >> 16) & 0xFF);
                return Color.FromRgb(r, g, b);
            }

            return null; // User cancelled
        }
    }
} 