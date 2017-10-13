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
			names ["transparent"] = Colors.Clear;
			names ["clear"] = Colors.Clear;
			names ["aliceblue"] = new Color(240, 248, 255);
			names ["antiquewhite"] = new Color(250, 235, 215);
			names ["aqua"] = new Color( 0, 255, 255);
			names ["aquamarine"] = new Color(127, 255, 212);
			names ["azure"] = new Color(240, 255, 255);
			names ["beige"] = new Color(245, 245, 220);
			names ["bisque"] = new Color(255, 228, 196);
			names ["black"] = new Color( 0, 0, 0);
			names ["blanchedalmond"] = new Color(255, 235, 205);
			names ["blue"] = new Color( 0, 0, 255);
			names ["blueviolet"] = new Color(138, 43, 226);
			names ["brown"] = new Color(165, 42, 42);
			names ["burlywood"] = new Color(222, 184, 135);
			names ["cadetblue"] = new Color( 95, 158, 160);
			names ["chartreuse"] = new Color(127, 255, 0);
			names ["chocolate"] = new Color(210, 105, 30);
			names ["coral"] = new Color(255, 127, 80);
			names ["cornflowerblue"] = new Color(100, 149, 237);
			names ["cornsilk"] = new Color(255, 248, 220);
			names ["crimson"] = new Color(220, 20, 60);
			names ["cyan"] = new Color( 0, 255, 255);
			names ["darkblue"] = new Color( 0, 0, 139);
			names ["darkcyan"] = new Color( 0, 139, 139);
			names ["darkgoldenrod"] = new Color(184, 134, 11);
			names ["darkgray"] = new Color(169, 169, 169);
			names ["darkgreen"] = new Color( 0, 100, 0);
			names ["darkgrey"] = new Color(169, 169, 169);
			names ["darkkhaki"] = new Color(189, 183, 107);
			names ["darkmagenta"] = new Color(139, 0, 139);
			names ["darkolivegreen"] = new Color( 85, 107, 47);
			names ["darkorange"] = new Color(255, 140, 0);
			names ["darkorchid"] = new Color(153, 50, 204);
			names ["darkred"] = new Color(139, 0, 0);
			names ["darksalmon"] = new Color(233, 150, 122);
			names ["darkseagreen"] = new Color(143, 188, 143);
			names ["darkslateblue"] = new Color( 72, 61, 139);
			names ["darkslategray"] = new Color( 47, 79, 79);
			names ["darkslategrey"] = new Color( 47, 79, 79);
			names ["darkturquoise"] = new Color( 0, 206, 209);
			names ["darkviolet"] = new Color(148, 0, 211);
			names ["deeppink"] = new Color(255, 20, 147);
			names ["deepskyblue"] = new Color( 0, 191, 255);
			names ["dimgray"] = new Color(105, 105, 105);
			names ["dimgrey"] = new Color(105, 105, 105);
			names ["dodgerblue"] = new Color( 30, 144, 255);
			names ["firebrick"] = new Color(178, 34, 34);
			names ["floralwhite"] = new Color(255, 250, 240);
			names ["forestgreen"] = new Color( 34, 139, 34);
			names ["fuchsia"] = new Color(255, 0, 255);
			names ["gainsboro"] = new Color(220, 220, 220);
			names ["ghostwhite"] = new Color(248, 248, 255);
			names ["gold"] = new Color(255, 215, 0);
			names ["goldenrod"] = new Color(218, 165, 32);
			names ["gray"] = new Color(128, 128, 128);
			names ["grey"] = new Color(128, 128, 128);
			names ["green"] = new Color( 0, 128, 0);
			names ["greenyellow"] = new Color(173, 255, 47);
			names ["honeydew"] = new Color(240, 255, 240);
			names ["hotpink"] = new Color(255, 105, 180);
			names ["indianred"] = new Color(205, 92, 92);
			names ["indigo"] = new Color( 75, 0, 130);
			names ["ivory"] = new Color(255, 255, 240);
			names ["khaki"] = new Color(240, 230, 140);
			names ["lavender"] = new Color(230, 230, 250);
			names ["lavenderblush"] = new Color(255, 240, 245);
			names ["lawngreen"] = new Color(124, 252, 0);
			names ["lemonchiffon"] = new Color(255, 250, 205);
			names ["lightblue"] = new Color(173, 216, 230);
			names ["lightcoral"] = new Color(240, 128, 128);
			names ["lightcyan"] = new Color(224, 255, 255);
			names ["lightgoldenrodyellow"] = new Color(250, 250, 210);
			names ["lightgray"] = new Color(211, 211, 211);
			names ["lightgreen"] = new Color(144, 238, 144);
			names ["lightgrey"] = new Color(211, 211, 211);
			names ["lightpink"] = new Color(255, 182, 193);
			names ["lightsalmon"] = new Color(255, 160, 122);
			names ["lightseagreen"] = new Color( 32, 178, 170);
			names ["lightskyblue"] = new Color(135, 206, 250);
			names ["lightslategray"] = new Color(119, 136, 153);
			names ["lightslategrey"] = new Color(119, 136, 153);
			names ["lightsteelblue"] = new Color(176, 196, 222);
			names ["lightyellow"] = new Color(255, 255, 224);
			names ["lime"] = new Color( 0, 255, 0);
			names ["limegreen"] = new Color( 50, 205, 50);
			names ["linen"] = new Color(250, 240, 230);
			names ["magenta"] = new Color(255, 0, 255);
			names ["maroon"] = new Color(128, 0, 0);
			names ["mediumaquamarine"] = new Color(102, 205, 170);
			names ["mediumblue"] = new Color( 0, 0, 205);
			names ["mediumorchid"] = new Color(186, 85, 211);
			names ["mediumpurple"] = new Color(147, 112, 219);
			names ["mediumseagreen"] = new Color( 60, 179, 113);
			names ["mediumslateblue"] = new Color(123, 104, 238);
			names ["mediumspringgreen"] = new Color( 0, 250, 154);
			names ["mediumturquoise"] = new Color( 72, 209, 204);
			names ["mediumvioletred"] = new Color(199, 21, 133);
			names ["midnightblue"] = new Color( 25, 25, 112);
			names ["mintcream"] = new Color(245, 255, 250);
			names ["mistyrose"] = new Color(255, 228, 225);
			names ["moccasin"] = new Color(255, 228, 181);
			names ["navajowhite"] = new Color(255, 222, 173);
			names ["navy"] = new Color( 0, 0, 128);
			names ["oldlace"] = new Color(253, 245, 230);
			names ["olive"] = new Color(128, 128, 0);
			names ["olivedrab"] = new Color(107, 142, 35);
			names ["orange"] = new Color(255, 165, 0);
			names ["orangered"] = new Color(255, 69, 0);
			names ["orchid"] = new Color(218, 112, 214);
			names ["palegoldenrod"] = new Color(238, 232, 170);
			names ["palegreen"] = new Color(152, 251, 152);
			names ["paleturquoise"] = new Color(175, 238, 238);
			names ["palevioletred"] = new Color(219, 112, 147);
			names ["papayawhip"] = new Color(255, 239, 213);
			names ["peachpuff"] = new Color(255, 218, 185);
			names ["peru"] = new Color(205, 133, 63);
			names ["pink"] = new Color(255, 192, 203);
			names ["plum"] = new Color(221, 160, 221);
			names ["powderblue"] = new Color(176, 224, 230);
			names ["purple"] = new Color(128, 0, 128);
			names ["red"] = new Color(255, 0, 0);
			names ["rosybrown"] = new Color(188, 143, 143);
			names ["royalblue"] = new Color( 65, 105, 225);
			names ["saddlebrown"] = new Color(139, 69, 19);
			names ["salmon"] = new Color(250, 128, 114);
			names ["sandybrown"] = new Color(244, 164, 96);
			names ["seagreen"] = new Color( 46, 139, 87);
			names ["seashell"] = new Color(255, 245, 238);
			names ["sienna"] = new Color(160, 82, 45);
			names ["silver"] = new Color(192, 192, 192);
			names ["skyblue"] = new Color(135, 206, 235);
			names ["slateblue"] = new Color(106, 90, 205);
			names ["slategray"] = new Color(112, 128, 144);
			names ["slategrey"] = new Color(112, 128, 144);
			names ["snow"] = new Color(255, 250, 250);
			names ["springgreen"] = new Color( 0, 255, 127);
			names ["steelblue"] = new Color( 70, 130, 180);
			names ["tan"] = new Color(210, 180, 140);
			names ["teal"] = new Color( 0, 128, 128);
			names ["thistle"] = new Color(216, 191, 216);
			names ["tomato"] = new Color(255, 99, 71);
			names ["turquoise"] = new Color( 64, 224, 208);
			names ["violet"] = new Color(238, 130, 238);
			names ["wheat"] = new Color(245, 222, 179);
			names ["white"] = new Color(255, 255, 255);
			names ["whitesmoke"] = new Color(245, 245, 245);
			names ["yellow"] = new Color(255, 255, 0);
			names ["yellowgreen"] = new Color(154, 205, 50);
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

