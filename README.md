# NGraphics

NGraphics is a cross platform library for rendering vector graphics on .NET. It provides a unified API for both immediate and retained mode rendering to fast and high quality native renderers.


## Installation

Install it from nuget.


## Getting Started

The most important class is `ICanvas`. Uses canvases to render vector graphics (rectangles, ellipses, paths) to "something". Sometimes canvases are views on the screen, sometimes they are images -- you never really know.

We can draw a little house easily enough:

```csharp
var canvas = Platforms.Current.CreateImageCanvas (new Size (100), scale: 2);

canvas.DrawEllipse (10, 20, 30, 30, Pens.Red, Brushes.White);
canvas.DrawRectangle (40, 50, 60, 70, brush: Brushes.Blue);
canvas.DrawPath (new PathOp[] {	
	new MoveTo (100, 100),
	new LineTo (50, 100),
	new LineTo (50, 0),
	new ClosePath ()
}, brush: Brushes.Gray);

canvas.GetImage ().SaveAsPng ("Example1.png");
```

<img src="TestResults/Example1-Mac.png" width="100" height="100" />

`Platforms.Current.CreateImageCanvas` is just our tricky way to get a platform-specific `ICanvas` that we can rendered on. `IImageCanvases` are special because you can call `GetImage` to get an image of the drawing when you are done. We use a `scale` of 2 to render retina graphics.

Paths are drawn using standard turtle graphics.


## Pens and Brushes

When drawing, you have a choice of pens to stroke the object with or brushes to fill it with.

Anyway.

`Pens` can be any *color* and any *width*.

`Brushes` can be solid colors or trippy multi-color gradients (linear and radial!)


## Colors

What would a graphics library be without a `Color` class? Well, this one is a struct. Colors are light-weight, have fun with them.

Normally you will use the RGBA constructor of color: `new Color (r, g, b, a)` where each value can range from 0 to 1.

If you're not normal, you might prefer web notation: `new Color ("#BEEFEE")`.



## Retained Mode

Sometimes it's nice to hang onto the graphical elements themselves so that you can change them later, or perhaps cache them from an expensive-to-compute draw operation, or maybe you just want to sing to them. Whatever your needs, NGraphics exposes the following graphical elements:

* `Rectangles` are best used for drawing rectangles.
* `Elliposes` can also be used to draw ovals and circles.
* `Paths` can draw anything that you can imagine, and more.

## Support


* iOS (Xamarin) using CoreGraphics
* Mac (Xamarin) using CoreGraphics
* .NET 4.5 using System.Drawing


## Retained Mode

```charp
var circle = new Ellipse (new Rectangle (Point.Zero, new Size (10)));

ICanvas canvas = ...;
circle.Draw (canvas);
```