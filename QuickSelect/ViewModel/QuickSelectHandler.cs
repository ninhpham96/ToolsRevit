﻿using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

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
        public QuickSelectViewModel? QuickSelectVM { get { return quickSelectVM; } set { quickSelectVM = value; } }
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
            using (Transaction tran = new Transaction(_doc, "1111"))
            {
                tran.Start();
                if(_doc!=null)
                    _doc.Delete(new ElementId(58398931));
                tran.Commit();
            }
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