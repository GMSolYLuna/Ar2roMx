using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace GunboundImageCreator.App
{
    /// <summary>
    /// Interaction logic for ImageFrame.xaml
    /// </summary>
    public partial class ImageFrame : INotifyPropertyChanged
    {
        private string _frameHeader;
        public string FrameHeader
        {
            get { return _frameHeader; }
            set
            {
                if (value == _frameHeader)
                    return;

                _frameHeader = value;
                OnPropertyChanged("FrameHeader");
            }
        }

        public int FrameMargin { get; set; }

        public ImageFrame()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetImage(Bitmap bmp)
        {
            using (bmp)
            {
                var bImg = LoadBitmap(bmp);
                imgMain.Source = bImg;
            }

            Width = imgMain.Source.Width + 20;
            Height = imgMain.Source.Height + 33;
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(Bitmap source)
        {
            BitmapSource bs;
            using (source)
            {
                source.MakeTransparent(Color.FromArgb(255, 0, 0, 0));
                var ip = source.GetHbitmap();

                try
                {
                    bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                                                                                      IntPtr.Zero, Int32Rect.Empty,
                                                                                      BitmapSizeOptions.FromEmptyOptions
                                                                                          ());
                }
                finally
                {
                    DeleteObject(ip);
                }
            }

            return bs;
        }

        private Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }


        private void MenuSaveClick(object sender, RoutedEventArgs e)
        {
            var bitmapImg = (BitmapSource)imgMain.Source;

            var saveDialog = new SaveFileDialog { Filter = "Archivos de imagen(*.bmp,*.png,*.jpg)|*.bmp;*.png;*.jpg" };

            if (saveDialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(saveDialog.FileName);

                using (var bmp = GetBitmap(bitmapImg))
                {
                    if (extension != null)
                        switch (extension.ToLower())
                        {
                            case ".png":
                                bmp.Save(saveDialog.FileName, ImageFormat.Png);
                                break;
                            case ".jpg":
                                bmp.Save(saveDialog.FileName, ImageFormat.Jpeg);
                                break;
                            default:
                                bmp.Save(saveDialog.FileName, ImageFormat.Bmp);
                                break;

                        }
                }
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}