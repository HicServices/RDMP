using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders;
/// <summary>
/// named colours for use within RDMP
/// </summary>
public static class StatusColours
{
    private static Color Black = Color.FromArgb(33, 37, 41);
    private static Color White = Color.FromArgb(242, 240, 239);

    public static Color Core = Color.FromArgb(138, 196, 75);
    public static Color CoreCompliment = Color.White;

    public static Color Internal = Color.FromArgb(240,68,56);
    public static Color InternalCompliment = White;


    public static Color ProjectSpecific = Color.FromArgb(71, 142, 204);
    public static Color ProjectSpecificCompliment = White;

    public static Color Deprecated = Color.FromArgb(97,126,140);
    public static Color DeprecatedCompliment = White;

    public static Color Supplemental = Color.FromArgb(133, 183, 17);//todo
    public static Color SupplementalCompliment = Black;

    public static Color SpecialistApproval = Color.FromArgb(248,152,29);
    public static Color SpecialistApprovalCompliment = White;

    public static Color ExtractionIdentifier = Core;
    public static Color ExtractionIdentifierCompliment = CoreCompliment;

    public static Color PrimaryKey = Color.FromArgb(227, 211, 35);
    public static Color PrimaryKeyCompliment = Black;

    public static Color HashOnRelease = Color.FromArgb(108, 167, 235);
    public static Color HashOnReleaseCompliment = Black;

    public static Color Frozen = Color.FromArgb(171, 209, 221);
    public static Color FrozenCompliment = White;

    public static Color Template = Color.FromArgb(246, 215, 176);
    public static Color TemplateCompliment = Black;


    public static Color Unknown = Color.FromArgb(255, 105, 180);
    public static Color UnknownCompliment = Black;
}
