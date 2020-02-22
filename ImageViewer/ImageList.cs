using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace ImageViewer
{
    public class ImageList
    {
        protected readonly List<string> IMAGE_EXTENTIONS = new List<string>(new string[] { ".bmp", ".jpg", ".jpeg", ".png" });
        protected List<string> imageList = new List<string>();
        public int lastUpdatedFileIndex;

        public ImageList()
        {
            lastUpdatedFileIndex = -1;
        }

        public ImageList(string folderPath)
        {
            this.findImages(folderPath);
        }

        //private ImageList(List<string> list)
        //{
        //    this.imageList = list;
        //}

        private void findImages(string folderPath)
        {
            DateTime lastUpdated = new DateTime(0);
            string[] allPathes = System.IO.Directory.GetFiles(folderPath);

            foreach (string path in allPathes)
            {
                string extension = System.IO.Path.GetExtension(path);

                if (IMAGE_EXTENTIONS.Contains(extension.ToLower()))
                {
                    Console.WriteLine(path);
                    imageList.Add(System.IO.Path.GetFullPath(path));

                    if (lastUpdated < System.IO.File.GetLastWriteTime(path))
                    {
                        lastUpdated = System.IO.File.GetLastWriteTime(path);
                        lastUpdatedFileIndex = imageList.Count - 1;
                    }
                }
                imageList.Sort();
            }
        }

        public int findIndex(string filepath)
        {
            int index = 0;

            if (filepath == null)
                return -1;

            filepath = System.IO.Path.GetFileName(filepath);
            foreach (string imagePath in imageList)
            {
                string imageFilename = System.IO.Path.GetFileName(imagePath);

                if (imageFilename == filepath)
                    return index;

                index += 1;
            }

            return -1;
        }

        public ImageList GetRange(int index, int count)
        {
            ImageList newList = new ImageList();
            newList.imageList = imageList.GetRange(index, count);

            return newList;
        }

        public bool contains(string filepath)
        {
            if (findIndex(filepath) >= 0)
                return true;
            else
                return false;
        }

        public int Count
        {
            get { return imageList.Count; }
        }

        public string this[int index]
        {
            get { return imageList[index]; }
        }

    }

    public class ZipImageList : ImageList
    {
        ZipArchive archive;

        public ZipImageList(string zipPath)
        {
            archive = ZipFile.OpenRead(zipPath);

            foreach (ZipArchiveEntry e in archive.Entries)
            {
                string extension = System.IO.Path.GetExtension(e.FullName);

                if (IMAGE_EXTENTIONS.Contains(extension.ToLower()))
                {
                    imageList.Add(e.FullName);
                    Console.WriteLine(e.FullName);
                }
            }
        }

        public ImageLoader getImageLoader()
        {
            return new ZipImageLoader(archive);
        }
    }
}
