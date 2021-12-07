// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Sorts tool strip items by <see cref="IAtomicCommand.Weight"/>
    /// </summary>
    public partial class RDMPCollectionCommonFunctionality
    {
        public class ToolStripItemComparer : IComparer
        {
            private ToolStripItemCollection _originalOrder;

            public ToolStripItemComparer(ToolStripItemCollection originalOrder)
            {
                this._originalOrder = originalOrder;
            }

            public int Compare(object x, object y)
            {
                ToolStripItem oItem1 = (ToolStripItem)x;
                ToolStripItem oItem2 = (ToolStripItem)y;

                var cmd1 = oItem1.Tag as IAtomicCommand;
                var cmd2 = oItem2.Tag as IAtomicCommand;

                var explicitOrder = (cmd1?.Weight ?? 0) - (cmd2?.Weight ?? 0);

                // if there is no difference in explicit weight 
                if(explicitOrder == 0)
                {
                    // use the original weight it had in the menu
                    return _originalOrder.IndexOf(oItem1) - _originalOrder.IndexOf(oItem2);
                }

                return explicitOrder;
            }
        }

    }
}
