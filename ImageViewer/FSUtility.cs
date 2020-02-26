using System;
using System.Windows.Forms;

namespace ImageViewer
{
    class FSUtility
    {
        static public bool rename(string from, string to)
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
    }
}
