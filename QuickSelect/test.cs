using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using QuickSelect.View;
using QuickSelect.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

[Transaction(TransactionMode.Manual)]
public class test : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uidoc = commandData.Application.ActiveUIDocument;
        Document doc = uidoc.Document;

        var pick = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
        var ele = doc.GetElement(pick);
        var paras = ele.Parameters;
        var ps = ele.GetParameters;
        StringBuilder sb = new StringBuilder();

        foreach ( Parameter par in paras )
        {
            string x = ele.get_Parameter(par.Definition).AsValueString();
            sb.Append(par.Definition.Name+"."+x+"."+"\n");
        }

        MessageBox.Show(sb.ToString());



        return Result.Succeeded;
    }
}
