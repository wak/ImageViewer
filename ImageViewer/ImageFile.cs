using System;
using System.Windows.Forms;

namespace ImageViewer
{
    public class ImageFile
    {
        const string REGEX = @"(?<filename>.*?)\s+(?<separator>-+)\s*(?<comment>.*)(?<extension>\..*?)";

        public string absPath;
        public string filename;
        public string filenameWithoutComment;
        public string comment;
        public int commentLevel;

        public ImageFile(string path)
        {
            updateProperty(path);
        }

        private void updateProperty(string path)
        {
            this.absPath = System.IO.Path.GetFullPath(path);
            this.filename = System.IO.Path.GetFileName(path);

            var mc = match();

            if (mc.Count > 0)
            {
                comment = mc[0].Groups["comment"].Value;
                commentLevel = mc[0].Groups["separator"].Length;
                filenameWithoutComment = mc[0].Groups["filename"].Value + mc[0].Groups["extension"].Value;
            }
            else
            {
                comment = null;
                commentLevel = 0;
                filenameWithoutComment = filename;
            }
        }

        public void changeLevel(int level)
        {
            if (level <= 0)
                return;

            if (level > 0 && comment == null)
                return;

            rename(buildFilePath(level));
        }

        private bool rename(string newName)
        {
            if (FSUtility.rename(absPath, newName))
            {
                updateProperty(newName);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string buildFilePath(int newLevel)
        {
            string newFilename;

            if (newLevel <= 0)
            {
                newFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(absPath), filenameWithoutComment);
            }
            else
            {
                string extension = System.IO.Path.GetExtension(absPath);
                string head = System.IO.Path.GetFileNameWithoutExtension(filenameWithoutComment);
                string separator = new string('-', newLevel);
                newFilename = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(absPath), 
                    string.Format("{0} {1} {2}{3}", head, separator, comment, extension)
                );
            }

            return newFilename;
        }

        private System.Text.RegularExpressions.MatchCollection match()
        {
            return System.Text.RegularExpressions.Regex.Matches(filename, REGEX);
        }

        public bool hasComment()
        {
            return comment != null;
        }
    }
}
