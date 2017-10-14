using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace NGraphics
{
	public class CodeWriter
	{
		readonly IFormatProvider icult = System.Globalization.CultureInfo.InvariantCulture;
		readonly TextWriter w;
		readonly string indentText = "\t";
		int indentLevel = 0;
		bool needsIndent = false;
		public Encoding Encoding { get { return w.Encoding; } }
		public CodeWriter (TextWriter writer, string indentText)
		{
			w = writer;
			this.indentText = indentText;
		}
		public void WriteLine (string format, params object[] args)
		{
			WriteLine (string.Format (icult, format, args));
		}
		public void WriteLine (string text)
		{
			WriteIndent ();
			w.WriteLine (text);
			needsIndent = true;
		}
		public void WriteLine ()
		{
			WriteIndent ();
			w.WriteLine ();
			needsIndent = true;
		}
		public void Write (string format, params object[] args)
		{
			Write (string.Format (icult, format, args));
		}
		public void Write (string text)
		{
			WriteIndent ();
			w.Write (text);
		}
		public void Write ()
		{
		}

		public void Indent ()
		{
			indentLevel++;
		}
		public void Outdent ()
		{
			indentLevel--;
		}
		void WriteIndent ()
		{
			if (!needsIndent)
				return;
			for (var i = 0; i < indentLevel; i++) {
				w.Write (indentText);
			}
			needsIndent = false;
		}
	}
	
}