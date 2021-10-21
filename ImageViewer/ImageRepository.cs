using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace ImageViewer
{
    public static class ImageRepositoryFactory
    {
        public static ImageRepository openRepository(string path, bool recursive)
        {
            if (Path.GetExtension(path).ToLower().EndsWith(".zip"))
                return new ZippedImageRepository(path, recursive);
            else
                return new ImageRepository(path, recursive);
        }
    }

    public class ImageRepository : IEnumerable<ImageFile>
    {
        protected readonly List<string> IMAGE_EXTENTIONS = new List<string>(new string[] { ".bmp", ".jpg", ".jpeg", ".png", ".emf" });
        public List<ImageFile> imageList = new List<ImageFile>();
        public int lastUpdatedFileIndex;

        public string repoPath = null;
        public ImageTree tree = null;

        public bool Recursive = false;

        protected string filter = null;

        private bool isVirtualRepository = true;
        public bool IsVirtualRepository
        {
            get { return isVirtualRepository; }
            set { isVirtualRepository = value; reload(); }
        }

        public ImageRepository()
        {
            clear();
            tree = new ImageTree(this);
        }

        public ImageRepository(string folderPath, bool recursive)
        {
            this.repoPath = folderPath;
            this.Recursive = recursive;
            tree = new ImageTree(this);
            reload();
        }

        public virtual void reload(string filter = null)
        {
            if (filter != null)
                filter = filter.ToLower();
            this.filter = filter;

            if (repoPath != null)
            {
                clear();
                findImages(repoPath);
                tree.reload();
            }
        }

        public virtual bool IsReadonly()
        {
            return false;
        }

        public string repoName()
        {
            return Path.GetFileName(repoPath);
        }

        private void clear()
        {
            lastUpdatedFileIndex = -1;
            imageList.Clear();
            if (tree != null)
                tree.clear();
        }

        private void findImages(string folderPath)
        {
            DateTime lastUpdated = new DateTime(0);
            string[] allPathes = System.IO.Directory.GetFiles(folderPath);

            Array.Sort<string>(allPathes);

            var list = new List<ImageFile>();
            foreach (string path in allPathes)
            {
                string extension = System.IO.Path.GetExtension(path);

                if (filter != null && System.IO.Path.GetFileName(path).ToLower().IndexOf(filter) < 0)
                    continue;

                if (IMAGE_EXTENTIONS.Contains(extension.ToLower()))
                {
                    // Console.WriteLine(path);
                    list.Add(new ImageFile(System.IO.Path.GetFullPath(path)));

                    if (lastUpdated < System.IO.File.GetLastWriteTime(path))
                    {
                        lastUpdated = System.IO.File.GetLastWriteTime(path);
                        lastUpdatedFileIndex = list.Count - 1;
                    }
                }
                else if (extension.EndsWith(".iv"))
                {
                    list.Add(new MetaFile(System.IO.Path.GetFullPath(path)));
                }
            }
            list.Sort();
            imageList.AddRange(list);

            if (Recursive)
            {
                try
                {
                    var dirs = new List<string>(Directory.GetDirectories(folderPath));
                    dirs.Sort();
                    foreach (var subdir in dirs)
                        findImages(subdir);
                }
                catch (Exception)
                {
                    // nothing to do
                }
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

    public class ZippedImageRepository : ImageRepository
    {
        ZipArchive archive;

        public ZippedImageRepository(string zipPath, bool recursive)
        {
            this.repoPath = zipPath;
            this.Recursive = recursive;
            reload();
            tree = new ImageTree(this);
        }

        public override void reload(string filter = null)
        {
            imageList.Clear();

            if (filter != null)
                filter = filter.ToLower();
            this.filter = filter;

            archive = ZipFile.OpenRead(repoPath);
            var zipImageLoader = new ZipImageLoader(archive);

            foreach (ZipArchiveEntry e in archive.Entries)
            {
                string extension = System.IO.Path.GetExtension(e.FullName);

                if (filter != null && e.Name.ToLower().IndexOf(filter) < 0)
                    continue;

                if (IMAGE_EXTENTIONS.Contains(extension.ToLower()) || extension.EndsWith(".iv"))
                {
                    imageList.Add(new ZippedImageFile(e.FullName, zipImageLoader));
                }
            }
            tree = new ImageTree(this);
        }

        public override bool IsReadonly()
        {
            return true;
        }
    }
}
