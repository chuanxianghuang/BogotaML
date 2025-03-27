﻿using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.UICommon
{
    public class ExecuteEventHandler : IExternalEventHandler
    {
        public string Name { get; private set; }

        public Action<UIApplication> ExecuteAction { get; set; }

        public ExecuteEventHandler(string name)
        {
            Name = name;
        }

        public void Execute(UIApplication app)
        {
            if (ExecuteAction != null)
            {
                try
                {
                    ExecuteAction(app);
                }
                catch
                { }
            }
        }

        public string GetName()
        {
            return Name;
        }
    }
}
