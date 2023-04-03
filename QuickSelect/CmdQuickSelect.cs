using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using QuickSelect.View;
using QuickSelect.ViewModel;
using System;

namespace QuickSelect
{
    [Transaction(TransactionMode.Manual)]
    public class CmdQuickSelect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }

        //Call this methos with the external command
        public Result Execute(UIApplication uiapp)
        {
            try
            {
                var view = new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler));
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("tb",ex.Message);
                return Result.Failed;
            }

        }
    }
}
