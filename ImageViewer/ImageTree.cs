using System;
using System.Collections.Generic;

namespace ImageViewer
{
    public class ImageTree
    {
        public string name;
        public ImageTree parent;
        public int commentLevel;
        public List<ImageTree> nodes;
        public List<ImageFile> files;

        private ImageTree(ImageFile f = null, ImageTree parent = null)
        {
            this.parent = parent;
            this.nodes = new List<ImageTree>();
            this.files = new List<ImageFile>();

            if (f != null)
            {
                this.name = f.comment;
                this.commentLevel = f.commentLevel;
                this.files.Add(f);
            }
            else
            {
                this.name = "";
                this.commentLevel = 0;
            }
        }

        public ImageTree(ImageList imageList) : this(null, null)
        {
            int currentLevel = 0;
            ImageTree currentNode = this;

            foreach (string s in imageList)
            {
                var f = new ImageFile(s);

                if (f.hasComment())
                {
                    while (currentNode.commentLevel >= f.commentLevel)
                        currentNode = currentNode.parent;

                    if (f.commentLevel == currentLevel)
                    {
                        currentNode = currentNode.newSiblingNode(f);
                    }
                    else
                    {
                        currentNode = currentNode.newChildNode(f);
                    }
                }
                else
                {
                    currentNode.addEntry(f);
                }
            }
        }

        public bool isRoot()
        {
            return parent == null;
        }

        public ImageTree newChildNode(ImageFile f)
        {
            ImageTree node = new ImageTree(f, this);

            nodes.Add(node);
            return node;
        }

        public ImageTree newSiblingNode(ImageFile f)
        {
            return this.parent.newChildNode(f);
        }

        public void addEntry(ImageFile e)
        {
            files.Add(e);
        }

        public ImageTree findTreeByAbsPath(string absPath)
        {
            foreach (var f in files)
            {
                if (f.absPath == absPath)
                    return this;
            }

            foreach (var n in nodes)
            {
                ImageTree result = n.findTreeByAbsPath(absPath);
                if (result != null)
                    return result;
            }

            return null;
        }

        public List<string> breadcrumbs(string rootName = "")
        {
            List<string> results = new List<string>();
            ImageTree c = this;

            while (c != null)
            {
                if (c.isRoot())
                    results.Insert(0, rootName);
                else
                    results.Insert(0, c.name);

                c = c.parent;
            }

            return results;
        }

        public void dump()
        {
            dump_(0, this);
        }

        private void dump_(int level, ImageTree node)
        {
            string indent = new string(' ', level * 2);

            Console.WriteLine(indent + '[' + node.name + ']');

            foreach (ImageFile entry in node.files)
            {
                Console.WriteLine(indent + "  - " + entry.filename);
            }

            foreach (ImageTree n in node.nodes)
            {
                dump_(level + 1, n);
            }
        }
    }
}
