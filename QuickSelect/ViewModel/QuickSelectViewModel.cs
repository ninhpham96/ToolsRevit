using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using QuickSelect.Utilities;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectViewModel : ObservableObject
    {
        [ObservableProperty]
        private UIApplication? uiApp = null;
        private Document? _doc = null;
        private QuickSelectHandler _handler;
        [ObservableProperty]
        private bool isOpen = false;

        public static QuickSelectViewModel? Instance { get; set; }


        public QuickSelectViewModel(UIApplication uiapp, QuickSelectHandler handler)
        {
            Instance = this;
            UiApp = uiapp;
            _doc = uiapp.ActiveUIDocument.Document;
            _handler = handler;
        }

        #region Command
        [RelayCommand]
        private void WindowLoaded()
        {
            TaskDialog.Show("n", "1");
            IsOpen = true;
        }
        [RelayCommand]
        private void WindowClosed()
        {
            IsOpen = false;
        }
        [RelayCommand]
        private void Clickme()
        {
            TaskDialog.Show("n", "1");
            _handler.QuickSelectVM = this;
            AppCommand.Handler.Request.Make(QuickSelectHandler.RequestId.OK);
            AppCommand.ExEvent.Raise();
            RevitUtils.SetFocusToRevit();
        }
        [RelayCommand]
        private void TreeViewItemExpanded(RoutedEventArgs e)
        {
            TreeViewItem? item = e.Source as TreeViewItem;
            if (item == null)
                return;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);
                try
                {
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                        item.Items.Add(CreateTreeItem(subDir));
                }
                catch { }
            }

        }
        #endregion
        #region methods
        public TreeViewItem CreateTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = o.ToString();
            item.Tag = o;
            item.Items.Add("Loading...");
            return item;
        }
        #endregion
    }

}
