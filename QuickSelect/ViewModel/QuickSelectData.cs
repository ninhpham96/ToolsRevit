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
            this.parent = parent;
        }
        public QuickSelectData(IGrouping<string?, Element> InumElements, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Category;
            Current = InumElements;
            Parent = parent;
            Name = InumElements.Key;
            ClearChildren();
        }
        public QuickSelectData(List<Element> elements, QuickSelectData parent)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Element;
            Current = elements;
            Parent = parent;
            Name = elements.FirstOrDefault().Name;
            ClearChildren();
        }
        #endregion
        #region methods
        private void ClearChildren()
        {
            Children = new ObservableCollection<QuickSelectData>();
            if (Type == EnumType.Element) return;
            Children?.Add(null);
        }
        #endregion
        #region Command
        [RelayCommand]
        private void Expanded()
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
                    Children?.Add(new QuickSelectData(child, this));
                }
            }
            else if (Type == EnumType.Category)
            {
                IGrouping<string?, Element> tempParent = (IGrouping<string?, Element>)Current;
                var myList = tempParent.GroupBy(e => e.Name);
                foreach (var t in myList)
                {
                    Children.Add(new QuickSelectData(t.ToList(), this));
                }
            }
            try
            {
                //if (IsChecked == null) return;
                foreach (var child in Children)
                {
                    child.IsChecked = IsChecked;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        #endregion
    }
}
