using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.RevitSelectionFilter
{
    public class LinkElementSelectionFilter : BaseSelectionFilter
    {
        private readonly Document _doc;

        public LinkElementSelectionFilter(Document doc, Func<Element, bool> validateElement)
            : base(validateElement)
        {
            _doc = doc;
        }

        public override bool AllowElement(Element elem) => true;

        public override bool AllowReference(Reference reference, XYZ position)
        {
            var element = _doc.GetElement(reference.ElementId);
            if (element is RevitLinkInstance)
            {
                var linkInstance = element as RevitLinkInstance;
                var linkElement = linkInstance.GetLinkDocument().GetElement(reference.LinkedElementId);
                return ValidateElement(linkElement);
            }
            else
            {
                return ValidateElement(element);
            }
        }
    }
}
