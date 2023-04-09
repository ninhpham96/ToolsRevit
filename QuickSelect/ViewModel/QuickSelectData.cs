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

namespace QuickSelect.ViewModel
{
    public partial class QuickSelectData : ObservableObject
    {
        #region properties and field
        [ObservableProperty]
        private EnumType? type;
        public string? Name { get; set; }
        [ObservableProperty]
        private object? parent;
        public bool IsExpanded
        {
            get
            {
                if (Children?.Count == 1 && Children.First() == null) return false;
                else return true;
            }
            set
            {
                if (value == true)
                    Expanded();
                else ClearChildren();
            }
        }
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                value = isChecked;
                SetCheckForParent();
                SetCheckForChildren();
            }
        }
        [ObservableProperty]
        private ICollection<ElementId>? selectElements;
        [ObservableProperty]
        public ObservableCollection<QuickSelectData>? children;
        #endregion
        #region Constructor
        public QuickSelectData(ICollection<Element> elements)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.ListFamily;
            Parent = elements;
            Name = "Family";
            ClearChildren();
        }
        public QuickSelectData(IGrouping<string?, Element> InumElements)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Category;
            Parent = InumElements;
            Name = InumElements.Key;
            ClearChildren();
        }
        public QuickSelectData(List<Element> elements)
        {
            SelectElements = new List<ElementId>();
            Type = EnumType.Element;
            Parent = elements;
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
        private void SetCheckForChildren()
        {
            
        }
        private void SetCheckForParent()
        {
        }
        #endregion
        #region Command
        [RelayCommand]
        private void Expanded()
        {
            Children = new ObservableCollection<QuickSelectData>();
            if (Parent == null)
                return;
            if (Type == EnumType.ListFamily)
            {
                ICollection<Element> tempParent = (ICollection<Element>)Parent;
                IEnumerable<IGrouping<string?, Element>> tempchildren = tempParent
                    .Where(e => e.Category != null).GroupBy(e => e.Category != null ? e.Category.Name : null);
                foreach (IGrouping<string?, Element> child in tempchildren)
                {
                    Children?.Add(new QuickSelectData(child));
                }
            }
            else if (Type == EnumType.Category)
            {
                IGrouping<string?, Element> tempParent = (IGrouping<string?, Element>)Parent;
                var myList = tempParent.GroupBy(e => e.Name);
                foreach (var t in myList)
                {
                    Children.Add(new QuickSelectData(t.ToList()));
                }
            }
        }
        #endregion
    }
}
