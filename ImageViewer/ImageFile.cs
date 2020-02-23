namespace ImageViewer
{
    public class ImageFile
    {
        const string REGEX = @"(?<filename>.*?)\s+(?<separator>-+)\s*(?<comment>.*)(?<extension>\..*?)";

        public readonly string absPath;
        public readonly string filename;
        public readonly string filenameWithoutComment;
        public readonly string comment;
        public readonly int commentLevel;

        public ImageFile(string path)
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
