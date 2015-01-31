using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public struct Color
	{
		public readonly double Red;
		public readonly double Green;
		public readonly double Blue;
		public readonly double Alpha;

		public int RedByte { get { return (int)(Red * 255 + 0.5); } }
		public int GreenByte { get { return (int)(Green * 255 + 0.5); } }
		public int BlueByte { get { return (int)(Blue * 255 + 0.5); } }
		public int AlphaByte { get { return (int)(Alpha * 255 + 0.5); } }

		public Color (double red, double green, double blue, double alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}
		public Color (double white, double alpha)
		{
			Red = white;
			Green = white;
			Blue = white;
			Alpha = alpha;
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Color ({0}, {1}, {2}, {3})", Red, Green, Blue, Alpha);
		}
	}

	public static class Colors
	{
		public static readonly Color Clear = new Color (0, 0, 0, 0);
		public static readonly Color Black = new Color (0, 0, 0, 1);
		public static readonly Color Gray = new Color (0.5, 0.5, 0.5, 1);
		public static readonly Color White = new Color (1, 1, 1, 1);
		public static readonly Color Red = new Color (1, 0, 0, 1);
		public static readonly Color Orange = new Color (1, 0xA5/255.0, 0, 1);
		public static readonly Color Yellow = new Color (1, 1, 0, 1);
		public static readonly Color Green = new Color (0, 1, 0, 1);
		public static readonly Color Blue = new Color (0, 0, 1, 1);

		static readonly Dictionary<string, Color> names = new Dictionary<string, Color> ();

		static Colors ()
		{
			names ["black"] = Colors.Black;
			names ["white"] = Colors.White;
			names ["red"] = Colors.Red;
			names ["orange"] = Colors.Orange;
		}

		public static bool TryParse (string colorString, out Color color)
		{
			if (string.IsNullOrWhiteSpace (colorString)) {
				color = Colors.Clear;
				return false;
			}

			var s = colorString.Trim ();

			if (s.Length == 7 && s [0] == '#') {

				var icult = CultureInfo.InvariantCulture;

				var r = int.Parse (s.Substring (1, 2), NumberStyles.HexNumber, icult);
				var g = int.Parse (s.Substring (3, 2), NumberStyles.HexNumber, icult);
				var b = int.Parse (s.Substring (5, 2), NumberStyles.HexNumber, icult);

				color = new Color (r / 255.0, g / 255.0, b / 255.0, 1);
				return true;

			}

			Color nc;
			if (names.TryGetValue (s.ToLowerInvariant (), out nc)) {
				color = nc;
				return true;
			}

			color = Colors.Clear;
			return false;
		}
	}
}

