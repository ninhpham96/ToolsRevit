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
using System.Windows.Markup;
using System.Data.SqlTypes;
using System.Windows.Data;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectViewModel : ObservableObject
    {
        #region properties and field

        [ObservableProperty]
        private UIApplication? uiApp = null;

        private QuickSelectHandler handler;
        private string oldkeyword = string.Empty;

        [ObservableProperty]
        private ICollection<ElementId>? selectElements = new List<ElementId>();

        [ObservableProperty]
        private bool isOpen = false;

        [ObservableProperty]
        private string? search = string.Empty;

        public static QuickSelectViewModel? Instance { get; set; }
        public ObservableCollection<QuickSelectData>? Items { get; set; }
        private List<Element>? ListElement = new List<Element>();

        public OptionType Scope { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value, nameof(IsSelected));
                if (value)
                {
                    Scope = OptionType.Selected;
                    InitItems();
                }
            }
        }

        private bool _isActiveView;

        public bool IsActiveView
        {
            get => _isActiveView;
            set
            {
                SetProperty(ref _isActiveView, value, nameof(IsActiveView));
                if (value)
                {
                    Scope = OptionType.ActiveView;
                    InitItems();
                }
            }
        }

        private bool _isAllProject;

        public bool IsAllProject
        {
            get => _isAllProject;
            set
            {
                SetProperty(ref _isAllProject, value, nameof(IsAllProject));
                if (value)
                {
                    Scope = OptionType.AllProject;
                    InitItems();
                }
            }
        }

        public void InitItems()
        {
            ListElement.Clear();
            SelectElements.Clear();
            if (Items != null)
            {
                Items.Clear();
            }
            else
            {
                Items = new ObservableCollection<QuickSelectData>();
            }
            if (uiApp.ActiveUIDocument == null)
            {
                return;
            }
            Document doc = uiApp.ActiveUIDocument?.Document;
            if (Scope == OptionType.ActiveView)
            {
                IEnumerable<IGrouping<string?, Element>> children = Data.Instance.GetAllElementsInView(doc)
                    .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                var items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, Search, null)));

                foreach (var item in items.OrderBy(x => x.Name))
                {
                    Items.Add(item);
                }
            }
            else if (Scope == OptionType.AllProject)
            {
                IEnumerable<IGrouping<string?, Element>> children = Data.Instance.GetAllElementsInProject(doc)
                    .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                var items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, Search, null)));

                foreach (var item in items.OrderBy(x => x.Name))
                {
                    Items.Add(item);
                }
            }
            else
            {
                ICollection<ElementId> selectedID = uiApp.ActiveUIDocument.Selection.GetElementIds();
                List<Element> selected = new();
                foreach (ElementId id in selectedID)
                {
                    selected.Add(doc.GetElement(id));
                }
                IEnumerable<IGrouping<string?, Element>> children = selected.Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                var items = new ObservableCollection<QuickSelectData>(children.Select(c => new QuickSelectData(c, Search, null)));

                foreach (var item in items.OrderBy(x => x.Name))
                {
                    Items.Add(item);
                }
            }

            if (Items?.Count > 0)
            {
                Items.First().SelectElements = SelectElements;
                foreach (QuickSelectData item in Items)
                {
                    foreach (Element ite in item.Current as IGrouping<string, Element>)
                    {
                        ListElement.Add(ite);
                    }
                }
                if (!string.IsNullOrWhiteSpace(Search))
                {
                    Searching();
                }
            }
        }

        #endregion properties and field

        #region constructor

        public QuickSelectViewModel(UIApplication uiapp, QuickSelectHandler handler)
        {
            Instance = this;
            UiApp = uiapp;
            this.handler = handler;
            IsActiveView = true;
        }

        #endregion constructor

        #region Command

        [RelayCommand]
        private void Click(QuickSelectData data)
        {
            try
            {
                if (data == null) return;
                SetCheckForChildren(data);
                SetCheckForParent(data);

                if (data.Type == EnumType.ListFamily)
                {
                    List<Element> elements = (List<Element>)data.Current;
                    if (data.IsChecked == true)
                        elements?.ForEach(elem => {
                            if (!SelectElements.Contains(elem.Id))
                            {
                                SelectElements?.Add(elem.Id);
                            }
                        });
                    else elements?.ForEach(elem => {
                        if (SelectElements.Contains(elem.Id))
                            SelectElements?.Remove(elem.Id);
                    });
                }
                else if (data.Type == EnumType.Category)
                {
                    List<Element> elements = ((IGrouping<string?, Element>)data.Current).ToList();
                    if (data.IsChecked == true)
                        elements?.ForEach(elem => {
                            if (!SelectElements.Contains(elem.Id))
                            {
                                SelectElements?.Add(elem.Id);
                            }
                        });
                    else elements?.ForEach(p => {
                        if (SelectElements.Contains(p.Id))
                            SelectElements?.Remove(p.Id);
                    });
                }
                else if (data.Type == EnumType.Parameter || data.Type == EnumType.Value)
                {
                    QuickSelectData elemData = data.Type == EnumType.Parameter ? data.Parent : data.Parent?.Parent;
                    List<Element> elements = ((IGrouping<string?, Element>)elemData?.Current).ToList();
                    elements?.ForEach(elem => {
                        if (SelectElements.Contains(elem.Id))
                            SelectElements?.Remove(elem.Id);
                    });

                    foreach (QuickSelectData d in elemData.Children.OrderByDescending(x => x.IsChecked))
                    {
                        if (d.IsChecked == false) continue;
                        if (d.IsChecked == true)
                        {
                            elements?.ForEach(elem => {
                                if (elem.GetParameters(d.Name)?.Count > 0 && !SelectElements.Contains(elem.Id))
                                {
                                    SelectElements?.Add(elem.Id);
                                }
                            });
                            if (SelectElements?.Count == elements?.Count)
                            {
                                break;
                            }
                            continue;
                        }

                        foreach (QuickSelectData item in d.Children.OrderByDescending(x => x.IsChecked))
                        {
                            if (item.IsChecked != true)
                            {
                                continue;
                            }
                            elements.ForEach(e => {
                                Parameter para = e.GetParameters(item.Parent.Name).FirstOrDefault();
                                if (para != null)
                                {
                                    string value = string.Empty;
                                    if (para.StorageType == StorageType.String)
                                    {
                                        value = para.AsString();
                                    }
                                    else
                                    {
                                        value = para.AsValueString();
                                    }
                                    if (string.IsNullOrWhiteSpace(value))
                                    {
                                        value = "<null>";
                                    }

                                    if (value == item.Name && !SelectElements.Contains(e.Id))
                                    {
                                        SelectElements.Add(e.Id);
                                    }
                                }
                            });
                        }

                        if (SelectElements?.Count == elements?.Count)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                RevitUtils.ShowWarning("データが見つかりません。 プロジェクトを確認し、データをリセットし、操作を再試行してください。");
            }
        }

        [RelayCommand]
        private void ClickOk()
        {
            try
            {
                handler.QuickSelectVM = Instance;
                AppCommand.Handler.Request.Make(QuickSelectHandler.RequestId.Select);
                AppCommand.ExEvent.Raise();
                RevitUtils.SetFocusToRevit();
            }
            catch (Exception)
            {
                RevitUtils.ShowWarning("データが見つかりません。 プロジェクトを確認し、データをリセットし、操作を再試行してください。");
            }
        }

        [RelayCommand]
        private void ClickZoomIn()
        {
            try
            {
                handler.QuickSelectVM = Instance;
                AppCommand.Handler.Request.Make(QuickSelectHandler.RequestId.Show);
                AppCommand.ExEvent.Raise();
                RevitUtils.SetFocusToRevit();
            }
            catch (Exception) { RevitUtils.ShowWarning("データが見つかりません。 プロジェクトを確認し、データをリセットし、操作を再試行してください。"); }
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

        [RelayCommand]
        public void Searching()
        {
            SelectElements.Clear();
            Items.Clear();
            if (uiApp.ActiveUIDocument == null)
            {
                return;
            }
            List<Element> temp = new List<Element>();
            var keySearch = string.Empty;
            if (string.IsNullOrWhiteSpace(Search))
            {
                if (ListElement?.Count > 0)
                {
                    temp.AddRange(ListElement);
                }
            }
            else
            {
                keySearch = Search.Trim();
                foreach (Element ele in ListElement)
                {
                    if (CheckElementOfList(keySearch, ele))
                        temp.Add(ele);
                }
            }

            IEnumerable<IGrouping<string?, Element>> children = temp
                        .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
            foreach (IGrouping<string?, Element> item in children)
            {
                Items.Add(new QuickSelectData(item, keySearch, null));
            }
        }

        [RelayCommand]
        private void Clear()
        {
            Search = string.Empty;
            InitItems();
        }

        [RelayCommand]
        private void TextChanged(string keyword)
        {
            if (keyword == string.Empty) Clear();
        }

        #endregion Command

        #region methods

        private bool CheckElementOfList(string keyword, Element ele)
        {
            try
            {
                if (CheckSubString(keyword, ele.Category.Name))
                    return true;

                ParameterSet paras = ele.Parameters;
                foreach (Parameter para in paras)
                {
                    if (para == null) continue;
                    if (para.Definition == null) continue;
                    if (CheckSubString(keyword, para.Definition.Name)) return true;

                    string value = string.Empty;
                    if (para.StorageType == StorageType.String)
                    {
                        value = para.AsString();
                    }
                    else
                    {
                        value = para.AsValueString();
                    }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        value = "<null>";
                    }

                    if (CheckSubString(keyword, value))
                        return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        private bool CheckSubString(string keyword, string target)
        {
            int isSubstring = target.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            return isSubstring != -1;
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
        }

        #endregion methods
    }
}