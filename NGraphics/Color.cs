using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	[System.Runtime.InteropServices.StructLayout (System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct Color
	{
		public byte B;
		public byte G;
		public byte R;
        public byte A;

		public double Red { get { return R / 255.0; } set { R = Round (value); } }
		public double Green { get { return G / 255.0; } set { G = Round (value); } }
		public double Blue { get { return B / 255.0; } set { B = Round (value); } }
		public double Alpha { get { return A / 255.0; } set { A = Round (value); } }

		static byte Round (double c)
		{
			return (byte)(Math.Min (255, Math.Max (0, (int)(c * 255 + 0.5))));
		}

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
		public Color (double red, double green, double blue)
		{
			R = (byte)(Math.Min (255, Math.Max (0, (int)(red * 255 + 0.5))));
			G = (byte)(Math.Min (255, Math.Max (0, (int)(green * 255 + 0.5))));
			B = (byte)(Math.Min (255, Math.Max (0, (int)(blue * 255 + 0.5))));
			A = 255;
		}
		public Color (double white, double alpha = 1.0)
		{
			var W = (byte)(Math.Min (255, Math.Max (0, (int)(white * 255 + 0.5))));
			R = W;
			G = W;
			B = W;
			A = (byte)(Math.Min (255, Math.Max (0, (int)(alpha * 255 + 0.5))));
		}
		public Color (string colorString)
		{
			Color color;
			if (Colors.TryParse (colorString, out color)) {
				R = color.R;
				G = color.G;
				B = color.B;
				A = color.A;
			} else {
				throw new ArgumentException ("Bad color string: " + colorString);
			}
		}

		public Color BlendWith (Color other, double otherWeight)
		{
			var t = otherWeight;
			var t1 = 1 - t;
			var r = Red * t1 + other.Red * t;
			var g = Green * t1 + other.Green * t;
			var b = Blue * t1 + other.Blue * t;
			var a = Alpha * t1 + other.Alpha * t;
			return new Color (r, g, b, a);
		}

		public Color WithAlpha (double alpha)
		{
			var a = (byte)(Math.Min (255, Math.Max (0, (int)(alpha * 255 + 0.5))));
			return new Color (R, G, B, a);
		}

		public static Color FromHSB (double hue, double saturation, double brightness, double alpha = 1.0)
		{
			var c = saturation * brightness;
			var hp = hue;
			if (hp < 0)
				hp = 1 - ((-hp) % 1);
			if (hp > 1)
				hp = hp % 1;
			hp *= 6;
			var x = c * (1 - Math.Abs ((hp % 2) - 1));
			double r1, g1, b1;
			if (hp < 1) {
				r1 = c;
				g1 = x;
				b1 = 0;
			}
			else if (hp < 2) {
				r1 = x;
				g1 = c;
				b1 = 0;
			}
			else if (hp < 3) {
				r1 = 0;
				g1 = c;
				b1 = x;
			}
			else if (hp < 4) {
				r1 = 0;
				g1 = x;
				b1 = c;
			}
			else if (hp < 5) {
				r1 = x;
				g1 = 0;
				b1 = c;
			}
			else {
				r1 = c;
				g1 = 0;
				b1 = x;
			}
			var m = brightness - c;
			return new Color (r1 + m, g1 + m, b1 + m, alpha);
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

