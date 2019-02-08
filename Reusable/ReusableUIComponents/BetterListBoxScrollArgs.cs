// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace ReusableUIComponents
{
    public class BetterListBoxScrollArgs
    {
        // Scroll event argument
        private int mTop;
        private bool mTracking;
        public BetterListBoxScrollArgs(int top, bool tracking)
        {
            mTop = top;
            mTracking = tracking;
        }
        public int Top
        {
            get { return mTop; }
        }
        public bool Tracking
        {
            get { return mTracking; }
        }
    }
}