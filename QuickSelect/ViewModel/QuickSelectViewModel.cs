using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

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
        #endregion
    }
}
