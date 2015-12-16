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

		[System.Runtime.Serialization.IgnoreDataMember]
		public double Red { get { return R / 255.0; } set { R = Round (value); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Green { get { return G / 255.0; } set { G = Round (value); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Blue { get { return B / 255.0; } set { B = Round (value); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Alpha { get { return A / 255.0; } set { A = Round (value); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Hue {
			get { return ToHSB ()[0]; }
			set {
				var hsb = ToHSB ();
				var c = Color.FromHSB (value, hsb [1], hsb [2], 1);
				R = c.R; G = c.G; B = c.B;
			}
		}
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Saturation {
			get { return ToHSB ()[1]; }
			set {
				var hsb = ToHSB ();
				var c = Color.FromHSB (value, hsb [1], hsb [2], 1);
				R = c.R; G = c.G; B = c.B;
			}
		}
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Brightness {
			get { return Value; }
			set {
				Value = value;
			}
		}
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Value {
			get { return ToHSV ()[2]; }
			set {
				var c = WithValue (value);
				R = c.R; G = c.G; B = c.B;
			}
		}

		static byte Round (double c)
		{
			return (byte)(Math.Min (255, Math.Max (0, (int)(c * 255 + 0.5))));
		}

		[System.Runtime.Serialization.IgnoreDataMember]
		public int Argb {
			get {
				return (A << 24) | (R << 16) | (G << 8) | B;
			}
		}
		[System.Runtime.Serialization.IgnoreDataMember]
		public int Rgba {
			get {
				return (R << 24) | (G << 16) | (B << 8) | A;
			}
		}
		[System.Runtime.Serialization.IgnoreDataMember]
		public int Abgr {
			get {
				return (A << 24) | (B << 16) | (G << 8) | R;
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

		public override bool Equals (object obj)
		{
			if (obj is Color) {
				return this == ((Color)obj);
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return R.GetHashCode () * 5 + G.GetHashCode () * 13 + B.GetHashCode () * 19 + A.GetHashCode () * 29;
		}

		public static bool operator == (Color a, Color b)
		{
			return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
		}

		public static bool operator != (Color a, Color b)
		{
			return a.R != b.R || a.G != b.G || a.B != b.B || a.A != b.A;
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

		public Color WithHue (double hue)
		{
			var hsb = ToHSB ();
			return Color.FromHSB (hue, hsb [1], hsb [2], hsb[3]);
		}

		public Color WithSaturation (double saturation)
		{
			var hsb = ToHSB ();
			return Color.FromHSB (hsb[0], saturation, hsb [2], hsb[3]);
		}

		public Color WithBrightness (double brightness)
		{
			return WithValue (brightness);
		}

		public Color WithValue (double value)
		{
			var hsb = ToHSB ();
			return Color.FromHSV (hsb[0], hsb [1], value, hsb[3]);
		}

		public static Color FromRGB (double red, double green, double blue, double alpha = 1)
		{
			return new Color (red, green, blue, alpha);
		}

		public static Color FromWhite (double white, double alpha = 1)
		{
			return new Color (white, white, white, alpha);
		}

		public static Color FromRGB (int rgb)
		{
			var w = 1.0 / 255;
			var r = ((rgb >> 16) & 0xFF) * w;
			var g = ((rgb >>  8) & 0xFF) * w;
			var b = ((rgb >>  0) & 0xFF) * w;
			return new Color (r, g, b, 1);
		}

		public static Color FromHSL (double hue, double saturation, double lightness, double alpha = 1.0)
		{
			var c = (1 - Math.Abs (2 * lightness - 1)) * saturation;
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
			var m = lightness - 0.5*c;
			return new Color (r1 + m, g1 + m, b1 + m, alpha);
		}

		public static Color FromHSB (double hue, double saturation, double brightness, double alpha = 1.0)
		{
			return FromHSV (hue, saturation, brightness, alpha);
		}

		public double[] ToHSB ()
		{
			return ToHSV ();
		}

		public double[] ToHSV ()
		{			
			var r = new double[4];

			int cmax = (R > G) ? R : G;
			if (B > cmax)
				cmax = B;
			int cmin = (R < G) ? R : G;
			if (B < cmin)
				cmin = B;

			var brightness = cmax / 255.0;
			double hue, saturation;
			if (cmax != 0) {
				saturation = (cmax - cmin) / (double)cmax;
			}
			else {
				saturation = 0;
			}
			if (saturation == 0) {
				hue = 0;
			}
			else {
				var redc = (cmax - R) / (double)(cmax - cmin);
				var greenc = (cmax - G) / (double)(cmax - cmin);
				var bluec = (cmax - B) / (double)(cmax - cmin);
				if (R == cmax)
					hue = bluec - greenc;
				else if (G == cmax)
					hue = 2 + redc - bluec;
				else
					hue = 4 + greenc - redc;
				hue = hue / 6;
				if (hue < 0) {
					hue = hue + 1;
				}
			}
			r [0] = hue;
			r [1] = saturation;
			r [2] = brightness;
			r [3] = Alpha;
			return r;
		}

		public static Color FromHSV (double hue, double saturation, double value, double alpha = 1.0)
		{
			var c = saturation * value;
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
			var m = value - c;
			return new Color (r1 + m, g1 + m, b1 + m, alpha);
		}

		public string HtmlString
		{
			get {
				return string.Format ("#{0:X2}{1:X2}{2:X2}", R, G, B);
			}
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

