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
using System.Linq;
using System.Text;

namespace OpenMobile.Graphics
{
    public static class Enums
    {
        /// <summary>
        /// Checks if a flag is set in an enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool IsSet<T>(T value, T flags) where T : struct
        {
            // You can add enum type checking to be perfectly sure that T is enum, this have some cost however
            // if (!typeof(T).IsEnum)
            //     throw new ArgumentException();
            long longFlags = Convert.ToInt64(flags);
            return (Convert.ToInt64(value) & longFlags) == longFlags;
        }
    }

    public static class Misc
    {
        public static OpenMobile.Graphics.Point ToOpenMobilePoint(this System.Drawing.Point point)
        {
            return new OpenMobile.Graphics.Point(point.X, point.Y);
        }

        public static System.Drawing.Point ToSystemPoint(this OpenMobile.Graphics.Point point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }

        public static OpenMobile.Graphics.Size ToOpenMobileSize(this System.Drawing.Size size)
        {
            return new OpenMobile.Graphics.Size(size.Width, size.Height);
        }

        public static System.Drawing.Size ToSystemSize(this OpenMobile.Graphics.Size size)
        {
            return new System.Drawing.Size(size.Width, size.Height);
        }

        public static OpenTK.Vector3 ToOpenTKVector3(this OpenMobile.Math.Vector3 v)
        {
            return new OpenTK.Vector3(v.X, v.Y, v.Z);
        }
        public static OpenTK.Vector3d ToOpenTKVector3d(this OpenMobile.Math.Vector3 v)
        {
            return new OpenTK.Vector3d(v.X, v.Y, v.Z);
        }

    }
}
