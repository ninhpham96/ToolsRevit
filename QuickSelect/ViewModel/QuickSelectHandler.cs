using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QuickSelect.ViewModel
{
    public class QuickSelectHandler : IExternalEventHandler
    {
        #region Field
        private static Document? _doc;
        private static UIApplication? _uiapp;
        private static Autodesk.Revit.ApplicationServices.Application? _app;
        private static UIDocument? _uidoc;
        #endregion

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
                    case RequestId.OK:
                        {
                            SelectFamily();
                            break;
                        }
                    case RequestId.ZoomIn:
                        {
                            ZoomInFamily();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to execute the external event.\n" + ex.Message, "Execute Event", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #region External event
        private void ZoomInFamily()
        {
            TaskDialog.Show("Ninh", "Test");
        }

        private void SelectFamily()
        {
            TaskDialog.Show("Ninh", "Test");
        }

        public string GetName()
        {
            return "My External Event";
        }
        #endregion


        public CommunicatorRequest Request { get; set; } = new CommunicatorRequest();

        public class CommunicatorRequest
        {
            private int _request = (int)RequestId.OK;

            public RequestId Take()
            {
                return (RequestId)Interlocked.Exchange(ref _request, (int)RequestId.OK);
            }

            public void Make(RequestId request)
            {
                Interlocked.Exchange(ref _request, (int)request);
            }
        }
        public enum RequestId
        {
            OK,
            Cancel,
            ZoomIn
        }
    }

}
