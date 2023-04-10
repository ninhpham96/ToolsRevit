using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using QuickSelect.Utilities;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;
using Autodesk.Revit.UI.Selection;
using System.Xml.Linq;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectViewModel : ObservableObject
    {
        #region properties and field
        [ObservableProperty]
        private UIApplication? uiApp = null;
        private Document? doc = null;
        private QuickSelectHandler handler;
        private ICollection<ElementId>? SelectElements = new List<ElementId>();
        [ObservableProperty]
        private bool isOpen = false;
        public static QuickSelectViewModel? Instance { get; set; }
        public ObservableCollection<QuickSelectData>? Items { get; set; }
        #endregion
        #region constructor
        public QuickSelectViewModel(UIApplication uiapp, QuickSelectHandler handler)
        {
            Instance = this;
            UiApp = uiapp;
            doc = uiapp.ActiveUIDocument.Document;
            this.handler = handler;
            //ICollection<Element> children = Data.Instance.GetAllElementsInView(doc);
            //List<QuickSelectData>? items = new List<QuickSelectData>() { new QuickSelectData(children, null) };
            //Items = new ObservableCollection<QuickSelectData>(children);

            IEnumerable<IGrouping<string?, Element>> children = Data.Instance.GetAllElementsInView(doc)
                .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
            Items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, null)));
            Items.First().SelectElements = SelectElements;

        }
        #endregion
        #region Command
        [RelayCommand]
        private void Click(QuickSelectData data)
        {
            try
            {
                if (data == null) return;
                if (data.Type == EnumType.ListFamily)
                {
                    List<Element> elements = (List<Element>)data.Current;
                    if (data.IsChecked == true)
                        elements?.ForEach(p =>
                        {
                            if (!SelectElements.Contains(p.Id))
                            {
                                SelectElements?.Add(p.Id);
                            }
                        });
                    else elements?.ForEach(p =>
                    {
                        if (SelectElements.Contains(p.Id))
                            SelectElements?.Remove(p.Id);
                    });
                }
                else if (data.Type == EnumType.Category)
                {
                    List<Element> elements = ((IGrouping<string?, Element>)data.Current).ToList();
                    if (data.IsChecked == true)
                        elements?.ForEach(p =>
                        {
                            if (!SelectElements.Contains(p.Id))
                            {
                                SelectElements?.Add(p.Id);
                            }
                        });
                    else elements?.ForEach(p =>
                    {
                        if (SelectElements.Contains(p.Id))
                            SelectElements?.Remove(p.Id);
                    });
                }
                else if (data.Type == EnumType.Element)
                {
                    List<Element> elements = (List<Element>)data.Current;
                    if (data.IsChecked == true)
                        elements?.ForEach(p =>
                        {
                            if (!SelectElements.Contains(p.Id))
                            {
                                SelectElements?.Add(p.Id);
                            }
                        });
                    else elements?.ForEach(p =>
                    {
                        if (SelectElements.Contains(p.Id))
                            SelectElements?.Remove(p.Id);
                    });
                }
                else if (data.Type == EnumType.Parameter)
                {
                    List<Element> elements = (data.Parent.Current) as List<Element>;
                    if (data.IsChecked == true)
                    {
                        if (!SelectElements.Contains(elements.First().Id))
                        {
                            elements.ForEach(p=>SelectElements.Add(p.Id));
                        }
                    }
                    else
                    {
                        if (SelectElements.Contains(elements.First().Id))
                            elements.ForEach(p => SelectElements.Remove(p.Id));
                    }
                }
                SetCheckForChildren(data);
                SetCheckForParent(data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        [RelayCommand]
        private void ClickOk()
        {
            SelectedElements();
        }
        [RelayCommand]
        private void ClickZoomIn()
        {
            MessageBox.Show("Test ZoomIn?");
        }
        [RelayCommand]
        private void ClickCancel()
        {
            MessageBox.Show("Test Cancel?");
        }
        [RelayCommand]
        private void WindowLoaded()
        {
            IsOpen = true;
        }
        [RelayCommand]
        private void WindowClosed()
        {
            IsOpen = false;
        }
        #endregion
        #region methods
        private void SelectedElements()
        {
            UiApp.ActiveUIDocument.Selection.SetElementIds(SelectElements);
        }
        private void SetCheckForChildren(QuickSelectData data)
        {
            if (data.Children != null && data.children.FirstOrDefault() != null)
            {
                foreach (var child in data.Children)
                {
                    child.IsChecked = data.IsChecked;
                    SetCheckForChildren(child);
                }
            }
        }
        private void SetCheckForParent(QuickSelectData data)
        {
            if (data == null) return;
            HashSet<bool> check = new HashSet<bool>();
            if (data.Parent != null)
            {
                foreach (QuickSelectData child in data.Parent.Children)
                {
                    if (child.IsChecked == null)
                    {
                        data.Parent.IsChecked = null;
                        return;
                    }
                    else
                    {
                        check.Add((bool)child.IsChecked);
                        if (check.Count > 1)
                        {
                            data.Parent.IsChecked = null;
                            SetCheckForParent(data.Parent);
                            return;
                        }
                    }
                }
                data.Parent.IsChecked = check.First();
            }
            SetCheckForParent(data.Parent);
        }
        #endregion
    }
}
