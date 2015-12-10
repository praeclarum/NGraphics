using System;
using AppKit;
using Foundation;
using System.Collections.Generic;

namespace NGraphics.Editor
{
	public class Style
	{
		NSDictionary baseAttrs = FontColorAttrs ("Menlo", 14, NSColor.FromRgba (0.25f, 0.25f, 0.25f, 1));
		NSDictionary wsAttrs = FontAttrs ("Helvetica Neue", 14);
		NSDictionary identAttrs = FontAttrs ("Helvetica Neue", 14);
		NSDictionary kwdAttrs = FontColorAttrs ("Helvetica Neue Bold", 14, NSColor.FromRgba (0.35f, 0.35f, 0.35f, 1));
		NSDictionary valAttrs = FontColorAttrs ("Menlo Bold", 14, NSColor.FromRgba (0x64/290.0f, 0x95/290.0f, 0xF3/290.0f, 1));
		NSDictionary gAttrs = FontColorAttrs ("Helvetica Neue Bold", 14, NSColor.FromRgba (0.05f, 0.05f, 0.05f, 1));
		NSDictionary commentAttrs = FontAttrs ("Georgia", 14);

		HashSet<string> keywords = new HashSet<string> {
			"class", "delegate", "do", "double", "event", "float", "for", "if", "int", "let", "new", "private", "protected", "public", "return", "using", "var", "void", "while",
		};
		HashSet<string> valwords = new HashSet<string> {
			"true", "false",
		};
		HashSet<string> gwords = new HashSet<string> ();

		public Style ()
		{
			var asms = new [] { typeof(IDrawable).Assembly, typeof(Platforms).Assembly };
			foreach (var a in asms) {
				foreach (var t in a.ExportedTypes) {
					gwords.Add (t.Name);
					foreach (var m in t.GetMembers ()) {
						gwords.Add (m.Name);
					}
				}
			}
		}

		static NSDictionary FontAttrs (string name, float size)
		{
			return NSDictionary.FromObjectsAndKeys (
				new NSObject[] { NSFont.FromFontName (name, size), },
				new NSObject[] { NSStringAttributeKey.Font, });
		}

		static NSDictionary ColorAttrs (NSColor color)
		{
			return NSDictionary.FromObjectsAndKeys (
				new NSObject[] { color },
				new NSObject[] { NSStringAttributeKey.ForegroundColor });
		}

		static NSDictionary FontColorAttrs (string name, float size, NSColor color)
		{
			return NSDictionary.FromObjectsAndKeys (
				new NSObject[] { NSFont.FromFontName (name, size), color },
				new NSObject[] { NSStringAttributeKey.Font, NSStringAttributeKey.ForegroundColor });
		}

		public void FormatCode (NSMutableAttributedString fs)
		{
			var s = fs.Value;
			var n = s.Length;
			var p = 0;

			fs.SetAttributes (baseAttrs, new NSRange (0, n));

			if (n == 0)
				return;

			Func<char, bool> isDigit = ch => {
				var l = char.ToLowerInvariant (ch);
				return l == '.' || l == 'a' || l=='b'||l=='c'||l=='d'||l=='e'||l=='f'||l=='x'||char.IsDigit (ch);
			};

			while (p < n) {
				var wsp = p;
				while (p < n && char.IsWhiteSpace (s [p])) {
					p++;
				}
				if (p != wsp) {
					fs.AddAttributes (wsAttrs, new NSRange (wsp, p - wsp));
				}
				if (p >= n)
					break;
				
				var ch = s [p];
				if (ch == '_' || char.IsLetter (ch)) {

					var sp = p;
					while (p < n && (s[p] == '_' || char.IsLetterOrDigit (s[p]))) {
						p++;
					}
					var len = p - sp;
					var ss = s.Substring (sp, len);
					if (keywords.Contains (ss)) {
						fs.AddAttributes (kwdAttrs, new NSRange (sp, len));
					} else if (valwords.Contains (ss)) {
						fs.AddAttributes (valAttrs, new NSRange (sp, len));
					} else if (gwords.Contains (ss)) {
						fs.AddAttributes (gAttrs, new NSRange (sp, len));
					} else {
						fs.AddAttributes (identAttrs, new NSRange (sp, len));
					}

				} else if (char.IsDigit (ch)) {
					var sp = p;
					while (p < n && isDigit (s[p])) {
						p++;
					}
					fs.AddAttributes (valAttrs, new NSRange (sp, p - sp));

				} else {
					p++;
				}
			}
		}
	}
}

