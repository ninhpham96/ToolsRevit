using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSelect
{
    public class Data
    {
        private static Data? _instance;
        private Data()
        {
        }
        public static Data Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Data();

                return _instance;
            }
        }
        public List<Family> GetAllFamilyInstances(Document doc)
        {
            var res = new FilteredElementCollector(doc).OfClass(typeof(Family)).Cast<Family>().ToList();
            return res;
        }
        public List<string> GetAllFamilyNameInView(
        Document doc)
        {
            return new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .ToElements()
                    .Select<Element, string>(e => e.Category != null ? e.Category.Name : string.Empty)
                    .Distinct<string>().Where(p => p != string.Empty).ToList();
        }
        public ICollection<Element> GetAllElementsInView(Document doc)
        {
             return new FilteredElementCollector(doc,doc.ActiveView.Id).ToElements();             
        }
        public ICollection<Element> GetAllElementsInProject(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();
        }
    }
}
