using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using QuickSelect.Utilities;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectHandler : IExternalEventHandler
    {
        #region Field

        private static Document? _doc;
        private static UIApplication? _uiapp;
        private static Autodesk.Revit.ApplicationServices.Application? _app;
        private static UIDocument? _uidoc;
        private QuickSelectViewModel? quickSelectVM = null;

        public QuickSelectViewModel? QuickSelectVM
        { get { return quickSelectVM; } set { quickSelectVM = value; } }

        public CommunicatorRequest Request { get; set; } = new CommunicatorRequest();

        #endregion Field

        public void Execute(UIApplication uiapp)
        {
            _uiapp = uiapp;
            _uidoc = uiapp.ActiveUIDocument;
            _app = uiapp.Application;
            _doc = uiapp.ActiveUIDocument.Document;
            try
            {
                switch (Request.Take())
                {
                    case RequestId.Cancel:
                    {
                        return;
                    }
                    case RequestId.Select:
                    {
                        SelectElement();
                        break;
                    }
                    case RequestId.Show:
                    {
                        ShowElement();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                RevitUtils.ShowException(ex);
            }
        }

        #region External event

        private void ShowElement()
        {
            if (quickSelectVM.SelectElements?.Count > 0)
            {
                _uidoc.Selection.SetElementIds(quickSelectVM.SelectElements);
                _uidoc.ShowElements(quickSelectVM.SelectElements);
            }
        }

        private void SelectElement()
        {
            if (quickSelectVM.SelectElements?.Count > 0)
            {
                _uidoc.Selection.SetElementIds(quickSelectVM.SelectElements);
            }
        }

        public string GetName()
        {
            return "MF Quick Select";
        }

        #endregion External event

        public class CommunicatorRequest
        {
            private int _request = (int)RequestId.Select;

            public RequestId Take()
            {
                return (RequestId)Interlocked.Exchange(ref _request, (int)RequestId.Select);
            }

            public void Make(RequestId request)
            {
                Interlocked.Exchange(ref _request, (int)request);
            }
        }

        public enum RequestId
        {
            Select,
            Cancel,
            Show
        }
    }
}