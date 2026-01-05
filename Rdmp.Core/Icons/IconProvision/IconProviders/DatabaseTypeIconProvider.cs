using FAnsi;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    internal class DatabaseTypeIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            if (concept is DatabaseType dt)
            {
                return dt switch
                {
                    DatabaseType.MicrosoftSQLServer => Image.Load<Rgba32>(CatalogueIcons.Microsoft),
                    DatabaseType.MySql => Image.Load<Rgba32>(CatalogueIcons.MySql),
                    DatabaseType.Oracle => Image.Load<Rgba32>(CatalogueIcons.Oracle),
                    DatabaseType.PostgreSql => Image.Load<Rgba32>(CatalogueIcons.PostgreSQL),
                    _ => null
                };
            }

            throw new NotImplementedException();
        }
    }
}
