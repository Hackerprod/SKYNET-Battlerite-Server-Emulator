using System;
using System.Windows.Forms;

namespace SKYNET
{
    internal class modCommon
    {
        internal static void Show(object v)
        {
            MessageBox.Show(v.ToString());
        }
    }
}