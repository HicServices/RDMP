using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.SimpleDialogs.Automation;

namespace CatalogueManager.Menus
{
    public class AutomationServiceSlotMenu:RDMPContextMenuStrip
    {
        public AutomationServiceSlotMenu(RDMPContextMenuStripArgs args, AutomationServiceSlot slot): base(args, slot)
        {
            AddCommonMenuItems();
        }
    }
}
