// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableUIComponents.Dependencies.Models;

namespace ReusableUIComponents.Performance.PerformanceStackPath
{
    class StackFrameVisualiser:IObjectVisualisation
    {
        private readonly int _worstOffenderCount;

        public StackFrameVisualiser(int worstOffenderCount)
        {
            _worstOffenderCount = worstOffenderCount;
        }

        public Bitmap GetImage(object toRender)
        {
            return Images.ViewDependencies;
        }

        public OrderedDictionary EntityInformation(object toRender)
        {
            var s = (StackFrame) toRender;
            var dictionary = new OrderedDictionary();
            dictionary.Add("Stack Frame" ,s.Frame);
            return dictionary;
        }

        public ColorResponse GetColor(object toRender, ColorRequest request)
        {
            var frame = (StackFrame)toRender;

            double fraction = (double)(frame.TimesCalled)/_worstOffenderCount;
            var color = PerformanceCounterResultsUI.GetHeat(fraction);


            return new ColorResponse(GetNearestKnownColor(color), KnownColor.White);
        }


        private KnownColor GetNearestKnownColor(Color input_color)
        {

            double dbl_input_red = Convert.ToDouble(input_color.R);
            double dbl_input_green = Convert.ToDouble(input_color.G);
            double dbl_input_blue = Convert.ToDouble(input_color.B);
            double distance = 500.0;

            KnownColor nearest_color = KnownColor.White;

            foreach (KnownColor kc in Enum.GetValues(typeof(KnownColor)))
            {
                var o = Color.FromKnownColor(kc);

                // compute the Euclidean distance between the two colors
                // note, that the alpha-component is not used in this example
                var dbl_test_red = Math.Pow(Convert.ToDouble(((Color)o).R) - dbl_input_red, 2.0);
                var dbl_test_green = Math.Pow(Convert.ToDouble(((Color)o).G) - dbl_input_green, 2.0);
                var dbl_test_blue = Math.Pow(Convert.ToDouble(((Color)o).B) - dbl_input_blue, 2.0);

                // it is not necessary to compute the square root
                // it should be sufficient to use:
                // temp = dbl_test_blue + dbl_test_green + dbl_test_red;
                // if you plan to do so, the distance should be initialized by 250000.0
                var temp = Math.Sqrt(dbl_test_blue + dbl_test_green + dbl_test_red);
                // explore the result and store the nearest color
                if (temp == 0.0)
                {
                    // the lowest possible distance is - of course - zero
                    // so I can break the loop (thanks to Willie Deutschmann)
                    // here I could return the input_color itself
                    // but in this example I am using a list with named colors
                    // and I want to return the Name-property too
                    nearest_color = kc;
                    break;
                }
                if (temp < distance)
                {
                    distance = temp;
                    nearest_color = kc;
                }
            }

            return nearest_color;
        }

        public string[] GetNameAndType(object toRender)
        {
            return new[] { ((StackFrame)toRender).GetMethodName(), ((StackFrame)toRender).TimesCalled.ToString(CultureInfo.InvariantCulture) };
        }
    }
}
