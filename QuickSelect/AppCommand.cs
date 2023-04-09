#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Reflection;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using QuickSelect.ViewModel;
using QuickSelect.Utilities;
using System.Configuration.Assemblies;
#endregion

namespace QuickSelect
{
    public class AppCommand : IExternalApplication
    {
        #region properties and fields
        public static QuickSelectHandler? Handler { get; set; } = null;
        public static ExternalEvent? ExEvent { get; set; } = null;
        private readonly string _tabName = "Tools";
        private static UIControlledApplication? _uiApp;
        internal static string assemblyPath = typeof(AppCommand).Assembly.Location;
        internal static AppCommand? GetInstance { get; private set; } = null;
        #endregion
        #region methods
        public Result OnStartup(UIControlledApplication a)
        {
            GetInstance = this;
            _uiApp = a;
            BuildUI(a);
            Handler = new QuickSelectHandler();
            ExEvent = ExternalEvent.Create(Handler);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
        private void BuildUI(UIControlledApplication uiApp)
        {
            RibbonPanel? panel = RibbonUtils.CreatePanel(uiApp, _tabName, "Utilities");
            var data = new PushButtonData("btnColor", "Color\nElement", assemblyPath, typeof(CmdQuickSelect).FullName);
            data.LargeImage = RibbonUtils.ConvertFromBitmap(Properties.Resources.subscript_32);
            data.Image = RibbonUtils.ConvertFromBitmap(Properties.Resources.subscript_16);
            data.ToolTip = "Read level name and update for each elements in parameter VCF_Etage";
            if (panel != null)
            {
                var btnColor = panel.AddItem(data) as PushButton;
            }

        }
        #endregion
    }
}
