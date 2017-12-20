using System;
using System.Drawing;

namespace ReusableLibraryCode.Icons.IconProvision
{
    public class DatabaseTypeIconProvider
    {
        private Bitmap _msBig;
        private Bitmap _msOverlay;

        private Bitmap _oraBig;
        private Bitmap _oraOverlay;

        private Bitmap _mysBig;
        private Bitmap _mysOverlay;

        public DatabaseTypeIconProvider()
        {
            _msBig = DatabaseProviderIcons.Microsoft;
            _msOverlay = DatabaseProviderIcons.MicrosoftOverlay;

            _mysBig = DatabaseProviderIcons.MySql;
            _mysOverlay = DatabaseProviderIcons.MySqlOverlay;

            _oraBig = DatabaseProviderIcons.Oracle;
            _oraOverlay = DatabaseProviderIcons.OracleOverlay;
        }

        public Bitmap GetOverlay(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return _msOverlay;
                case DatabaseType.MYSQLServer:
                    return _mysOverlay;
                case DatabaseType.Oracle:
                    return _oraOverlay;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public Bitmap GetImage(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return _msBig;
                case DatabaseType.MYSQLServer:
                    return _mysBig;
                case DatabaseType.Oracle:
                    return _oraBig;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
