using QuickSelect.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuickSelect.View
{
    /// <summary>
    /// Interaction logic for QuickSelectView.xaml
    /// </summary>
    public partial class QuickSelectView : Window
    {
        QuickSelectViewModel viewModel;
        public static QuickSelectView? Instance { get; private set; } = null;
        public QuickSelectView(QuickSelectViewModel vm)
        {
            InitializeComponent();
            // Set ower revit window for Wpf form
            if (!vm.IsOpen) //Enforce single window
            {
                InitializeComponent();
                Instance = this;
                var uiapp = vm.UiApp;
                viewModel = vm;
                IntPtr revitWindow;

                if (uiapp != null)
                {
                    revitWindow = uiapp.MainWindowHandle;
                    //Get window of Revit form Revit handle
                    HwndSource hwndSource = HwndSource.FromHwnd(revitWindow);
                    var windowRevitOpen = hwndSource.RootVisual as Window;
                    this.Owner = windowRevitOpen; //Set onwer for WPF window
                    this.DataContext = vm;
                    this.Show();
                }               

            }
            if (Instance?.WindowState == WindowState.Minimized)
                Instance.WindowState = WindowState.Normal;
        }
    }
}
