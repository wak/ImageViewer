﻿using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ImageViewer
{
    public class ImageFile : IComparable<ImageFile>
    {
        private static readonly ImageLoader imageLoader = new ImageLoader();

        const string REGEX = @"(?<filename>.*?)\s+(?<separator>-+)\s*(?<comment>.*)(?<extension>\..*?)";

        public string AbsPath;
        public string Filename;
        public string FilenameWithoutComment;
        public string Comment;
        public int CommentLevel;

        public ImageFile(string path)
        {
            updateProperty(path);
        }

        private void updateProperty(string path)
        {
            this.AbsPath = System.IO.Path.GetFullPath(path);
            this.Filename = System.IO.Path.GetFileName(path);

            var mc = Regex.Matches(Filename, REGEX);

            if (mc.Count > 0)
            {
                Comment = mc[0].Groups["comment"].Value;
                CommentLevel = mc[0].Groups["separator"].Length;
                FilenameWithoutComment = mc[0].Groups["filename"].Value + mc[0].Groups["extension"].Value;
            }
            else
            {
                Comment = null;
                CommentLevel = 0;
                FilenameWithoutComment = Filename;
            }
        }

        public virtual Image LoadImage()
        {
            return imageLoader.loadImage(AbsPath);
        }

        public void ChangeLevel(int level)
        {
            if (level <= 0)
                return;

            if (level > 0 && Comment == null)
                return;

            Rename(BuildFilePath(level));
        }

        public bool hasComment()
        {
            return Comment != null;
        }

        private bool Rename(string newName)
        {
            if (FSUtility.rename(AbsPath, newName))
            {
                updateProperty(newName);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string BuildFilePath(int newLevel)
        {
            string newFilename;

            if (newLevel <= 0)
            {
                newFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AbsPath), FilenameWithoutComment);
            }
            else
            {
                string extension = System.IO.Path.GetExtension(AbsPath);
                string head = System.IO.Path.GetFileNameWithoutExtension(FilenameWithoutComment);
                string separator = new string('-', newLevel);
                newFilename = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(AbsPath), 
                    string.Format("{0} {1} {2}{3}", head, separator, Comment, extension)
                );
            }

            return newFilename;
        }

        public int CompareTo(ImageFile other)
        {
            return AbsPath.CompareTo(other.AbsPath);
        }
    }

    public class ZippedImageFile : ImageFile
    {
        private ZipImageLoader ZipImageLoader;

        public ZippedImageFile(string path, ZipImageLoader zipImageLoader) : base(path)
        {
            AbsPath = path;
            ZipImageLoader = zipImageLoader;
        }

        public override Image LoadImage()
        {
            return ZipImageLoader.loadImage(AbsPath);
        }
    }
}
