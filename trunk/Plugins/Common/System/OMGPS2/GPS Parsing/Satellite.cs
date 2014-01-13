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

// Sample code found here: http://channel9.msdn.com/coding4fun/articles/Where-the-Heck-am-I-Connecting-NET-20-to-a-GPS

using System;
using System.Collections.Generic;

namespace OMGPS2
{
	public class Satellite
	{
		public int Id;
		public bool Used;		
		public int PRN;
		public int SignalQuality;
		public int Azimuth;
		public int Elevation;
		public object Thing;
	}

    public class Satellites : Dictionary<int, Satellite>
    {
        public void Add(Satellite sat)
        {
            if (this.ContainsKey(sat.Id))
            {
                Satellite existingSat = this[sat.Id];
                existingSat.Azimuth = sat.Azimuth;
                existingSat.Elevation = sat.Elevation;
                existingSat.PRN = sat.PRN;
                existingSat.SignalQuality = sat.SignalQuality;
                existingSat.Used = sat.Used;
            }
            else
            {
                this.Add(sat.Id, sat);
            }
        }
    }
}