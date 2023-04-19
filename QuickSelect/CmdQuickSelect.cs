using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using QuickSelect.Utilities;
using QuickSelect.View;
using QuickSelect.ViewModel;
using System;
using System.Collections.Generic;

namespace QuickSelect
{
    [Transaction(TransactionMode.Manual)]
    public class CmdQuickSelect : IExternalCommand
    {
        private UIDocument? uidoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            return Execute(commandData.Application);
        }

        public Result Execute(UIApplication uiapp)
        {
            try
            {
                ICollection<ElementId> selectedID = uidoc.Selection.GetElementIds();

                TaskDialog taskDialog = new TaskDialog("適用範囲");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "カレントービュー");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "プロジェクト全体");
                if (selectedID?.Count > 0)
                {
                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "選択したオブジェクト");
                }
                var result = taskDialog.Show();
                if (result == TaskDialogResult.CommandLink1)
                {
                    new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler, OptionType.ActiveView));
                }
                else if (result == TaskDialogResult.CommandLink2)
                {
                    new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler, OptionType.AllProject));
                }
                else if (result == TaskDialogResult.CommandLink3)
                {
                    new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler, OptionType.Selected));
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                RevitUtils.ShowException(ex);
                return Result.Failed;
            }
        }
    }
}