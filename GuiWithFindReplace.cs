using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VietOCR
{
    public class GuiWithFindReplace : GuiWithPostprocess
    {
        public GuiWithFindReplace()
        {

        }

        protected override void buttonFind_Click(object sender, RoutedEventArgs e)
        {
            if (OwnedWindows.Count > 0)
            {
                foreach (Window form in this.OwnedWindows)
                {
                    FindDialog findDlg1 = form as FindDialog;
                    if (findDlg1 != null)
                    {
                        findDlg1.Show();
                        return;
                    }
                }
            }

            FindDialog findDialog = new FindDialog();
            findDialog.Owner = this;

            findDialog.Show();

        }
    }
}
