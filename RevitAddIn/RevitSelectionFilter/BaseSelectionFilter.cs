using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.RevitSelectionFilter
{
    public abstract class BaseSelectionFilter : ISelectionFilter
    {
        protected readonly Func<Element, bool> ValidateElement;

        public BaseSelectionFilter(Func<Element, bool> validateElement)
        {
            ValidateElement = validateElement;
        }

        public abstract bool AllowElement(Element elem);

        public abstract bool AllowReference(Reference reference, XYZ position);
    }
}
