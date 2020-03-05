using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

        public ImageRepository imageRepository = null;

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

        public ImageTree(ImageRepository imageRepojitory) : this(null, null)
        {
            this.imageRepository = imageRepojitory;
            setupTree();
        }

        protected void setupTree()
        {
            if (imageRepository.IsVirtualRepository)
                new VirtualTreeBuilder().BuildTree(imageRepository, this);
            else
                new DirectoryBuildTree().BuildTree(imageRepository, this);
        }

        public bool contains(ImageFile file)
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
            // ImageRepository経由以外から呼び出してはいけない。

            if (!isRoot())
                throw new NotImplementedException("reload should root node.");

            clear();
            setupTree();
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

        public bool isRoot()
        {
            return parent == null;
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

        public void fixLevel(bool recursive = true)
        {
            if (files.Count > 0)
                files[0].ChangeLevel(treeLevel);

            if (recursive)
                foreach (var t in nodes)
                    t.fixLevel(true);
        }

        public void clear()
        {
            nodes.Clear();
            files.Clear();
            name = "";
            commentLevel = 0;
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

        public void addFile(ImageFile e)
        {
            files.Add(e);
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

        internal bool Empty()
        {
            return files.Count == 0;
        }
    }

    interface ITreeBuilder
    {
        void BuildTree(ImageRepository repository, ImageTree root);
    }

    class VirtualTreeBuilder : ITreeBuilder
    {
        public VirtualTreeBuilder()
        { }

        public void BuildTree(ImageRepository repository, ImageTree root)
        {
            int currentLevel = 0;
            ImageTree currentNode = root;

            foreach (var f in repository)
            {
                if (f.HasComment())
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
                    currentNode.addFile(f);
                }
            }
        }
    }

    class DirectoryBuildTree : ITreeBuilder
    {
        private class HEntry
        {
            public Hashtable hash = new Hashtable();
            public List<ImageFile> files = new List<ImageFile>();
        }

        public void BuildTree(ImageRepository repository, ImageTree root)
        {
            var hashRoot = makeHash(repository);
            makeTree(root, hashRoot);
        }

        private HEntry makeHash(ImageRepository repository)
        {
            var rootEntry = new HEntry();

            foreach (var f in repository)
            {
                var c = rootEntry;

                foreach (var part in Path.GetDirectoryName(f.AbsPath).Split(Path.DirectorySeparatorChar))
                {
                    if (!c.hash.Contains(part))
                        c.hash[part] = new HEntry();
                    c = (HEntry)c.hash[part];
                }
                c.files.Add(f);
            }

            var relRoot = rootEntry;
            while (relRoot.files.Count == 0 && relRoot.hash.Count == 1)
            {
                foreach (var t in relRoot.hash.Keys)
                {
                    relRoot = (HEntry)relRoot.hash[t];
                    break;
                }
            }

            return relRoot;
        }

        private void makeTree(ImageTree tree, HEntry entry, int level = 0)
        {
            entry.files.Sort();
            foreach (var f in entry.files)
                tree.addFile(f);

            string[] keyList = new string[entry.hash.Count];
            entry.hash.Keys.CopyTo(keyList, 0);
            var t = new List<string>(keyList);
            t.Sort();

            foreach (var k in t)
            {
                var newEntry = (HEntry)entry.hash[k];
                var newTree = tree.newChildNode(null);

                newTree.name = (string)k;
                newTree.treeLevel = level + 1;

                makeTree(newTree, newEntry, level + 1);
            }
        }
    }
}
