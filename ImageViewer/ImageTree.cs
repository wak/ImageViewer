using System;
using System.Collections.Generic;

namespace ImageViewer
{
    public class ImageTree
    {
        public string name;
        public ImageTree parent;
        public int commentLevel;
        public int treeLevel;
        public List<ImageTree> nodes;
        public List<ImageFile> files;

        private ImageRepository imageList = null;

        private ImageTree(ImageFile f = null, ImageTree parent = null)
        {
            this.parent = parent;
            this.nodes = new List<ImageTree>();
            this.files = new List<ImageFile>();

            if (parent == null)
                treeLevel = 0;
            else
                treeLevel = parent.treeLevel + 1;

            clear();

            if (f != null)
            {
                this.name = f.Comment;
                this.commentLevel = f.CommentLevel;
                this.files.Add(f);
            }
        }

        public ImageTree(ImageRepository imageList) : this(null, null)
        {
            this.imageList = imageList;
            setupTree(imageList);
        }

        private void setupTree(ImageRepository imageList)
        {
            int currentLevel = 0;
            ImageTree currentNode = this;

            foreach (var f in imageList)
            {
                if (f.hasComment())
                {
                    while (currentNode.commentLevel >= f.CommentLevel)
                        currentNode = currentNode.parent;

                    if (f.CommentLevel == currentLevel)
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

        internal bool contains(ImageFile file)
        {
            if (file == null)
                return false;

            foreach (var f in files)
                if (f.AbsPath == file.AbsPath)
                    return true;

            return false;
        }

        // 破壊的
        public void upLevel(bool recursive = false)
        {
            if (files.Count > 0)
                files[0].ChangeLevel(treeLevel - 1);

            if (recursive)
                foreach (var t in nodes)
                    t.upLevel(true);
        }

        // 破壊的
        public void downLevel(bool recursive = false)
        {
            if (files.Count > 0)
                files[0].ChangeLevel(treeLevel + 1);

            if (recursive)
                foreach (var t in nodes)
                    t.downLevel(true);
        }

        public void reload()
        {
            if (!isRoot())
                throw new NotImplementedException("reload should root node.");

            imageList.reload();
            clear();
            setupTree(imageList);
        }

        private void clear()
        {
            nodes.Clear();
            files.Clear();
            name = "";
            commentLevel = 0;
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
                if (f.AbsPath == absPath)
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
                Console.WriteLine(indent + "  - " + entry.Filename);
            }

            foreach (ImageTree n in node.nodes)
            {
                dump_(level + 1, n);
            }
        }

        internal void fixLevel(bool recursive = true)
        {
            if (files.Count > 0)
                files[0].ChangeLevel(treeLevel);

            if (recursive)
                foreach (var t in nodes)
                    t.fixLevel(true);
        }
    }
}
