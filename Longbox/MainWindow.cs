using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Longbox
{
    public partial class MainWindow : Form
    {
        private bool IsFullScreen { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            pageView.Window = this;
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            pageView.OpenComic(openFileDialog.FileName);
        }

        public void setPageLabel(string txt)
        {
            pageNumberLabel.Text = txt;
        }

        private void fullScreenButton_Click(object sender, EventArgs e)
        {
            SetFullScreen(!IsFullScreen);
        }

        private void SetFullScreen(bool fs)
        {
            if (fs)
            {
                TopMost = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                menuBar.Visible = false;
            }
            else
            {
                TopMost = false;
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                menuBar.Visible = true;
            }

            IsFullScreen = fs;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                SetFullScreen(false);
        }
    }
}
