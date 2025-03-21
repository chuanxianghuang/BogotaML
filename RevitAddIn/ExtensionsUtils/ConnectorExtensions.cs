using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.ExtensionsUtils
{
    public static class ConnectorExtensions
    {
        public static List<Connector> ToList(this ConnectorSet connectorSet)
        {
            var connectors = new List<Connector>();
            foreach (Connector connector in connectorSet)
            {
                connectors.Add(connector);
            }

            return connectors;
        }
    }
}
