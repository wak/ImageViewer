using System;
using System.Collections.Generic;

public class ImageList
{
    private readonly List<string> IMAGE_EXTENTIONS = new List<string>(new string[] { ".bmp", ".jpg", ".jpeg", ".png" });
    private List<string> imageList = new List<string>();

    public ImageList()
    {
        // nothing to do (empty class)
    }

    public ImageList(string folderPath)
    {
        this.findImages(folderPath);
    }

    private void findImages(string folderPath)
    {
        string[] allPathes = System.IO.Directory.GetFiles(folderPath);

        foreach (string path in allPathes)
        {
            string extension = System.IO.Path.GetExtension(path);

            if (IMAGE_EXTENTIONS.Contains(extension.ToLower()))
            {
                Console.WriteLine(path);
                imageList.Add(System.IO.Path.GetFullPath(path));
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

    public int Count
    {
        get { return imageList.Count; }
    }

    public string this[int index]
    {
        get { return imageList[index]; }
    }

}
