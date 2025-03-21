using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.ExtensionsUtils
{
    public class FamilyUitls
    {
        public static FamilySymbol GetFamilySymbol(Family family, string symName = "")
        {
            var familyDoc = family.Document;
            List<FamilySymbol> symbols = family.GetFamilySymbolIds()
                .Select(a => familyDoc.GetElement(a))
                .Cast<FamilySymbol>()
                .ToList();

            FamilySymbol familySymbol = symbols.FirstOrDefault(a => a.Name == symName);
            if (familySymbol == null)
            {
                bool isHaveName = string.IsNullOrEmpty(symName);
                familySymbol = isHaveName ? symbols.First() : symbols.First().Duplicate(symName) as FamilySymbol;
            }

            if (!familySymbol.IsActive)
            {
                familySymbol.Activate();
            }

            return familySymbol;
        }
    }
}
