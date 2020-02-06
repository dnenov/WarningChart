using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace WC.Helpers
{
    public class BitmapIcons
    {
        public enum IconSize
        {
            ICON_SMALL = 16,
            ICON_MEDIUM = 24,
            ICON_LARGE = 32
        }

        public const int DEFAULT_DPI = 96;

        /// <summary>
        /// The Window Handle of the host process
        /// </summary>
        private static IntPtr _hWndRevit;
        /// <summary>
        /// Revit Verson
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// The Revit Application
        /// </summary>
        public UIControlledApplication Application { get; set; }
        /// <summary>
        /// The image location in the Resource folder of the Assembly
        /// </summary>
        public string IconFilePath { get; set; }
        /// <summary>
        /// This Assembly, used to get the bitmap stream
        /// </summary>
        private Assembly loadedAssembly { get; set; }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="loadedAssembly">This assembly, used to retrieve the resource images</param>
        /// <param name="imageFile">The image path it the resource folder. Remember, it needs to be Embedded Resource</param>
        /// <param name="app">The Revit Application</param>
        public BitmapIcons(Assembly loadedAssembly, string imageFile, UIControlledApplication app)
        {
            this.IconFilePath = imageFile;
            this.Application = app;
            this.Version = Int32.Parse(app.ControlledApplication.VersionNumber);
            this.loadedAssembly = loadedAssembly;
            if (this.Version < 2019)
            {
                GetHandle18();
            }
            else
            {
                GetHandle19();
            }
        }
        /// <summary>
        /// Warns if the image is too large
        /// </summary>
        private void CheckIconSize()
        {
            var image = Image.FromFile(IconFilePath);
            var imageSize = Math.Max(image.Width, image.Height);
            if (imageSize > 96)
            {
                string warning = "Icon file is too large";
            }
        }
        /// <summary>
        /// Get Windows handle of the host process
        /// </summary>
        private void GetHandle18()
        {
            if (null == _hWndRevit)
            {
                Process process
                    = Process.GetCurrentProcess();

                IntPtr h = process.MainWindowHandle;
                _hWndRevit = new WindowHandle(h).Handle;
            }
        }
        private void GetHandle19()
        {
            if (null == _hWndRevit)
            {
                _hWndRevit = Application.MainWindowHandle;
            }
        }
        /// <summary>
        /// Resample image and creates bitmap or the given size.
        /// Produces crispiest icons of all times
        /// Icons are assumed to be square
        /// Courtesey to pyRevit
        /// </summary>
        /// <param name="size">IconSize (int): icon size (width and height)</param>
        /// <returns>Imaging.BitmapSource: object containing image data at given size</returns>
        private BitmapSource CreateBitmap(IconSize size)
        {
            using (var stream = loadedAssembly.GetManifestResourceStream(IconFilePath))
            {
                var bitmapImage = new Bitmap(stream);
                var adjustedIconSize = (int)size * 2;
                var adjustedDPI = DEFAULT_DPI * 2;
                var screenScaling = ProcessScreenScalefactor();

                stream.Seek(0, System.IO.SeekOrigin.Begin);
                BitmapImage baseImage = new BitmapImage();
                baseImage.BeginInit();
                baseImage.StreamSource = stream;
                //baseImage.StreamSource = fileStream;
                baseImage.DecodePixelHeight = Convert.ToInt32(adjustedIconSize * screenScaling);
                baseImage.EndInit();
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var imageSize = baseImage.PixelWidth;
                var imageFormat = baseImage.Format;
                var imageBytePerPixel = baseImage.Format.BitsPerPixel / 8;
                var palette = baseImage.Palette;

                var stride = imageSize * imageBytePerPixel;
                var arraySize = stride * imageSize;
                var imageData = Array.CreateInstance(typeof(Byte), arraySize);
                baseImage.CopyPixels(imageData, stride, 0);

                var bitmapSource = BitmapSource.Create(
                    Convert.ToInt32(adjustedIconSize * screenScaling),
                    Convert.ToInt32(adjustedIconSize * screenScaling),
                    adjustedDPI * screenScaling,
                    adjustedDPI * screenScaling,
                    imageFormat,
                    palette,
                    imageData,
                    stride);

                return bitmapSource;
            }            
        }
        /// <summary>
        /// Takes into account the user screen settings in order to create a proper scale ratio for the image icon
        /// </summary>
        /// <returns>Scale Factor (double): the scale factor to be multiplied by.</returns>
        private double ProcessScreenScalefactor()
        {
            Screen screen = Screen.FromHandle(_hWndRevit);
            if(screen != null)
            {
                var actualWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                var scaledWidth = Screen.PrimaryScreen.WorkingArea.Width;
                return Math.Abs(scaledWidth / actualWidth);
            }
            return 1.0;
        }

        public BitmapSource SmallBitmap()
        {
            return this.CreateBitmap(IconSize.ICON_SMALL);
        }
        public BitmapSource MediumBitmap()
        {
            return this.CreateBitmap(IconSize.ICON_MEDIUM);
        }
        public BitmapSource LargeBitmap()
        {
            return this.CreateBitmap(IconSize.ICON_LARGE);
        }
    }
}

/// <summary>
/// Retrieve Revit Windows thread in order to pass it to the form as it's owner
/// </summary>
public class WindowHandle : IWin32Window
{
    IntPtr _hwnd;

    public WindowHandle(IntPtr h)
    {
        Debug.Assert(IntPtr.Zero != h,
          "expected non-null window handle");

        _hwnd = h;
    }

    public IntPtr Handle
    {
        get
        {
            return _hwnd;
        }
    }
}
