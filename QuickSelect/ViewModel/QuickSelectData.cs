using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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
        public string? Name { get; set; }
        IGrouping<string?, Element>? Parent { get; set; }
        [ObservableProperty]
        public ObservableCollection<QuickSelectData>? children;
        public QuickSelectData(IGrouping<string?, Element> inumElement)
        {
            Parent = inumElement;
            Children = new ObservableCollection<QuickSelectData>();            
            Name = inumElement.Key;
            ClearChildren();
        }
        public QuickSelectData(Element element)
        {
            Children = new ObservableCollection<QuickSelectData>();
            Name = element.Name;
            ClearChildren();
        }
        public bool IsExpanded
        {
            get
            {
                if(Children?.Count==1 && Children.First() ==null) return false;
                else return true;
            }
            set
            {
                if (value == true)
                    Expanded();
                else ClearChildren();
            }
        }
        private void ClearChildren()
        {
            Children = new ObservableCollection<QuickSelectData>();
            Children.Add(null);
        }
        #region Command
        [RelayCommand]
        private void Click(Object o)
        {
            MessageBox.Show("Message");
        }
        [RelayCommand]
        private void Expanded()
        {
            if (Parent == null)
                return;
            Children = new ObservableCollection<QuickSelectData>();
            foreach (var p in Parent)
            {
                Children?.Add(new QuickSelectData(p));
            }
        }
        #endregion
    }
}
