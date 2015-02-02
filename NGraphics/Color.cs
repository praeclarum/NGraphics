using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	[System.Runtime.InteropServices.StructLayout (System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct Color
	{
		public readonly byte B;
		public readonly byte G;
		public readonly byte R;
        public readonly byte A;

		public double Red { get { return R / 255.0; } }
		public double Green { get { return G / 255.0; } }
		public double Blue { get { return B / 255.0; } }
		public double Alpha { get { return A / 255.0; } }

		public int Argb {
			get {
				return (A << 24) | (R << 16) | (G << 8) | B;
			}
		}

		Color (byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public Color (double red, double green, double blue, double alpha)
		{
			R = (byte)(Math.Min (255, Math.Max (0, (int)(red * 255 + 0.5))));
			G = (byte)(Math.Min (255, Math.Max (0, (int)(green * 255 + 0.5))));
			B = (byte)(Math.Min (255, Math.Max (0, (int)(blue * 255 + 0.5))));
			A = (byte)(Math.Min (255, Math.Max (0, (int)(alpha * 255 + 0.5))));
		}
		public Color (double white, double alpha)
		{
			var W = (byte)(Math.Min (255, Math.Max (0, (int)(white * 255 + 0.5))));
			R = W;
			G = W;
			B = W;
			A = (byte)(Math.Min (255, Math.Max (0, (int)(alpha * 255 + 0.5))));
		}

		public Color WithAlpha (double alpha)
		{
			var a = (byte)(Math.Min (255, Math.Max (0, (int)(alpha * 255 + 0.5))));
			return new Color (R, G, B, a);
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
		public static readonly Color DarkGray = new Color (0.25, 0.25, 0.25, 1);
		public static readonly Color Gray = new Color (0.5, 0.5, 0.5, 1);
		public static readonly Color LightGray = new Color (0.75, 0.75, 0.75, 1);
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
			names ["transparent"] = Colors.Clear;
			names ["clear"] = Colors.Clear;
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

