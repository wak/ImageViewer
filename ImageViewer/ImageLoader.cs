using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace ImageViewer
{
    public class ImageCache
    {
        public string path;
        public Image image;
    }

    public class ImageLoader
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
                loadImage2(path, c);
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

        protected virtual void loadImage2(string path, ImageCache c)
        {
            using (FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                c.path = path;
                c.image = Image.FromStream(stream);
            }
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

    public class ZipImageLoader : ImageLoader
    {
        ZipArchive archive;

        public ZipImageLoader(ZipArchive a)
        {
            archive = a;
        }

        protected override void loadImage2(string path, ImageCache c)
        {
            ZipArchiveEntry e = archive.GetEntry(path);
            if (e == null)
                throw new Exception();

            c.path = path;
            c.image = Image.FromStream(e.Open());
        }
    }
}
