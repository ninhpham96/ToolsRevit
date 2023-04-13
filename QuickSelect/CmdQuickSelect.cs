using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using QuickSelect.View;
using QuickSelect.ViewModel;
using System;
using System.Collections.Generic;

namespace QuickSelect
{
    [Transaction(TransactionMode.Manual)]
    public class CmdQuickSelect : IExternalCommand
    {
        UIDocument? uidoc;
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
                if (selectedID != null && selectedID.Count != 0)
                {
                    new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler, OptionType.Selected));
                }
                else
                {
                    TaskDialog taskDialog = new TaskDialog("Option");
                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Active View");
                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "All project");
                    var result = taskDialog.Show();
                    if (result == TaskDialogResult.CommandLink1)
                        new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler, OptionType.ActiveView));
                    if (result == TaskDialogResult.CommandLink2)
                        new QuickSelectView(new QuickSelectViewModel(uiapp, AppCommand.Handler, OptionType.AllProject));
                }
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
