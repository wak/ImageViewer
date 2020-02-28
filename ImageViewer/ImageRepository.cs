using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace ImageViewer
{
    public class ImageRepository : IEnumerable<ImageFile>
    {
        protected readonly List<string> IMAGE_EXTENTIONS = new List<string>(new string[] { ".bmp", ".jpg", ".jpeg", ".png" });
        protected List<ImageFile> imageList = new List<ImageFile>();
        public int lastUpdatedFileIndex;

        public string repoPath = null;

        public ImageRepository()
        {
            clear();
        }

        public ImageRepository(string folderPath)
        {
            this.repoPath = folderPath;
            this.findImages(folderPath);
        }

        public void reload()
        {
            if (repoPath != null)
            {
                clear();
                this.findImages(repoPath);
            }
        }

        private void clear()
        {
            lastUpdatedFileIndex = -1;
            imageList.Clear();
        }

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
                    imageList.Add(new ImageFile(System.IO.Path.GetFullPath(path)));

                    if (lastUpdated < System.IO.File.GetLastWriteTime(path))
                    {
                        lastUpdated = System.IO.File.GetLastWriteTime(path);
                        lastUpdatedFileIndex = imageList.Count - 1;
                    }
                }
                else if (extension.EndsWith(".iv"))
                {
                    imageList.Add(new MetaFile(System.IO.Path.GetFullPath(path)));
                }
                imageList.Sort();
            }
        }

        public int findIndex(string absPath)
        {
            int index = 0;

            if (absPath == null)
                return -1;

            foreach (var imagePath in imageList)
            {
                if (imagePath.AbsPath == absPath)
                    return index;

                index += 1;
            }

            return -1;
        }

        public ImageFile FindImage(string filepath)
        {
            int index = findIndex(filepath);

            if (index == -1)
                return null;
            else
                return this[index];
        }

        public ImageRepository GetRange(int index, int count)
        {
            ImageRepository newList = new ImageRepository();
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

        public IEnumerator<ImageFile> GetEnumerator()
        {
            return new ImageListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ImageListEnumerator(this);
        }

        public int Count
        {
            get { return imageList.Count; }
        }

        public ImageFile this[int index]
        {
            get { return imageList[index]; }
        }

        private class ImageListEnumerator : IEnumerator<ImageFile>
        {
            private ImageRepository imageList;
            private int currentIndex = -1;

            public ImageListEnumerator(ImageRepository imageList)
            {
                this.imageList = imageList;
            }

            public ImageFile Current { get { return imageList[currentIndex]; } }
            object IEnumerator.Current { get { return imageList[currentIndex]; } }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                currentIndex += 1;

                if (imageList.Count <= currentIndex)
                    return false;
                else
                    return true;
            }

            public void Reset()
            {
                currentIndex = -1;
            }
        }
    }

    public class ZipImageList : ImageRepository
    {
        ZipArchive archive;

        public ZipImageList(string zipPath)
        {
            archive = ZipFile.OpenRead(zipPath);
            var zipImageLoader = new ZipImageLoader(archive);

            foreach (ZipArchiveEntry e in archive.Entries)
            {
                string extension = System.IO.Path.GetExtension(e.FullName);

                if (IMAGE_EXTENTIONS.Contains(extension.ToLower()))
                {
                    imageList.Add(new ZippedImageFile(e.FullName, zipImageLoader));
                    Console.WriteLine(e.FullName);
                }
            }
        }
    }
}
