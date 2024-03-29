﻿using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ShieldVSExtension.ToolWindows
{
    [Guid("23b0bdd9-76b7-4917-a75e-e29a99bcd863")]
    public class ConfigurationWindow : ToolWindowPane
    {
        public ConfigurationWindow() : base(null)
        {
            Caption = "ConfigurationWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new ConfigurationWindowControl(null);
        }
    }
}
