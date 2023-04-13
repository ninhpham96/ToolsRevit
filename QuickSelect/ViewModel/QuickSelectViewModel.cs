﻿using Autodesk.Revit.UI;
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
using System.Windows.Markup;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectViewModel : ObservableObject
    {
        #region properties and field
        [ObservableProperty]
        private UIApplication? uiApp = null;
        private Document? doc = null;
        private QuickSelectHandler handler;
        private OptionType optionType;
        [ObservableProperty]
        private ICollection<ElementId>? selectElements = new List<ElementId>();
        [ObservableProperty]
        private bool isOpen = false;
        public static QuickSelectViewModel? Instance { get; set; }
        public ObservableCollection<QuickSelectData>? Items { get; set; }
        #endregion
        #region constructor
        public QuickSelectViewModel(UIApplication uiapp, QuickSelectHandler handler, OptionType option)
        {
            Instance = this;
            UiApp = uiapp;
            doc = uiapp.ActiveUIDocument.Document;
            this.handler = handler;
            optionType = option;
            //ICollection<Element> children = Data.Instance.GetAllElementsInView(doc);
            //List<QuickSelectData>? items = new List<QuickSelectData>() { new QuickSelectData(children, null) };
            //Items = new ObservableCollection<QuickSelectData>(items);

            if (optionType == OptionType.ActiveView)
            {
                IEnumerable<IGrouping<string?, Element>> children = Data.Instance.GetAllElementsInView(doc)
                    .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                Items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, null)));
                Items.First().SelectElements = SelectElements;
            }
            else if (optionType == OptionType.AllProject)
            {
                IEnumerable<IGrouping<string?, Element>> children = Data.Instance.GetAllElementsInProject(doc)
                    .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                Items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, null)));
                Items.First().SelectElements = SelectElements;
            }
            else if (optionType == OptionType.Selected)
            {
                ICollection<ElementId> selectedID = uiapp.ActiveUIDocument.Selection.GetElementIds();
                List<Element> selected = new();
                foreach (ElementId id in selectedID)
                {
                    selected.Add(doc.GetElement(id));
                }
                IEnumerable<IGrouping<string?, Element>> children = selected.Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                Items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, null)));
                Items.First().SelectElements = SelectElements;
            }

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
                    SetCheckForChildren(data);
                    SetCheckForParent(data);
                    foreach (QuickSelectData item in data.Parent.Children)
                    {
                        if (item.Name == data.Name) continue;
                        if (item.IsChecked == false) continue;
                        else
                        {
                            elements?.ForEach(p =>
                            {
                                if (!SelectElements.Contains(p.Id))
                                {
                                    SelectElements?.Add(p.Id);
                                }
                            });
                        }
                    }
                    return;
                }
                else if (data.Type == EnumType.Value)
                {
                    List<Element> elements = (data.Parent?.Parent?.Current) as List<Element>;
                    elements.ForEach(e =>
                    {
                        if (e.LookupParameter(data.Parent.Name).AsValueString() == data.Name)
                        {
                            if (data.IsChecked == true)
                            {
                                if (!SelectElements.Contains(e.Id))
                                    SelectElements.Add(e.Id);
                            }
                            else
                            {
                                if (SelectElements.Contains(e.Id))
                                    SelectElements.Remove(e.Id);
                            }
                        }
                    });
                    SetCheckForChildren(data);
                    SetCheckForParent(data);
                    foreach (QuickSelectData d in data.Parent.Parent.Children)
                    {
                        if (d.Name == data.Parent.Name) continue;
                        if (d.IsChecked == false) continue;
                        if(d.IsChecked == true)
                        {
                            foreach (var item in (d.Parent.Current) as List<Element>)
                            {
                                if (!SelectElements.Contains(item.Id))
                                    SelectElements.Add(item.Id);
                            }
                        }
                        if (d.IsChecked == null)
                        {
                            foreach (QuickSelectData item in d.Children)
                            {
                                elements.ForEach(e =>
                                {
                                    if (e.LookupParameter(d.Name).AsValueString() == item.Name)
                                    {
                                        if (item.IsChecked == true)
                                        {
                                            if (!SelectElements.Contains(e.Id))
                                                SelectElements.Add(e.Id);
                                        }
                                    }
                                });
                            }
                        }
                    }
                    return;
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
            try
            {
                handler.QuickSelectVM = Instance;
                AppCommand.Handler.Request.Make(QuickSelectHandler.RequestId.OK);
                AppCommand.ExEvent.Raise();
                RevitUtils.SetFocusToRevit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        [RelayCommand]
        private void ClickZoomIn()
        {
            try
            {
                handler.QuickSelectVM = Instance;
                AppCommand.Handler.Request.Make(QuickSelectHandler.RequestId.ZoomIn);
                AppCommand.ExEvent.Raise();
                RevitUtils.SetFocusToRevit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        [RelayCommand]
        private void ClickCancel(object ob)
        {
            var window = ob as Window;
            if (window != null) window.Close();
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
            if (data.Parent == null) return;
            HashSet<bool> check = new HashSet<bool>();
            foreach (QuickSelectData child in data.Parent.Children)
            {
                if (child.IsChecked == null)
                {
                    data.Parent.IsChecked = null;
                    SetCheckForParent(data.Parent);
                    return;
                }
                else
                {
                    check.Add((bool)child.IsChecked);
                    if (check.Count > 1) break;
                }
            }
            if (check.Count > 1 || check.Count == 0)
                data.Parent.IsChecked = null;
            else if (check.Count == 1)
                data.Parent.IsChecked = check.FirstOrDefault();
            SetCheckForParent(data.Parent);
            #region
            //if (data.Parent != null)
            //{
            //    foreach (QuickSelectData child in data.Parent.Children)
            //    {
            //        if (child.IsChecked == null)
            //        {
            //            data.Parent.IsChecked = null;
            //            //continue;
            //        }
            //        else
            //        {
            //            check.Add((bool)child.IsChecked);
            //            if (check.Count > 1)
            //            {
            //                data.Parent.IsChecked = null;
            //                //SetCheckForParent(data.Parent);
            //                //return;
            //            }
            //        }
            //    }
            //    //data.Parent.IsChecked = check.First();
            //}
            //SetCheckForParent(data.Parent);
            #endregion
        }
        #endregion
    }
}
