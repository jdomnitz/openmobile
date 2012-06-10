/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMobile.Math
{
    public class Conversions
    {
        public static double DegreeToRadian(double angle)
        {
            return System.Math.PI * angle / 180.0;
        }
        public static float DegreeToRadian(float angle)
        {
            return (float)(System.Math.PI * angle / 180.0);
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / System.Math.PI);
        }
        public static float RadianToDegree(float angle)
        {
            return (float)(angle * (180.0 / System.Math.PI));
        }
    }
}
