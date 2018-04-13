using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NGraphics
{
   public class PresentationFoundationPlatform : IPlatform
   {
      public string Name { get { return "WPF"; } }

      public const double DPI = 96;

      public IImage CreateImage(Color[] colors, int pixelWidth, double scale)
      {
         var pixelHeight = colors.Length / pixelWidth;
         var palette = new BitmapPalette(colors.Select(c => c.GetColor()).ToList());
         var pixelFormat = PixelFormats.Rgb24;
         var dpi = DPI * scale;
         var stride = pixelWidth / pixelFormat.BitsPerPixel;
         var pixels = new byte[pixelHeight * stride];

         var bitmap = BitmapSource.Create(pixelWidth, pixelHeight, dpi, dpi, pixelFormat, palette, pixels, stride);
         return new ImageSourceImage(bitmap);
      }

      public IImageCanvas CreateImageCanvas(Size size, double scale, bool transparency)
      {
         if (!transparency)
         {
            throw new NotImplementedException();
         }
         var drawingVisual = new DrawingVisual();
         var dvCanvas = new DrawingVisualImageCanvas(drawingVisual, size, scale);
         return dvCanvas;
      }

      public IImage LoadImage(Stream stream)
      {
         var bitmap = new BitmapImage();
         bitmap.BeginInit();
         bitmap.StreamSource = stream;
         bitmap.EndInit();
         return new ImageSourceImage(bitmap);
      }

      public IImage LoadImage(string path)
      {
         return LoadImage(File.Open(path, FileMode.Open));
      }

      public TextMetrics MeasureText(string text, Font font)
      {
         return TextUtil.MeasureText(text, font);
      }

      public Task<Stream> OpenFileStreamForWritingAsync(string path)
      {
         return Task.FromResult((Stream)new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read));
      }
   }

   static class TextUtil
   {
      public static FormattedText GetFormattedText(string text, Font font)
      {
         var formattedText = new FormattedText(text, CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, font.GetTypeface(), font.Size, null);
         return formattedText;
      }

      public static TextMetrics MeasureText(string text, Font font)
      {
         var formattedText = GetFormattedText(text, font);
         var metrics = new TextMetrics {
            Width = formattedText.WidthIncludingTrailingWhitespace,
            Ascent = formattedText.Baseline,
            Descent = Math.Min(formattedText.Height, formattedText.Extent) - formattedText.Baseline
         };
         return metrics;
      }
   }

   public class DrawingContextCanvas : ICanvas
   {
      public DrawingContext DrawingContext { get { return dc; } }

      public Size Size { get; private set; }

      DrawingContext dc;

      public DrawingContextCanvas(DrawingContext dc, Size size)
      {
         this.dc = dc;
         Size = size;
      }

      public void DrawEllipse(Rect frame, Pen pen, Brush brush)
      {
         dc.DrawEllipse(brush?.GetBrush(), pen?.GetPen(), frame.Center.GetPoint(), frame.Width / 2, frame.Height / 2);
      }

      public void DrawImage(IImage image, Rect frame, double alpha)
      {
         if (alpha != 1)
         {
            throw new NotImplementedException();
         }

         var isImage = image as ImageSourceImage;
         dc.DrawImage(isImage.BitmapSource, frame.GetRect());
      }

      public void DrawPath(IEnumerable<PathOp> ops, Pen pen, Brush brush)
      {
         StreamGeometry streamGeometry = new StreamGeometry();
         using (StreamGeometryContext geometryContext = streamGeometry.Open())
         {
            foreach (var op in ops)
            {
               var mt = op as MoveTo;
               if (mt != null)
               {
                  geometryContext.BeginFigure(mt.Point.GetPoint(), true, false);
                  continue;
               }

               var lt = op as LineTo;
               if (lt != null)
               {
                  geometryContext.LineTo(lt.Point.GetPoint(), true, false);
                  continue;
               }

               var at = op as ArcTo;
               if (at != null)
               {
                  /*
                  Point c1, c2;
                  at.GetCircles(pp, out c1, out c2);
                  var circleCenter = at.LargeArc ^ !at.SweepClockwise ? c2 : c1;
                  var rotationAngle = (float)Math.Atan2(at.Point.Y - circleCenter.Y, at.Point.X - circleCenter.X);
                  geometryContext.ArcTo(at.Point.GetPoint(), at.Radius.GetSize(), rotationAngle,
                      at.LargeArc, at.SweepClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                      true, false
                  );*/
                  throw new NotImplementedException();
               }

               var ct = op as CurveTo;
               if (ct != null)
               {
                  geometryContext.BezierTo(ct.Control1.GetPoint(), ct.Control2.GetPoint(), ct.Point.GetPoint(), true, false);
               }

               throw new NotSupportedException();
            }
         }

         dc.DrawGeometry(brush?.GetBrush(), pen?.GetPen(), streamGeometry);
      }

      public void DrawRectangle(Rect frame, Size corner, Pen pen = null, Brush brush = null)
      {
         if (corner.Width > 0 || corner.Height > 0)
         {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
               bool isStroked = pen != null;
               const bool isSmoothJoin = true;

               var diag = corner.Diagonal;
               var cornerRadius = new {
                  TopLeft = diag,
                  TopRight = diag,
                  BottomRight = diag,
                  BottomLeft = diag
               };

               var rect = frame.GetRect();

               context.BeginFigure(rect.TopLeft + new Vector(0, cornerRadius.TopLeft), brush != null, true);
               context.ArcTo(new System.Windows.Point(rect.TopLeft.X + cornerRadius.TopLeft, rect.TopLeft.Y),
                   new System.Windows.Size(cornerRadius.TopLeft, cornerRadius.TopLeft),
                   90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);

               context.LineTo(rect.TopRight - new Vector(cornerRadius.TopRight, 0), isStroked, isSmoothJoin);
               context.ArcTo(new System.Windows.Point(rect.TopRight.X, rect.TopRight.Y + cornerRadius.TopRight),
                   new System.Windows.Size(cornerRadius.TopRight, cornerRadius.TopRight),
                   90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);

               context.LineTo(rect.BottomRight - new Vector(0, cornerRadius.BottomRight), isStroked, isSmoothJoin);
               context.ArcTo(new System.Windows.Point(rect.BottomRight.X - cornerRadius.BottomRight, rect.BottomRight.Y),
                   new System.Windows.Size(cornerRadius.BottomRight, cornerRadius.BottomRight),
                   90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);

               context.LineTo(rect.BottomLeft + new Vector(cornerRadius.BottomLeft, 0), isStroked, isSmoothJoin);
               context.ArcTo(new System.Windows.Point(rect.BottomLeft.X, rect.BottomLeft.Y - cornerRadius.BottomLeft),
                   new System.Windows.Size(cornerRadius.BottomLeft, cornerRadius.BottomLeft),
                   90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);

               context.Close();
            }
            dc.DrawGeometry(brush?.GetBrush(), pen?.GetPen(), geometry);
         }
         else
         {
            dc.DrawRectangle(brush?.GetBrush(), pen?.GetPen(), frame.GetRect());
         }
      }

      public void DrawText(string text, Rect frame, Font font, TextAlignment alignment, Pen pen, Brush brush)
      {
         var formattedText = TextUtil.GetFormattedText(text, font);
         formattedText.SetForegroundBrush((brush ?? Brushes.Black).GetBrush());
         var point = frame.TopLeft.GetPoint();
         point.Y -= formattedText.Baseline;
         dc.DrawText(formattedText, point);
      }

      public TextMetrics MeasureText(string text, Font font)
      {
         return TextUtil.MeasureText(text, font);
      }

      List<int> stackDepths = new List<int>();

      public void RestoreState()
      {
         var lastStackIndex = stackDepths.Count - 1;

         while (stackDepths[lastStackIndex] > 0)
         {
            dc.Pop();
            stackDepths[lastStackIndex]--;
         }
         stackDepths.RemoveAt(lastStackIndex);
      }

      public void SaveState()
      {
         stackDepths.Add(0);
      }

      public void Transform(Transform transform)
      {
         var matrixTransform = new System.Windows.Media.MatrixTransform(transform.A, transform.B, transform.C, transform.D, transform.E, transform.F);
         dc.PushTransform(matrixTransform);
         if (stackDepths.Count > 0)
         {
            stackDepths[stackDepths.Count - 1]++;
         }
      }


   }

   public class DrawingVisualImageCanvas : DrawingContextCanvas, IImageCanvas
   {
      public double Scale { get; private set; }

      public DrawingVisual DrawingVisual { get; private set; }

      public DrawingVisualImageCanvas(DrawingVisual dv, Size size, double scale) : base(dv.RenderOpen(), size)
      {
         DrawingVisual = dv;
         Scale = scale;
      }

      public IImage GetImage()
      {
         DrawingContext.Close();
         var mergedImage = new RenderTargetBitmap((int)Size.Width, (int)Size.Height, PresentationFoundationPlatform.DPI, PresentationFoundationPlatform.DPI, PixelFormats.Pbgra32);
         mergedImage.Render(DrawingVisual);
         var img = new ImageSourceImage(mergedImage);
         img.BitmapSource.Freeze();
         return img;
      }
   }

   public class ImageSourceImage : IImage
   {
      public BitmapSource BitmapSource { get; private set; }


      public double Scale { get; set; }

      public Size Size { get { return new Size(BitmapSource.Width, BitmapSource.Height); } }

      public ImageSourceImage(BitmapSource bitmapSource)
      {
         BitmapSource = bitmapSource;
      }

      public void Save(Stream stream, BitmapEncoder encoder)
      {
         var bitmapFrame = BitmapFrame.Create(BitmapSource);
         encoder.Frames.Add(bitmapFrame);
         encoder.Save(stream);
      }

      public void SaveAsPng(Stream stream)
      {
         BitmapEncoder encoder = new PngBitmapEncoder();
         Save(stream, encoder);
      }

      public void SaveAsPng(string path)
      {
         SaveAsPng(File.OpenWrite(path));
      }
   }

   public static class Conversions
   {
      public static Typeface GetTypeface(this Font font)
      {
         return new Typeface(font.Name);
      }

      public static System.Windows.Size GetSize(this Size size)
      {
         return new System.Windows.Size(size.Width, size.Height);
      }

      public static System.Windows.Rect GetRect(this Rect rect)
      {
         return new System.Windows.Rect(rect.X, rect.Y, rect.Width, rect.Height);
      }

      public static System.Windows.Point GetPoint(this Point point)
      {
         return new System.Windows.Point(point.X, point.Y);
      }

      public static System.Windows.Media.Color GetColor(this Color color)
      {
         return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
      }

      public static System.Windows.Media.Pen GetPen(this Pen pen)
      {
         return new System.Windows.Media.Pen(new SolidColorBrush(pen.Color.GetColor()), pen.Width);
      }

      public static System.Windows.Media.Brush GetBrush(this Brush brush)
      {
         if (brush is GradientBrush)
         {
            var gradienBrush = brush as GradientBrush;
            var stops = new GradientStopCollection(gradienBrush.Stops.Select(
                g => new System.Windows.Media.GradientStop(g.Color.GetColor(), g.Offset)
            ));

            if (brush is LinearGradientBrush)
            {
               return new System.Windows.Media.LinearGradientBrush(stops);
            }
            else if (brush is RadialGradientBrush)
            {
               return new System.Windows.Media.RadialGradientBrush(stops);
            }
            else
            {
               throw new NotImplementedException();
            }
         }
         else if (brush is SolidBrush)
         {
            var solidBrush = brush as SolidBrush;
            return new SolidColorBrush(solidBrush.Color.GetColor());
         }
         else
         {
            throw new NotImplementedException();
         }
      }

   }
}
