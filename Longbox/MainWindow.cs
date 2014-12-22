using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Longbox
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            pageView.OpenComic(openFileDialog.FileName);
        }
    }
}
