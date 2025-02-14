using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupBrowserUI_Design, UserControl>))]
public abstract class LookupBrowserUI_Design : RDMPSingleDatabaseObjectControl<Lookup>;