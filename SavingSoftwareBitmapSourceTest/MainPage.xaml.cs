using NGraphics.UWP;
using System.IO;
using Windows.UI.Xaml.Controls;
using System;
using NGraphics;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace SavingSoftwareBitmapSourceTest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            test();
        }
        public async void test()
        {
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri( "ms-appx:///Assets/ErulisseuiinSpaceshipPack.svg",UriKind.Absolute));
            using (var stream = new System.IO.StreamReader(await file.OpenStreamForReadAsync()))
            {
                var canvas = Platforms.Current.CreateImageCanvas(new Size(120 * 5, 120), scale: 2);
                var svg = NGraphics.Graphic.LoadSvg(stream);
                svg.Draw(canvas);
                img.Source = (canvas.GetImage() as WICBitmapSourceImage).SaveAsSoftwareBitmapSource();
            }
        }
    }
}
