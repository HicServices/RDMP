using System;
using System.Windows.Forms;

namespace Rdmp.UI.Collections
{
    public class MenuBuiltEventArgs : EventArgs
    {
        /// <summary>
        /// The right click context menu that has just been built
        /// </summary>
        public ContextMenuStrip Menu { get; }

        /// <summary>
        /// The object for which the <see cref="Menu"/> was built
        /// </summary>
        public object Obj { get; }

        public MenuBuiltEventArgs(ContextMenuStrip menu, object obj)
        {
            Menu = menu;
            Obj = obj;
        }
    }
}