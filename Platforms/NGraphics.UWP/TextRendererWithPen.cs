using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace NGraphics.UWP
{
    class TextRendererWithPen : CallbackBase, SharpDX.DirectWrite.TextRenderer
    {
        readonly SharpDX.Direct2D1.Factory _d2DFactory;
        readonly RenderTarget _renderTarget;
        public SharpDX.Direct2D1.Brush FontBrush { get; set; }
        public SharpDX.Direct2D1.Brush PenBrush { get; set; }
        public StrokeStyle PenStyle { get; set; }
        public float PenWidth { get; set; }

        public TextRendererWithPen(SharpDX.Direct2D1.Factory d2DFactory, RenderTarget renderTarget)
        {
            _d2DFactory = d2DFactory;
            _renderTarget = renderTarget;
        }
        public Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
        {
            var pathGeometry = new PathGeometry(_d2DFactory);
            var geometrySink = pathGeometry.Open();

            var fontFace = glyphRun.FontFace;
            if (glyphRun.Indices.Length > 0)
                fontFace.GetGlyphRunOutline(glyphRun.FontSize, glyphRun.Indices, glyphRun.Advances, glyphRun.Offsets, glyphRun.IsSideways, glyphRun.BidiLevel % 2 != 0, geometrySink);
            geometrySink.Close();
            geometrySink.Dispose();
            fontFace.Dispose();

            var matrix = new Matrix3x2()
            {
                M11 = 1,
                M12 = 0,
                M21 = 0,
                M22 = 1,
                M31 = baselineOriginX,
                M32 = baselineOriginY
            };

            var transformedGeometry = new TransformedGeometry(_d2DFactory, pathGeometry, matrix);
            _renderTarget.DrawGeometry(transformedGeometry, PenBrush, PenWidth, PenStyle);
            _renderTarget.FillGeometry(transformedGeometry, FontBrush);

            pathGeometry.Dispose();
            transformedGeometry.Dispose();

            return SharpDX.Result.Ok;
        }

        public Result DrawInlineObject(object clientDrawingContext, float originX, float originY, InlineObject inlineObject, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
        {
            return Result.NotImplemented;
        }

        public Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, ComObject clientDrawingEffect)
        {
            var rect = new SharpDX.RectangleF(0, strikethrough.Offset, strikethrough.Width, strikethrough.Offset + strikethrough.Thickness);
            var rectangleGeometry = new RectangleGeometry(_d2DFactory, rect);
            var matrix = new Matrix3x2()
            {
                M11 = 1,
                M12 = 0,
                M21 = 0,
                M22 = 1,
                M31 = baselineOriginX,
                M32 = baselineOriginY
            };
            var transformedGeometry = new TransformedGeometry(_d2DFactory, rectangleGeometry, matrix);

            _renderTarget.DrawGeometry(transformedGeometry, FontBrush);
            _renderTarget.FillGeometry(transformedGeometry, FontBrush);

            rectangleGeometry.Dispose();
            transformedGeometry.Dispose();

            return Result.Ok;
        }

        public Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, ComObject clientDrawingEffect)
        {
            var rect = new SharpDX.RectangleF(0, underline.Offset, underline.Width, underline.Offset + underline.Thickness);
            var rectangleGeometry = new RectangleGeometry(_d2DFactory, rect);
            var matrix = new Matrix3x2()
            {
                M11 = 1,
                M12 = 0,
                M21 = 0,
                M22 = 1,
                M31 = baselineOriginX,
                M32 = baselineOriginY
            };
            var transformedGeometry = new TransformedGeometry(_d2DFactory, rectangleGeometry, matrix);

            _renderTarget.DrawGeometry(transformedGeometry, FontBrush);
            _renderTarget.FillGeometry(transformedGeometry, FontBrush);

            rectangleGeometry.Dispose();
            transformedGeometry.Dispose();

            return SharpDX.Result.Ok;
        }

        public RawMatrix3x2 GetCurrentTransform(object clientDrawingContext)
        {
            return _renderTarget.Transform;
        }

        public float GetPixelsPerDip(object clientDrawingContext)
        {
            return _renderTarget.PixelSize.Width / 96f;
        }

        public bool IsPixelSnappingDisabled(object clientDrawingContext)
        {
            return false;
        }
    }
}
