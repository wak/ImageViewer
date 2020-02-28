using System;
using System.IO;
using System.Windows.Forms;

namespace ImageViewer
{
    static class FSUtility
    {
        static public bool Rename(string from, string to)
        {
            if (from == to)
                return true;

            try
            {
                System.IO.File.Move(from, to);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("名前の変更に失敗しました。\n\n" + e.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        static public void Touch(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
        }
    }
}
