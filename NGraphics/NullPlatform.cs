using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NGraphics
{
	public class NullPlatform : IPlatform
	{
		public string Name { get { return "Null"; } }

		public TextMetrics MeasureText (string text, Font font)
		{
			return GlobalMeasureText (text, font);
		}

		public Task<Stream> OpenFileStreamForWritingAsync (string path)
		{
			return Task.FromResult ((Stream)new MemoryStream ());
		}

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			return new NullImageSurface ();
		}

		public IImage CreateImage (Color[] colors, int width, double scale = 1.0)
		{
			return new NullImage ();
		}

		public IImage LoadImage (Stream stream)
		{
			return new NullImage ();
		}
		public IImage LoadImage (string path)
		{
			return new NullImage ();
		}

		class NullImageSurface : IImageCanvas
		{
			public IImage GetImage ()
			{
				return new NullImage ();
			}
			public Size Size { get { return Size.Zero; } }
			public double Scale { get { return 1.0; } }
			public void SaveState ()
			{
			}
			public void Transform (Transform transform)
			{
			}
			public void RestoreState ()
			{
			}
			public TextMetrics MeasureText (string text, Font font)
			{
				return GlobalMeasureText (text, font);
			}
			public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawRectangle (Rect frame, Size corner, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
			{
			}
		}

		class NullImage : IImage
		{
			public Size Size { get { return Size.Zero; } }
			public double Scale { get { return 1.0; } }
			public void SaveAsPng (string path)
			{
			}
			public void SaveAsPng (Stream stream)
			{
			}
		}

		public static TextMetrics GlobalMeasureText (string text, Font font)
		{
			var size = GetTextSize (text, font, out var lines);
			return new TextMetrics {
				Width = size.Width,
				Ascent = 15.4 / 20.0 * font.Size,
				Descent = 4.6 / 20.0 * font.Size,
			};
		}

		public static Size GetTextSize (string text, Font font, out int lines)
		{
			var _height = font.Size;
			var isBold = font.Name.Contains ("Bold");
			var fontHeight = _height;
			var lineHeight = (int)(_height * 1.42857143); // Floor is intentional -- browsers round down

			if (string.IsNullOrEmpty (text)) {
				lines = 1;
				return new Size (0, lineHeight);
			}

			var props = isBold ? BoldCharacterProportions : CharacterProportions;
			var avgp = isBold ? BoldAverageCharProportion : AverageCharProportion;

			var px = 0.0;
			lines = 1;
			var maxPWidth = 0.0;
			var pwidthConstraint = double.PositiveInfinity;
			var firstSpaceX = -1.0;
			var lastSpaceIndex = -1;
			var lineStartIndex = 0;

			var end = text.Length;
			for (var i = 0; i < end; i++) {
				var c = (int)text[i];
				var pw = (c < 128) ? props[c] : avgp;
				// Should we wrap?
				if (px + pw > pwidthConstraint && lastSpaceIndex > 0) {
					lines++;
					maxPWidth = Math.Max (maxPWidth, firstSpaceX);
					i = lastSpaceIndex;
					while (i < end && text[i] == ' ')
						i++;
					i--;
					px = 0;
					firstSpaceX = -1;
					lastSpaceIndex = -1;
					lineStartIndex = i + 1;
				}
				else {
					if (c == ' ') {
						if (i >= lineStartIndex && i > 0 && text[i - 1] != ' ')
							firstSpaceX = px;
						lastSpaceIndex = i;
					}
					px += pw;
				}
			}
			maxPWidth = Math.Max (maxPWidth, px);
			var width = _height * maxPWidth;
			var height = lines * lineHeight;
			return new Size (width, height);
		}

		static readonly double[] CharacterProportions = {
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0.27799999713897705, 0.27799999713897705, 0.27799999713897705, 0.27799999713897705, 0.27799999713897705, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0.27799999713897705, 0.25899994373321533, 0.4259999990463257, 0.5560001134872437, 0.5560001134872437, 1.0000001192092896, 0.6299999952316284, 0.27799999713897705,
			0.25899994373321533, 0.25899994373321533, 0.3520001173019409, 0.6000000238418579, 0.27799999713897705, 0.3890000581741333, 0.27799999713897705, 0.3330000638961792,
			0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437,
			0.5560001134872437, 0.5560001134872437, 0.27799999713897705, 0.27799999713897705, 0.6000000238418579, 0.6000000238418579, 0.6000000238418579, 0.5560001134872437,
			0.8000000715255737, 0.6480001211166382, 0.6850000619888306, 0.722000002861023, 0.7040001153945923, 0.6110001802444458, 0.5740000009536743, 0.7589999437332153,
			0.722000002861023, 0.25899994373321533, 0.5190001726150513, 0.6669999361038208, 0.5560001134872437, 0.8709999322891235, 0.722000002861023, 0.7600001096725464,
			0.6480001211166382, 0.7600001096725464, 0.6850000619888306, 0.6480001211166382, 0.5740000009536743, 0.722000002861023, 0.6110001802444458, 0.9259999990463257,
			0.6110001802444458, 0.6480001211166382, 0.6110001802444458, 0.25899994373321533, 0.3330000638961792, 0.25899994373321533, 0.6000000238418579, 0.5000001192092896,
			0.22200000286102295, 0.5370000600814819, 0.593000054359436, 0.5370000600814819, 0.593000054359436, 0.5370000600814819, 0.2960001230239868, 0.5740000009536743,
			0.5560001134872437, 0.22200000286102295, 0.22200000286102295, 0.5190001726150513, 0.22200000286102295, 0.8530000448226929, 0.5560001134872437, 0.5740000009536743,
			0.593000054359436, 0.593000054359436, 0.3330000638961792, 0.5000001192092896, 0.31500017642974854, 0.5560001134872437, 0.5000001192092896, 0.7580000162124634,
			0.5180000066757202, 0.5000001192092896, 0.4800001382827759, 0.3330000638961792, 0.22200000286102295, 0.3330000638961792, 0.6000000238418579, 0
		};
		const double AverageCharProportion = 0.5131400561332703;

		static readonly double[] BoldCharacterProportions = {
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0.27799999713897705, 0.27799999713897705, 0.27799999713897705, 0.27799999713897705, 0.27799999713897705, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0.27799999713897705, 0.27799999713897705, 0.46299993991851807, 0.5560001134872437, 0.5560001134872437, 1.0000001192092896, 0.6850000619888306, 0.27799999713897705,
			0.2960001230239868, 0.2960001230239868, 0.40700018405914307, 0.6000000238418579, 0.27799999713897705, 0.40700018405914307, 0.27799999713897705, 0.37099993228912354,
			0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437, 0.5560001134872437,
			0.5560001134872437, 0.5560001134872437, 0.27799999713897705, 0.27799999713897705, 0.6000000238418579, 0.6000000238418579, 0.6000000238418579, 0.5560001134872437,
			0.8000000715255737, 0.6850000619888306, 0.7040001153945923, 0.7410000562667847, 0.7410000562667847, 0.6480001211166382, 0.593000054359436, 0.7589999437332153,
			0.7410000562667847, 0.29499995708465576, 0.5560001134872437, 0.722000002861023, 0.593000054359436, 0.9070001840591431, 0.7410000562667847, 0.777999997138977,
			0.6669999361038208, 0.777999997138977, 0.722000002861023, 0.6490000486373901, 0.6110001802444458, 0.7410000562667847, 0.6299999952316284, 0.9440001249313354,
			0.6669999361038208, 0.6669999361038208, 0.6480001211166382, 0.3330000638961792, 0.37099993228912354, 0.3330000638961792, 0.6000000238418579, 0.5000001192092896,
			0.25899994373321533, 0.5740000009536743, 0.6110001802444458, 0.5740000009536743, 0.6110001802444458, 0.5740000009536743, 0.3330000638961792, 0.6110001802444458,
			0.593000054359436, 0.2580000162124634, 0.27799999713897705, 0.5740000009536743, 0.2580000162124634, 0.906000018119812, 0.593000054359436, 0.6110001802444458,
			0.6110001802444458, 0.6110001802444458, 0.3890000581741333, 0.5370000600814819, 0.3520001173019409, 0.593000054359436, 0.5200001001358032, 0.8140000104904175,
			0.5370000600814819, 0.5190001726150513, 0.5190001726150513, 0.3330000638961792, 0.223000168800354, 0.3330000638961792, 0.6000000238418579, 0
		};
		const double BoldAverageCharProportion = 0.5346300601959229;
	}
}
