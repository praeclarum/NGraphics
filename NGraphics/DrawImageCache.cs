using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	/// <summary>
	/// Caches drawings by rendering them to images.
	/// Drawings are identified by a key and are removed from the cache
	/// based upon their access times (least recently accessed entries are removed first).
	/// </summary>
	public class DrawImageCache<TKey>
	{
		class Entry
		{
			public Tuple<Size, double, TKey> FullKey;
			public DateTime LastAccessTime;
			public IImage Image;
		}

		readonly IPlatform platform;

		readonly int maxNumEntries;
		readonly Dictionary<Tuple<Size, double, TKey>, Entry> cache = new Dictionary<Tuple<Size, double, TKey>, Entry> ();

		/// <summary>
		/// Initializes a new instance of the <see cref="DrawImageCache`1"/> class.
		/// </summary>
		/// <param name="platform">The graphics platform used to generate images.</param>
		/// <param name="maxNumEntries">Max number cache entries.</param>
		public DrawImageCache (IPlatform platform, int maxNumEntries)
		{
			this.platform = platform;
			this.maxNumEntries = Math.Max (1, maxNumEntries);
		}

		/// <summary>
		/// Clear the cache.
		/// </summary>
		public void Clear ()
		{
			cache.Clear ();
		}

		/// <summary>
		/// Gets the image by either pulling it from the cache or re-rendering it.
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="key">Key identifying this entry.</param>
		/// <param name="size">The image size.</param>
		/// <param name="scale">The image scale.</param>
		/// <param name="transparent">Whether the image should have a transparent background.</param>
		/// <param name="draw">The function called to draw the entry.</param>
		public IImage GetImage (TKey key, Size size, double scale, bool transparent, Action<ICanvas> draw)
		{
			var now = DateTime.Now;
			var fullKey = Tuple.Create (size, scale, key);
			Entry entry;
			if (!cache.TryGetValue (fullKey, out entry)) {
				var canvas = platform.CreateImageCanvas (size, scale, transparent);
				draw (canvas);
				entry = new Entry {
					FullKey = fullKey,
					LastAccessTime = now,
					Image = canvas.GetImage (),
				};
				//Console.WriteLine ("CACHE " + key);
				cache [fullKey] = entry;
				while (cache.Count > maxNumEntries) {
					var oldest = cache.Values.OrderBy (x => x.LastAccessTime).First ();
					//Console.WriteLine ("EJECT " + oldest.FullKey);
					cache.Remove (oldest.FullKey);
				}
			}
			entry.LastAccessTime = now;
			return entry.Image;
		}
	}
}

