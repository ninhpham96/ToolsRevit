using Autodesk.Revit.UI;
using QuickSelect.Utilities;
using QuickSelect.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickSelect
{
    public class AppCommand : IExternalApplication
    {
        #region properties and fields

        public static bool IsShow = false;
        public static QuickSelectHandler? Handler { get; set; } = null;
        public static ExternalEvent? ExEvent { get; set; } = null;
        private static UIControlledApplication? _uiApp;
        internal static string assemblyPath = typeof(AppCommand).Assembly.Location;
        internal static AppCommand? GetInstance { get; private set; } = null;

        #endregion properties and fields

        #region methods

        public Result OnStartup(UIControlledApplication app)
        {
            GetInstance = this;
            _uiApp = app;
            CreateRibbonPanel(app);
            Handler = new QuickSelectHandler();
            ExEvent = ExternalEvent.Create(Handler);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }

        private void CreateRibbonPanel(UIControlledApplication application)
        {
            string tabName = "MF Tools";
            string panelName = "クイック選択";
            string buttonName = "クイック選択";

            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception) { }
            List<RibbonPanel> allPanel = application.GetRibbonPanels(tabName);
            RibbonPanel panel = allPanel.FirstOrDefault(x => x.Name.Equals(panelName));
            if (panel == null)
                panel = application.CreateRibbonPanel(tabName, panelName);

            PushButtonData buttonData = new PushButtonData(
                name: buttonName,
                text: buttonName,
                assemblyName: assemblyPath,
                className: typeof(CmdQuickSelect).FullName);
            PushButton pushButton = panel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "パラメータで要素を選択";
            pushButton.LargeImage = RibbonUtils.ConvertFromBitmap(Properties.Resources.QuickSelect);
        }

        #endregion methods
    }
}