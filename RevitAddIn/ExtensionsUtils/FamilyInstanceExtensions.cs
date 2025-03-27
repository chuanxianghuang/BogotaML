using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.ExtensionsUtils
{
    public static class FamilyInstanceExtensions
    {
        public static List<Connector> GetConnectors(this FamilyInstance familyInstance)
        {
            var connectorSet = familyInstance?.MEPModel?.ConnectorManager?.Connectors;
            var connectors = connectorSet?.Cast<Connector>().ToList();
            return connectors;
        }

        public static XYZ GetLocation(this FamilyInstance familyInstance)
        {
            var locationPoint = familyInstance.Location as LocationPoint;
            return locationPoint?.Point;
        }

        public static Curve GetLocationCurve(this FamilyInstance familyInstance)
        {
            var locationCurve = familyInstance.Location as LocationCurve;
            return locationCurve?.Curve;
        }
    }
}
