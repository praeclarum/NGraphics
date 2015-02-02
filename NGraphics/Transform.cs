using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NGraphics
{
	public abstract class Transform
	{
		public Transform Previous;
		protected Transform (Transform previous = null)
		{
			Previous = previous;
		}
		protected abstract string ToCode ();
		public override string ToString ()
		{
			var s = ToCode ();
			if (Previous != null) {
				s = Previous + " " + s;
			}
			return s;
		}
	}

	public class MatrixTransform : Transform
	{
		public double[] Elements;

		public MatrixTransform (Transform previous = null)
			: base (previous)
		{
			Elements = new double[6];
		}
		public MatrixTransform (double[] elements, Transform previous = null)
			: this (previous)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");
			if (elements.Length != 6)
				throw new ArgumentException ("6 elements were expected");
			Array.Copy (elements, Elements, 6);
		}
		protected override string ToCode ()
		{
			return string.Format (CultureInfo.InvariantCulture, "matrix(...)");
		}
	}

	public class Translate : Transform
	{
		public Size Size;
		public Translate (Size size, Transform previous = null)
			: base (previous)
		{
			Size = size;
		}
		public Translate (double dx, double dy, Transform previous = null)
			: this (new Size (dx, dy), previous)
		{			
		}
		protected override string ToCode ()
		{
			return string.Format (CultureInfo.InvariantCulture, "translate({0}, {1})", Size.Width, Size.Height);
		}
	}

	public class Scale : Transform
	{
		public Size Size;
		public Scale (Size size, Transform previous = null)
			: base (previous)
		{
			Size = size;
		}
		public Scale (double dx, double dy, Transform previous = null)
			: this (new Size (dx, dy), previous)
		{			
		}
		protected override string ToCode ()
		{
			return string.Format (CultureInfo.InvariantCulture, "scale({0}, {1})", Size.Width, Size.Height);
		}
	}

	public class Rotate : Transform
	{
		/// <summary>
		/// The angle in degrees.
		/// </summary>
		public double Angle;
		/// <summary>
		/// Initializes a new instance of the <see cref="NGraphics.Rotate"/> class.
		/// </summary>
		/// <param name="angle">Angle in degrees</param>
		/// <param name="previous">Previous.</param>
		public Rotate (double angle, Transform previous = null)
			: base (previous)
		{
			Angle = angle;
		}
		protected override string ToCode ()
		{
			return string.Format (CultureInfo.InvariantCulture, "rotate({0})", Angle);
		}
	}


}
