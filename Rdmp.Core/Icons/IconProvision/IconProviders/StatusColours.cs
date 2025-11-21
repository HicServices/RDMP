using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders;

public static class StatusColours
{
    private static Color Black = Color.FromArgb(33, 37, 41);
    private static Color White = Color.FromArgb(242, 240, 239);

    public static Color Core = Color.FromArgb(60, 136, 103);
    public static Color CoreCompliment = Color.White;

    public static Color Internal = Color.FromArgb(253, 22, 22);
    public static Color InternalCompliment = Black;


    public static Color ProjectSpecific = Color.FromArgb(27, 220, 242);
    public static Color ProjectSpecificCompliment = Black;

    public static Color Deprecated = Color.FromArgb(217, 217, 217);
    public static Color DeprecatedCompliment = Black;

    public static Color Supplemental = Color.FromArgb(133, 183, 17);
    public static Color SupplementalCompliment = Black;

    public static Color SpecialistApproval = Color.FromArgb(255, 96, 10);
    public static Color SpecialistApprovalCompliment = Black;

    public static Color ExtractionIdentifier = Core;
    public static Color ExtractionIdentifierCompliment = CoreCompliment;

    public static Color PrimaryKey = Color.FromArgb(227, 211, 35);
    public static Color PrimaryKeyCompliment = Black;

    public static Color HashOnRelease = Color.FromArgb(108, 167, 235);
    public static Color HashOnReleaseCompliment = Black;

    public static Color Frozen = Color.FromArgb(171, 209, 221);
    public static Color FrozenCompliment = Black;

    public static Color Template = Color.FromArgb(246, 215, 176);
    public static Color TemplateCompliment = Black;


    public static Color Unknown = Color.FromArgb(255, 105, 180);
    public static Color UnknownCompliment = Black;
}
