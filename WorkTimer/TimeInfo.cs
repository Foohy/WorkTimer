using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace WorkTimer
{
    class TimeInfo
    {
        public static bool Running { get; private set; }
        public static DateTime StartTime { get; private set; }
        public static DirectoryInfo Folder { get; private set; }
        public static TimeSpan TotalTime { get; private set; }

        private static DateTime NextScreenshot;

        /// <summary>
        /// Start the timer and create a new work session
        /// </summary>
        /// <returns>DirectoryInfo: directory for the newly created work session</returns>
        public static DirectoryInfo Start()
        {
            if (Running) { return null; }

            try
            {
                //Create the directory to hold the images/information
                DirectoryInfo dir = Directory.CreateDirectory(Environment.CurrentDirectory + "/" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss"));
                Folder = dir;
            }
            catch (Exception e)
            {
                MessageBox.Show( "Failed to create work directory. " + e.Message, "Work Timer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            Running = true;
            StartTime = DateTime.Now;
            TotalTime = new TimeSpan();
            NextScreenshot = new DateTime();

            return Folder;

        }

        /// <summary>
        /// Update GUI elements and test if a screenshot should be taken
        /// </summary>
        public static void Update()
        {
            if (!Running) { return; }

            TotalTime = DateTime.Now - StartTime;

            if (DateTime.Now > NextScreenshot)
            {
                //take a screenshot
                NextScreenshot = DateTime.Now + TimeSpan.FromMinutes(MainForm.ScreenshotInterval);
                TakeScreenshot();
            }
        }
        
        /// <summary>
        /// Stop the current work session
        /// </summary>
        /// <returns>The directory of the current work session</returns>
        public static DirectoryInfo Stop()
        {
            if (!Running) { return null; }

            Running = false;
            TotalTime = DateTime.Now - StartTime;

            return Folder;
        }

        /// <summary>
        /// Get the total passed time in a nice pretty string.
        /// </summary>
        /// <returns>Time string</returns>
        public static string GetFormattedTime()
        {
            return string.Format("{0:HH:mm:ss}", new DateTime( TotalTime.Ticks) );
        }

        private static void TakeScreenshot()
        {
            Bitmap screenshot = new Bitmap(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics screenGraph = Graphics.FromImage(screenshot);
            screenGraph.CopyFromScreen(SystemInformation.VirtualScreen.X, SystemInformation.VirtualScreen.Y, 0, 0, SystemInformation.VirtualScreen.Size, CopyPixelOperation.SourceCopy);

            try
            {
                screenshot.Save(Folder + "/" + DateTime.Now.ToString("d_HH_mm_ss") + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception e )
            {
                MessageBox.Show("Failed to save screenshot. " + e.Message);
            }
        }
    }
}
