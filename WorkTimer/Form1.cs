using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WorkTimer
{
    public partial class Form1 : Form
    {

        public const int ScreenshotInterval = 5; //Measured in minutes

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateListView();
        }

        #region Core Functions
        private void UpdateListView()
        {
            //Clear the listview
            listPreviousSessions.Items.Clear();

            //Load any previous work sessions
            string[] dirs = Directory.GetDirectories(Environment.CurrentDirectory);

            foreach (string dir in dirs)
            {
                if (File.Exists(dir + "/info.txt"))
                {
                    string[] lines = File.ReadAllLines(dir + "/info.txt");
                    if (lines.Length < 2) return;

                    //Pull info from that file
                    DateTime start;
                    DateTime end;

                    if (!DateTime.TryParse(lines[0], out start) || !DateTime.TryParse(lines[1], out end)) { return; }

                    ListViewItem item = listPreviousSessions.Items.Insert(0, start.ToString());
                    item.Tag = (object)dir;
                    item.SubItems.Add(end.ToString());
                    item.SubItems.Add((end - start).ToString());
                }
            }
        }

        private void Start()
        {
            DirectoryInfo dir = TimeInfo.Start();

            if (dir != null)
            {
                timeUpdate.Enabled = true;

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                btnTrayStartStop.Text = "Stop";
            }
        }

        private void Stop()
        {
            if (TimeInfo.Folder == null || !TimeInfo.Folder.Exists) { return; }
            TimeInfo.Stop();
            timeUpdate.Enabled = false;

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnTrayStartStop.Text = "Start";

            labelTime.Text = TimeInfo.GetFormattedTime();

            try
            {
                StreamWriter sw = File.CreateText(TimeInfo.Folder + "/info.txt");
                sw.WriteLine(TimeInfo.StartTime.ToString());
                sw.WriteLine(DateTime.Now.ToString());
                sw.Flush();
                sw.Close();
            }
            catch (Exception e )
            {
                MessageBox.Show("Failed to write info file. " + e.Message, "Work Timer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        #region UI Events

        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
            UpdateListView();
        }

        private void timeUpdate_Tick(object sender, EventArgs e)
        {
            TimeInfo.Update();

            labelTime.Text = TimeInfo.GetFormattedTime();
        }

        #endregion

        private void listPreviousSessions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listPreviousSessions.GetItemAt(e.X, e.Y);

            if (item != null)
            {
                try
                {
                    System.Diagnostics.Process.Start((string)item.Tag);
                }
                catch (Exception)
                {
                    UpdateListView(); //The file didn't exist, meaning our list view was out of date.
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.Hide();
                WindowState = FormWindowState.Minimized;
                trayIcon.Visible = true;
            }
        }

        private void btnTrayClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTrayStartStop_Click(object sender, EventArgs e)
        {
            if (TimeInfo.Running) 
                Stop();
            else
                Start();
            
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }
    }
}
