using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Shapes = Windows.UI.Xaml.Shapes;

namespace NGraphics
{
	/// <summary>
	/// Wraps a XAML Canvas whose children are created using the ICanvas draw functions.
	/// Required to run on the UI thread.
	/// </summary>
	public class CanvasCanvas : ICanvas
	{
		readonly Canvas canvas;
		readonly List<Child> children = new List<Child> ();

		readonly Stack<State> stateStack = new Stack<State> ();

		public Canvas Canvas { get { return canvas; } }

		class State
		{
			public Transform Transform;
		}

		public CanvasCanvas (Canvas canvas)
		{
			this.canvas = canvas;
			stateStack.Push (new State { Transform = new IdentityTransform (), });
			ClassifyChildren ();
		}

		public void SaveState ()
		{
			stateStack.Push (new State { Transform = stateStack.Peek ().Transform, });
		}

		public void Transform (Transform transform)
		{
			var state = stateStack.Peek ();
			state.Transform = new AggregateTransform (transform, state.Transform);
		}

		public void RestoreState ()
		{
			if (stateStack.Count > 1) {
				stateStack.Pop ();
			}
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			var ch = GetChild (ChildType.Text);
			var s = (Shapes.Rectangle)ch.Shape;
			FormatShape (s, pen, brush);
		}
		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			var ch = GetChild (ChildType.Path);
			var s = (Shapes.Rectangle)ch.Shape;
			FormatShape (s, pen, brush);
		}
		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			var ch = GetChild (ChildType.Rectangle);
			var s = (Shapes.Rectangle)ch.Shape;
			s.Width = frame.Width;
			s.Height = frame.Height;
			FormatShape (s, pen, brush);
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			var ch = GetChild (ChildType.Ellipse);
			var s = (Shapes.Rectangle)ch.Shape;
			s.Width = frame.Width;
			s.Height = frame.Height;
			FormatShape (s, pen, brush);
		}
		public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			var ch = GetChild (ChildType.Image);
			var s = (Shapes.Rectangle)ch.Shape;
			s.Width = frame.Width;
			s.Height = frame.Height;
		}

		void FormatShape (Shapes.Shape shape, Pen pen, Brush brush)
		{
		}

		Child GetChild (ChildType type)
		{
			while (nextChildIndex < children.Count && children[nextChildIndex].Type != type) {
				// TODO: This shape is out of order
				nextChildIndex++;
			}

			if (nextChildIndex >= children.Count) {
				FrameworkElement shape;
				switch (type) {
					case ChildType.Rectangle: shape = new Shapes.Rectangle (); break;
					case ChildType.Ellipse: shape = new Shapes.Ellipse (); break;
					case ChildType.Path: shape = new Shapes.Path (); break;
					case ChildType.Image: shape = new Image (); break;
					case ChildType.Text: shape = new TextBlock (); break;
					default: throw new NotSupportedException (type + " not supported");
				}
				var ch = new Child {
					Type = type,
					Shape = shape,
				};
				children.Add (ch);
				nextChildIndex = children.Count;
				return ch;
			}
			else {
				var ch = children[nextChildIndex];
				nextChildIndex++;
				return ch;
			}
		}

		void ClassifyChildren ()
		{
			// TODO: Reuse existing children
		}

		enum ChildType
		{
			Path,
			Rectangle,
			Ellipse,
			Image,
			Text,
		}

		int nextChildIndex = 0;

		class Child
		{
			public ChildType Type;
			public FrameworkElement Shape;
		}
	}
}
