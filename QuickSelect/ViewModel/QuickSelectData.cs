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
            }
            set
            {
                isExpanded = value;
                OnPropertyChanged();
                if (value == true)
                    Expanded();
            }
        }

        [ObservableProperty]
        private bool? isChecked = false;

        [ObservableProperty]
        private ICollection<ElementId>? selectElements;

        [ObservableProperty]
        public ObservableCollection<QuickSelectData>? children;

        private string keyword = string.Empty;

        #endregion properties and field

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

        #endregion Constructor

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

        #endregion methods

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
                    var dataCategory = tempParent.GroupBy(e => e.Name);
                    List<Parameter> para = new List<Parameter>();
                    List<string> paraName = new List<string>();
                    foreach (IGrouping<string?, Element> group in dataCategory)
                    {
                        foreach (var ele in group.ToList())
                        {
                            if (string.IsNullOrWhiteSpace(keyword) || CheckSubString(keyword, ele.Category.Name))
                            {
                                foreach (Parameter item in ele.Parameters)
                                {
                                    if (item.Definition != null && !paraName.Any(x => x.Equals(item.Definition.Name)))
                                    {
                                        para.Add(item);
                                        paraName.Add(item.Definition.Name);
                                    }
                                }
                                continue;
                            }
                            foreach (Parameter item in ele.Parameters)
                            {
                                if (item.Definition == null || paraName.Any(x => x.Equals(item.Definition.Name))) continue;

                                if (CheckSubString(keyword, item.Definition.Name))
                                {
                                    para.Add(item);
                                    paraName.Add(item.Definition.Name);
                                    continue;
                                }

                                string value = string.Empty;
                                if (item.StorageType == StorageType.String)
                                {
                                    value = item.AsString();
                                }
                                else
                                {
                                    value = item.AsValueString();
                                }

                                if (string.IsNullOrWhiteSpace(value))
                                {
                                    value = "<null>";
                                }

                                if (CheckSubString(keyword, value))
                                {
                                    para.Add(item);
                                    paraName.Add(item.Definition.Name);
                                }
                            }
                        }
                    }
                    para.Sort(new ParaComparer());
                    foreach (Parameter item in para)
                    {
                        Children.Add(new QuickSelectData(item, keyword, this));
                    }
                }
                else if (Type == EnumType.Parameter)
                {
                    List<string> temp = new List<string>();
                    Parameter par = Current as Parameter;
                    List<Element>? elements = ((IGrouping<string?, Element>)Parent?.Current).ToList();
                    if (par.Definition == null && elements?.Count > 0)
                    {
                        return;
                    }

                    foreach (Element ele in elements)
                    {
                        Parameter param = ele.GetParameters(par.Definition.Name).FirstOrDefault();
                        if (param == null)
                        {
                            continue;
                        }
                        string value = string.Empty;
                        if (param.StorageType == StorageType.String)
                        {
                            value = param.AsString();
                        }
                        else
                        {
                            value = param.AsValueString();
                        }

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            value = "<null>";
                        }

                        if (string.IsNullOrWhiteSpace(keyword) || CheckSubString(keyword, ele.Category.Name) || CheckSubString(keyword, value))
                        {
                            temp.Add(value);
                        }
                    }

                    temp.Sort();
                    HashSet<string> values = new HashSet<string>(temp);
                    foreach (var v in values)
                    {
                        Children.Add(new QuickSelectData(v, keyword, this));
                    }
                }
                foreach (var child in Children)
                {
                    child.IsChecked = IsChecked;
                }
            }
        }

        #endregion Command
    }

    public class ParaComparer : IComparer<Parameter>
    {
        public int Compare(Parameter x, Parameter y)
        {
            return x.Definition.Name.CompareTo(y.Definition.Name);
        }
    }
}