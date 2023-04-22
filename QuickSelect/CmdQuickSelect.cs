using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using QuickSelect.Utilities;
using QuickSelect.View;
using QuickSelect.ViewModel;
using System;

namespace QuickSelect
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CmdQuickSelect : IExternalCommand
    {
        private UIDocument? uidoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (AppCommand.IsShow)
            {
                return Result.Cancelled;
            }

            AppCommand.IsShow = true;
            uidoc = commandData.Application.ActiveUIDocument;
            return Execute(commandData.Application);
        }

        public Result Execute(UIApplication uiapp)
        {
            try
            {
                new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler));
                return Result.Succeeded;
            }
            catch (Exception)
            {
                RevitUtils.ShowWarning("データが見つかりません。 プロジェクトを確認し、データをリセットし、操作を再試行してください。");
                return Result.Failed;
            }
        }
    }
}