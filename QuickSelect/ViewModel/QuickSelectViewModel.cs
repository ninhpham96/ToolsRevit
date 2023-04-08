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
using System.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectViewModel : ObservableObject
    {
        [ObservableProperty]
        private UIApplication? uiApp = null;
        private Document? doc = null;
        private QuickSelectHandler handler;
        [ObservableProperty]
        private bool isOpen = false;

        public static QuickSelectViewModel? Instance { get; set; }
        public ObservableCollection<QuickSelectData>? Items { get; set; }
        public QuickSelectViewModel(UIApplication uiapp, QuickSelectHandler handler)
        {
            Instance = this;
            UiApp = uiapp;
            doc = uiapp.ActiveUIDocument.Document;
            this.handler = handler;
            IEnumerable<IGrouping<string?,Element>> children = Data.Instance.GetAllElementsInView(doc)
                .Where(e=>e.Category!=null).GroupBy(e=>e.Category!=null?e.Category.Name:null);
            Items = new ObservableCollection<QuickSelectData>(children.Select(ele => new QuickSelectData(ele)));
        }

        #region Command
        [RelayCommand]
        private void ClickOk()
        {
            MessageBox.Show("Test Ok?");
        }
        [RelayCommand]
        private void ClickZoomIn()
        {
            MessageBox.Show("Test ZoomIn?");
        }
        [RelayCommand]
        private void ClickCancel()
        {
            MessageBox.Show("Test Cancel?");
        }
        [RelayCommand]
        private void WindowLoaded()
        {
            IsOpen = true;
        }
        [RelayCommand]
        private void WindowClosed()
        {
            IsOpen = false;
        }
        #endregion
        #region methods
        #endregion
    }
}
