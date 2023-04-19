using Autodesk.Windows;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace QuickSelect.Utilities
{
    public class RevitUtils
    {
        #region Triggering External Event Execute by Setting Focus

        //Thanks for solution:
        //https://thebuildingcoder.typepad.com/blog/2013/12/triggering-immediate-external-event-execute.html
        //https://github.com/jeremytammik/RoomEditorApp/tree/master/RoomEditorApp
        //https://thebuildingcoder.typepad.com/blog/2016/03/implementing-the-trackchangescloud-external-event.html#5
        /// <summary>
        /// The GetForegroundWindow function returns a
        /// handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Move the window associated with the passed
        /// handle to the front.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(
          IntPtr hWnd);

        public static void SetFocusToRevit()
        {
            IntPtr hRevit = ComponentManager.ApplicationWindow;
            IntPtr hBefore = GetForegroundWindow();

            if (hBefore != hRevit)
            {
                SetForegroundWindow(hRevit);
                SetForegroundWindow(hBefore);
            }
        }

        #endregion Triggering External Event Execute by Setting Focus

        /// <summary>
        /// prompt user with information
        /// </summary>
        public static void ShowInfor(string content, string title = "情報")
        {
            System.Windows.MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// prompt user with warning
        /// </summary>
        public static void ShowWarning(string content, string title = "警告")
        {
            System.Windows.MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Show exception information
        /// </summary>
        public static void ShowError(string content, string title = "エラー")
        {
            System.Windows.MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// prompt a yes/no question to ask for user decision
        /// </summary>
        public static MessageBoxResult ShowQuestion(string content, string title = "質問")
        {
            return System.Windows.MessageBox.Show(content, title, MessageBoxButton.YesNo, MessageBoxImage.Question); ;
        }

        /// <summary>
        /// prompt user with an exception detail
        /// </summary>
        public static void ShowException(Exception ex, string title = "エラー")
        {
            string content = ex.Message /*+ "\n" + ex.StackTrace.ToString()*/;
            System.Windows.MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}