using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ImageViewer
{
    class ImageCache
    {
        public string path;
        public Image image;
    }

    class ImageLoader
    {
        const int MAX_CACHE_IMAGES = 3;

        private List<ImageCache> imageLRUCache;

        public ImageLoader()
        {
            imageLRUCache = new List<ImageCache>();
        }

        public Image loadImage(string path)
        {
            Image image = findCache(path);
            if (image != null)
                return image;

            ImageCache c = new ImageCache();
            try
            {
                using (FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    c.path = path;
                    c.image = Image.FromStream(stream);
                }
            }
            catch (Exception)
            {
                return null;
            }

            imageLRUCache.Insert(0, c);

            if (imageLRUCache.Count > MAX_CACHE_IMAGES)
                imageLRUCache.RemoveAt(imageLRUCache.Count - 1);

            Console.WriteLine("read file");
            return c.image;
        }

        private Image findCache(string path)
        {
            for (int i = 0; i < imageLRUCache.Count; i++)
            {
                ImageCache c = imageLRUCache[i];

                if (c.path == path)
                {
                    if (i != 0)
                    {
                        imageLRUCache.RemoveAt(i);
                        imageLRUCache.Insert(0, c);
                    }
                    Console.WriteLine("use cache");
                    return c.image;
                }
            }
            return null;
        }
    }
}
