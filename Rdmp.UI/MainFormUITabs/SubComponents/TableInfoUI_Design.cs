using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.MainFormUITabs.SubComponents;

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<TableInfoUI_Design, UserControl>))]
public abstract class TableInfoUI_Design : RDMPSingleDatabaseObjectControl<TableInfo>;