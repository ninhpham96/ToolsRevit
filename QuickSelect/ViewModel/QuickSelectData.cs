using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Linq;

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectData : ObservableObject
    {
        #region properties and field
        [ObservableProperty]
        private EnumType? type;
        public string? Name { get; set; }
        [ObservableProperty]
        private object? current = null;
        [ObservableProperty]
        private QuickSelectData? parent;
        private bool isExpanded;
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
                //if (Children?.Count == 1 && Children.First() == null) return false;
                //else return true;
            }
            set
            {
                isExpanded = value;
                OnPropertyChanged();
                if (value == true)
                    Expanded();
                //else
                //    ClearChildren();
            }
        }
        [ObservableProperty]
        private bool? isChecked = false;
        [ObservableProperty]
        private ICollection<ElementId>? selectElements;
        [ObservableProperty]
        public ObservableCollection<QuickSelectData>? children;
        private string keyword = string.Empty;
        #endregion
        #region Constructor
        public QuickSelectData(ICollection<Element> elements, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.ListFamily;
            Current = elements;
            Parent = parent;
            Name = "Family";
            ClearChildren();
        }
        public QuickSelectData(IGrouping<string?, Element> InumElements, string keyword, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Category;
            Current = InumElements;
            Parent = parent;
            Name = InumElements.Key;
            this.keyword = keyword;
            ClearChildren();
        }
        public QuickSelectData(List<Element> elements, string keyword, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Element;
            Current = elements;
            Parent = parent;
            Name = elements.FirstOrDefault().Name;
            this.keyword = keyword;
            ClearChildren();
        }
        public QuickSelectData(Parameter parameter, string keyword, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Parameter;
            Current = parameter;
            Parent = parent;
            Name = parameter.Definition.Name;
            this.keyword = keyword;
            ClearChildren();
        }
        public QuickSelectData(string valuestring, string keyword, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Value;
            Current = valuestring;
            Parent = parent;
            Name = valuestring;
            ClearChildren();
        }
        #endregion
        #region methods
        private bool CheckSubString(string keyword, string target)
        {
            int isSubstring = target.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (isSubstring == -1) return false;
            else return true;
        }
        private void ClearChildren()
        {
            Children = new ObservableCollection<QuickSelectData>();
            if (Type == EnumType.Value) return;
            Children?.Add(null);
        }
        #endregion
        #region Command
        [RelayCommand]
        private void Expanded()
        {
            if (Children?.Count == 1 && Children.FirstOrDefault() == null)
            {
                Children = new ObservableCollection<QuickSelectData>();
                if (Current == null)
                    return;
                if (Type == EnumType.ListFamily)
                {
                    ICollection<Element> tempParent = (ICollection<Element>)Current;
                    IEnumerable<IGrouping<string?, Element>> tempchildren = tempParent
                        .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                    foreach (IGrouping<string?, Element> child in tempchildren)
                    {
                        Children?.Add(new QuickSelectData(child, keyword, this));
                    }
                }
                else if (Type == EnumType.Category)
                {
                    IGrouping<string?, Element> tempParent = (IGrouping<string?, Element>)Current;
                    var myList = tempParent.GroupBy(e => e.Name);
                    foreach (IGrouping<string?, Element> item in myList)
                    {
                        Children.Add(new QuickSelectData(item.ToList(), keyword, this));
                    }
                }
                else if (Type == EnumType.Element)
                {
                    List<Element> tempParent = (List<Element>)Current;
                    List<Parameter> para = new List<Parameter>();
                    foreach (var ele in tempParent)
                    {
                        if (CheckSubString(keyword, ele.Category.Name))
                        {
                            foreach (Parameter item in ele.Parameters)
                            {
                                para.Add(item);
                            }
                            break;
                        }
                        foreach (Parameter item in ele.Parameters)
                        {
                            if (item.Definition == null) continue;
                            if (CheckSubString(keyword, item.Definition.Name))
                            {
                                para.Add(item);
                                continue;
                            }
                            if (item.StorageType == StorageType.ElementId)
                            {
                                if (item.AsElementId() == null || item.AsElementId().ToString() == "-1")
                                {
                                    if (CheckSubString(keyword, "<null>"))
                                        para.Add(item);
                                }
                                else
                                {
                                    if (CheckSubString(keyword, item.AsElementId().ToString()))
                                        para.Add(item);
                                }
                            }
                            else if (item.StorageType == StorageType.String)
                            {
                                if (item.AsValueString() == null)
                                {
                                    if (CheckSubString(keyword, "<null>"))
                                        para.Add(item);
                                }
                                else
                                {
                                    if (CheckSubString(keyword, item.AsValueString()))
                                        para.Add(item);
                                }
                            }
                            else if (item.StorageType == StorageType.Double)
                            {
                                if (CheckSubString(keyword, item.AsDouble().ToString()))
                                    para.Add(item);
                            }
                            else if (item.StorageType == StorageType.Integer)
                            {
                                if (item.AsElementId() == null) continue;
                                if (CheckSubString(keyword, item.AsInteger().ToString()))
                                    para.Add(item);
                            }
                            else if (item.StorageType == StorageType.None)
                            {
                                if (CheckSubString(keyword, item.AsInteger().ToString()))
                                    para.Add(item);
                            }
                            //if (item.AsValueString() == null)
                            //{
                            //    if (CheckSubString(keyword, ""))
                            //        para.Add(item);
                            //}
                            //else
                            //{
                            //    if (CheckSubString(keyword, item.AsValueString()))
                            //        para.Add(item);
                            //}
                        }
                    }
                    para.Sort(new ParaComparer());
                    //if (keyword != string.Empty)
                    //{
                    foreach (Parameter item in para)
                    {
                        Children.Add(new QuickSelectData(item, keyword, this));
                    }
                    //}
                    //else
                    //{
                    //    for (int i = 0; i < para.Count; i++)
                    //    {
                    //        if (i % 2 == 0)
                    //        {
                    //            Children.Add(new QuickSelectData(para[i], keyword, this));
                    //        }
                    //        else continue;
                    //    }
                    //}
                }
                else if (Type == EnumType.Parameter)
                {
                    List<string> temp = new List<string>();
                    Parameter par = Current as Parameter;
                    List<Element>? elements = Parent?.Current as List<Element>;
                    bool check = true;
                    if (CheckSubString(keyword, elements.First().Category.Name) && keyword != string.Empty)
                    {
                        check = true;
                        foreach (Element ele in elements)
                        {
                            var storageType = par.StorageType;
                            if (storageType == StorageType.ElementId)
                            {
                                if (ele.get_Parameter(par.Definition).AsElementId() == null)
                                    temp.Add("<null>");
                                else if (ele.get_Parameter(par.Definition).AsElementId().ToString() == "-1")
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsElementId().ToString());
                            }
                            else if ((storageType == StorageType.String))
                            {
                                if (ele.get_Parameter(par.Definition).AsValueString() == null)
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsValueString());
                            }
                            else if ((storageType == StorageType.Double))
                            {
                                if (ele.get_Parameter(par.Definition).AsDouble() == null)
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsDouble().ToString());
                            }
                            else if ((storageType == StorageType.Integer))
                            {
                                if (ele.get_Parameter(par.Definition).AsInteger() == null)
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsInteger().ToString());
                            }
                            else if ((storageType == StorageType.None))
                            {
                                if (ele.get_Parameter(par.Definition).AsInteger() == null)
                                    temp.Add("<None>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsInteger().ToString());
                            }
                        }
                    }
                    else
                    {
                        check = false;
                        foreach (Element? ele in elements)
                        {
                            var storageType = par.StorageType;
                            if (storageType == StorageType.ElementId)
                            {
                                if (ele.get_Parameter(par.Definition).AsElementId() == null)
                                    temp.Add("<null>");
                                else if (ele.get_Parameter(par.Definition).AsElementId().ToString() == "-1")
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsElementId().ToString());
                            }
                            else if ((storageType == StorageType.String))
                            {
                                if (ele.get_Parameter(par.Definition).AsValueString() == null)
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsValueString());
                            }
                            else if ((storageType == StorageType.Double))
                            {
                                if (ele.get_Parameter(par.Definition).AsDouble() == null)
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsDouble().ToString());
                            }
                            else if ((storageType == StorageType.Integer))
                            {
                                if (ele.get_Parameter(par.Definition).AsInteger() == null)
                                    temp.Add("<null>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsInteger().ToString());
                            }
                            else if ((storageType == StorageType.None))
                            {
                                if (ele.get_Parameter(par.Definition).AsInteger() == null)
                                    temp.Add("<None>");
                                else
                                    temp.Add(ele.get_Parameter(par.Definition).AsInteger().ToString());
                            }
                        }
                    }
                    if (check)
                    {
                        temp.Sort();
                        HashSet<string> values = new HashSet<string>(temp);
                        foreach (var v in values)
                        {
                            Children.Add(new QuickSelectData(v, keyword, this));
                        }
                    }
                    else if (!check)
                    {
                        temp.Sort();
                        HashSet<string> values = new HashSet<string>(temp);
                        foreach (var v in values)
                        {
                            if (CheckSubString(keyword, v))
                                Children.Add(new QuickSelectData(v, keyword, this));
                        }
                    }
                }
                foreach (var child in Children)
                {
                    child.IsChecked = IsChecked;
                }
            }
        }
        #endregion
    }
    public class ParaComparer : IComparer<Parameter>
    {
        public int Compare(Parameter x, Parameter y)
        {
            return x.Definition.Name.CompareTo(y.Definition.Name);
        }
    }
}
