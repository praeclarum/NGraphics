using NGraphics;
using NGraphics.UWP;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Yinyue200.SvgView
{

    public sealed class SvgView : Control
    {
        Image img;
        public SvgView()
        {
            this.DefaultStyleKey = typeof(SvgView);
            Loaded += SvgView_Loaded;
            SizeChanged += SvgView_SizeChanged;
            
        }

        private void SvgView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            redraw();
        }

        private void SvgView_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                var disinfo = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
                disinfo.DpiChanged += Disinfo_DpiChanged;
                Unloaded += (a, b) =>
                {
                    disinfo.DpiChanged -= Disinfo_DpiChanged;
                };
            }
            catch
            {
                //在设计器模式下可能无法监听DPI
            }
            img = (Image)GetTemplateChild(nameof(img));

            img.Source = images;

        }

        private void Disinfo_DpiChanged(Windows.Graphics.Display.DisplayInformation sender, object args)
        {
            redraw();
        }

        public Uri UriSource
        {
            get { return (Uri)GetValue(UriSourceProperty); }
            set { SetValue(UriSourceProperty, value); }
        }
        public static async Task<Stream> GetStreamFromUriAsync(Uri uri, System.Threading.CancellationToken cancellationToken)
        {

            switch (uri.Scheme)
            {
                case "ms-appx":
                case "ms-appdata":
                    {
                        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                        var result = await file.OpenAsync(Windows.Storage.FileAccessMode.Read).AsTask(cancellationToken).ConfigureAwait(false);
                        return result.AsStreamForRead();
                    }
                case "ms-resource":
                    {
                        var rm = ResourceManager.Current;
                        var context = ResourceContext.GetForCurrentView();
                        var candidate = rm.MainResourceMap.GetValue(uri.LocalPath, context);
                        if (candidate != null && candidate.IsMatch)
                        {
                            var file = await candidate.GetValueAsFileAsync();
                            return (await file.OpenAsync(FileAccessMode.Read).AsTask(cancellationToken).ConfigureAwait(false)).AsStreamForRead();
                        }
                        throw new Exception("Resource not found");
                    }
                case "file":
                    {
                        var file = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
                        return (await file.OpenAsync(FileAccessMode.Read).AsTask(cancellationToken).ConfigureAwait(false)).AsStreamForRead();
                    }
                default:
                    {
                        try
                        {
                            var streamRef = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(uri);
                            return (await streamRef.OpenReadAsync().AsTask(cancellationToken).ConfigureAwait(false)).AsStreamForRead();
                        }
                        catch
                        {
                            return null;
                        }
                    }
            }
        }



        public bool UseRecommendedSize
        {
            get { return (bool)GetValue(UseRecommendedSizeProperty); }
            set { SetValue(UseRecommendedSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UseRecommendedSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseRecommendedSizeProperty =
            DependencyProperty.Register("UseRecommendedSize", typeof(bool), typeof(SvgView), new PropertyMetadata(true));





        // Using a DependencyProperty as the backing store for UriSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.Register(nameof(UriSource), typeof(Uri), typeof(SvgView), new PropertyMetadata(null, UriSourceChangedCallback));
        public async static void UriSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newvalue = (Uri)e.NewValue;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return;
            }
            var file = await GetStreamFromUriAsync(newvalue,System.Threading.CancellationToken.None);
            ((SvgView)d).SetSource(file);
        }

        public void SetSource(Stream stream)
        {
            using (var reader= new StreamReader(stream))
            {
                if(AutoRedrawWhenDpiChanged)
                {
                    svg = reader.ReadToEnd();
                    redraw();
                }
                else
                {
                    setsource(reader);
                }
            }
        }

        private void redraw()
        {
            if(svg==null)
            {
                return;
            }
            using (var sr = new StringReader(svg))
            {
                setsource(sr);
            }
        }


        private bool canuse(double num)
        {
            if(double.IsNaN(num)||double.IsNegativeInfinity(num) || double.IsPositiveInfinity(num) || num<=0)
            {
                return false;
            }
            return true;
        }

        private void setsource(TextReader tr)
        {
            float dpi;
            try
            {
                dpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
            }
            catch
            {
                dpi = 96f;
            }
            setsource(tr, dpi);
        }
        Windows.UI.Xaml.Media.ImageSource _images;
        Windows.UI.Xaml.Media.ImageSource images
        {
            get
            {
                return _images;
            }
            set
            {
                _images = value;
                if(img!=null)
                {
                    img.Source = images;
                }
            }
        }
        private void setsource(TextReader tr, float disinfo)
        {
            if(canuse(this.ActualHeight)&&canuse(this.ActualWidth))
            {
                var svg = NGraphics.Graphic.LoadSvg(tr);
                Size size;
                if (UseRecommendedSize)
                {
                    size = svg.Size;
                }
                else
                {
                    size = new Size(ActualWidth, ActualHeight);
                }
                var canvas = Platforms.Current.CreateImageCanvas(size, scale: disinfo / 96f);
                svg.Draw(canvas);
                images = (canvas.GetImage() as WICBitmapSourceImage).SaveAsSoftwareBitmapSource();
            }

        }
        string svg;
        /// <summary>
        /// 在DPI改变后自动重绘SVG图像
        /// </summary>
        public bool AutoRedrawWhenDpiChanged
        {
            get { return (bool)GetValue(AutoRedrawWhenDpiChangedProperty); }
            set { SetValue(AutoRedrawWhenDpiChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoRedrawWhenDpiChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoRedrawWhenDpiChangedProperty =
            DependencyProperty.Register(nameof(AutoRedrawWhenDpiChanged), typeof(bool), typeof(SvgView), new PropertyMetadata(true));


    }
}
