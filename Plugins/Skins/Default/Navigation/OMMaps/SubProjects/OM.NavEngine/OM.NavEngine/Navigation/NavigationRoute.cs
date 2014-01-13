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
using DotSpatial.Positioning;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace OpenMobile.NavEngine
{
    public class NavigationRoute
    {
        #region Private variables

        private int _CurrentRouteIndex = 0;

        #endregion

        #region public properties

        /// <summary>
        /// The GMap directions object
        /// </summary>
        public GDirections GMapDirectionsObject
        {
            get
            {
                return this._GMapDirectionsObject;
            }
            set
            {
                if (this._GMapDirectionsObject != value)
                {
                    this._GMapDirectionsObject = value;
                }
            }
        }
        private GDirections _GMapDirectionsObject;

        /// <summary>
        /// The GMap route points
        /// </summary>
        public List<PointLatLng> GMapRoutePoints
        {
            get
            {
                return this._GMapRoutePoints;
            }
            set
            {
                if (this._GMapRoutePoints != value)
                {
                    this._GMapRoutePoints = value;
                }
            }
        }
        private List<PointLatLng> _GMapRoutePoints;        

        /// <summary>
        /// Textual representation of the distance
        /// </summary>
        public string DistanceText
        {
            get
            {
                return this._DistanceText;
            }
            set
            {
                if (this._DistanceText != value)
                {
                    this._DistanceText = value;
                }
            }
        }
        private string _DistanceText;

        /// <summary>
        /// Textual representation of the duration
        /// </summary>
        public string DurationText
        {
            get
            {
                return this._DurationText;
            }
            set
            {
                if (this._DurationText != value)
                {
                    this._DurationText = value;
                }
            }
        }
        private string _DurationText;

        /// <summary>
        /// The duration for this route
        /// </summary>
        public Duration Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                if (this._Duration != value)
                {
                    this._Duration = value;
                }
            }
        }
        private Duration _Duration;

        /// <summary>
        /// The distance of this route
        /// </summary>
        public Distance Distance
        {
            get
            {
                return this._Distance;
            }
            set
            {
                if (this._Distance != value)
                {
                    this._Distance = value;
                }
            }
        }
        private Distance _Distance;

        /// <summary>
        /// End address for this route
        /// </summary>
        public KnownPosition EndLocation
        {
            get
            {
                return this._EndLocation;
            }
            set
            {
                if (this._EndLocation != value)
                {
                    this._EndLocation = value;
                }
            }
        }
        private KnownPosition _EndLocation;

        /// <summary>
        /// Start location for this route
        /// </summary>
        public KnownPosition StartLocation
        {
            get
            {
                return this._StartLocation;
            }
            set
            {
                if (this._StartLocation != value)
                {
                    this._StartLocation = value;
                }
            }
        }
        private KnownPosition _StartLocation;

        /// <summary>
        /// Summary for this route
        /// </summary>
        public string Summary
        {
            get
            {
                return this._Summary;
            }
            set
            {
                if (this._Summary != value)
                {
                    this._Summary = value;
                }
            }
        }
        private string _Summary;

        /// <summary>
        /// The textual description for this route
        /// </summary>
        public string RouteText
        {
            get
            {
                return this._RouteText;
            }
            set
            {
                if (this._RouteText != value)
                {
                    this._RouteText = value.Replace("\n", "\r\n");
                }
            }
        }
        private string _RouteText;        

        /// <summary>
        /// The steps for this route
        /// </summary>
        public List<NavigationStep> RouteSteps
        {
            get
            {
                return this._RouteSteps;
            }
            set
            {
                if (this._RouteSteps != value)
                {
                    this._RouteSteps = value;
                }
            }
        }
        private List<NavigationStep> _RouteSteps = new List<NavigationStep>();

        /// <summary>
        /// The current step for this route
        /// </summary>
        public NavigationStep CurrentRouteStep
        {
            get
            {
                return this._RouteSteps[_CurrentRouteIndex];
            }
        }

        /// <summary>
        /// The copyright info for this route (must be shown to the user!)
        /// </summary>
        public String Copyright
        {
            get
            {
                return this._Copyright;
            }
            set
            {
                if (this._Copyright != value)
                {
                    this._Copyright = value;
                }
            }
        }
        private String _Copyright;

        #endregion

        #region GMap.Net objects

        /// <summary>
        /// Get's the GMap.NET route object which can be shown directly on a map
        /// </summary>
        /// <returns></returns>
        public GMapRoute GetGMapRoute()
        {
            return new GMapRoute(_GMapRoutePoints, _Summary);
        }

        #endregion

        #region FindInitialRouteStep

        /// <summary>
        /// Finds and set's the initial route start step (Finds the closest route step to the specified latitude and longitude with GMAP.Net's data format)
        /// </summary>
        /// <param name="currentPosition"></param>
        public void FindInitialRouteStep(PointLatLng currentPosition)
        {
            FindInitialRouteStep(new Position(new Latitude(currentPosition.Lat), new Longitude(currentPosition.Lng)));
        }

        /// <summary>
        /// Finds and set's the initial route start step (Finds the closest route step to the specified latitude and longitude)
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void FindInitialRouteStep(double latitude, double longitude)
        {
            FindInitialRouteStep(new Position(new Latitude(latitude), new Longitude(longitude)));
        }
        
        /// <summary>
        /// Finds and set's the initial route start step (Finds the closest route step to the specified position with Dot.Spatials position data type)
        /// </summary>
        /// <param name="currentPosition"></param>
        public void FindInitialRouteStep(Position currentPosition)
        {
            // Find the closest location in the route steps to the specified location
            int index = _RouteSteps.IndexOf(_RouteSteps.OrderBy(y => y.StartPosition.DistanceTo(currentPosition)).FirstOrDefault());
            if (index >= 0)
                _CurrentRouteIndex = index;

            for (int i = 0; i < index; i++)
                _RouteSteps[i].StepExecuted = true;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new route
        /// </summary>
        private NavigationRoute()
        {
        }

        /// <summary>
        /// Creates a new route from a Google directions object
        /// </summary>
        /// <param name="route"></param>
        /// <param name="directions"></param>
        public NavigationRoute(GMapRoute route, GDirections directions)
        {
            _GMapRoutePoints = route.Points;
            _GMapDirectionsObject = directions;
            _Copyright = directions.Copyrights;
            _Distance = new Distance(directions.DistanceValue, DistanceUnit.Meters);
            _DistanceText = directions.Distance;
            _Duration = new Duration(directions.DurationValue);
            _DurationText = directions.Duration;
            _EndLocation = new KnownPosition(new Position(new Latitude(directions.EndLocation.Lat), new Longitude(directions.EndLocation.Lng)), directions.EndAddress);
            _StartLocation = new KnownPosition(new Position(new Latitude(directions.StartLocation.Lat), new Longitude(directions.StartLocation.Lng)), directions.StartAddress);
            _Summary = directions.Summary;

            _RouteSteps = new List<NavigationStep>();
            foreach (var step in directions.Steps)
                _RouteSteps.Add(new NavigationStep(step));

            // Set initial start point in route
            _CurrentRouteIndex = 0;

            // Set textual description for this route
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}, {1}", directions.Summary, directions.Copyrights);
            sb.AppendLine();
            sb.AppendFormat("From {0}", _StartLocation.Address);
            sb.AppendLine();
            sb.AppendFormat("To {0}", _EndLocation.Address);
            sb.AppendLine();
            sb.AppendFormat("Distance {0}", _DistanceText);
            sb.AppendLine();
            sb.AppendFormat("Duration {0}", _DurationText);
            sb.AppendLine();
            foreach (var step in _RouteSteps)
            {
                sb.AppendFormat("After {0} {1}", step.DistanceText, step.Instructions);
                sb.AppendLine();
            }
            _RouteText = sb.ToString();

            CalculateRouteDistances();
        }

        #endregion

        #region Distance calculations

        /// <summary>
        /// Calculates the route distance data
        /// </summary>
        public void CalculateRouteDistances()
        {
            // Calculate distances to each step from start
            for (int i = 0; i < _RouteSteps.Count; i++)
            {
                _RouteSteps[i].DistanceFromStart = GetDistanceToStep(i);
                if (i != _RouteSteps.Count - 1)
                    _RouteSteps[i].DistanceToNextStep = GetDistanceFromStepToStep(i, i + 1);
            }
        }

        /// <summary>
        /// Get's the total distance of this route
        /// </summary>
        /// <returns></returns>
        public Distance GetTotalDistance()
        {
            Distance distance = new Distance();

            // calculate total distance between each point on the route line (The polyline returned by route object)
            for (int i = 0; i < _GMapRoutePoints.Count - 1; i++)
                distance += _GMapRoutePoints[i].ToPosition().DistanceTo(_GMapRoutePoints[i + 1].ToPosition(), true);

            return distance;
        }

        /// <summary>
        /// Get's the distance to a specific step
        /// </summary>
        /// <param name="index">Zero based step index</param>
        /// <returns></returns>
        public Distance GetDistanceToStep(int index)
        {
            Distance distance = new Distance();

            // calculate total distance between each point on the route line (The polyline returned by route object)
            int endIndex = GetRoutePointIndexFromRouteStepIndex(index);
            for (int i = 0; i < endIndex; i++)
                distance += _GMapRoutePoints[i].ToPosition().DistanceTo(_GMapRoutePoints[i + 1].ToPosition(), true);

            return distance;
        }

        /// <summary>
        /// Get's the distance from one step to another step
        /// </summary>
        /// <param name="startStep">Zero based step index</param>
        /// <param name="endStep">Zero based step index</param>
        /// <returns></returns>
        public Distance GetDistanceFromStepToStep(int startStep, int endStep)
        {
            Distance distance = new Distance();

            // calculate total distance between each point on the route line (The polyline returned by route object)
            int startIndex = GetRoutePointIndexFromRouteStepIndex(startStep);
            int endIndex = GetRoutePointIndexFromRouteStepIndex(endStep);
            for (int i = startIndex; i < endIndex; i++)
                distance += _GMapRoutePoints[i].ToPosition().DistanceTo(_GMapRoutePoints[i + 1].ToPosition(), true);

            return distance;
        }

        /// <summary>
        /// Get's the distance to the next route step from the specified position
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public Distance GetDistanceToNextStep(Position currentPosition)
        {
            return GetDistanceToStep(_CurrentRouteIndex, currentPosition);
        }

        /// <summary>
        /// Get's the distance to a specific route step from the specified position
        /// </summary>
        /// <param name="index">Zero based step index</param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public Distance GetDistanceToStep(int index, Position currentPosition)
        {
            int closestIndex = GetRoutePointClosestIndex(currentPosition);
            int side = 0;
            // Are we on the upper or lower side of this index
            if (closestIndex == 0)
            {   // We're on the upper side
                side = -1;
            }
            else if (closestIndex == _RouteSteps.Count - 1)
            {   // We're on the lower side
                side = 1;
            }
            else
            {   // Check distances to get side
                Distance distanceDown = currentPosition.DistanceTo(_RouteSteps[closestIndex - 1].EndPosition);
                Distance distanceUp = currentPosition.DistanceTo(_RouteSteps[closestIndex + 1].StartPosition);
                if (distanceDown.Value > distanceUp.Value)
                {   // We're at the lower side
                    side = -1;
                }
                else
                {   // We're at the upper side
                    side = 1;
                }
            }

            if (side > 1)
                closestIndex++;

            int routePointIndex_Start = closestIndex;
            int routePointIndex_End = GetRoutePointIndexFromRouteStepIndex(index);

            Distance distance = new Distance();

            // Add initial distance 
            distance += currentPosition.DistanceTo(_GMapRoutePoints[routePointIndex_Start].ToPosition(), true);

            // calculate total distance between each point on the route line (The polyline returned by route object)
            for (int i = 0; i < routePointIndex_End - 1; i++)
                distance += _GMapRoutePoints[i].ToPosition().DistanceTo(_GMapRoutePoints[i + 1].ToPosition(), true);

            return distance;
        }

        private int GetRoutePointIndexFromRouteStepIndex(int routeStepIndex, bool endPosition = false)
        {
            //return _GMapRoutePoints.IndexOf(_RouteSteps[routeStepIndex].StartPosition.ToPointLatLng());
            var points = _GMapRoutePoints.OrderBy(y => y.ToPosition().DistanceTo((endPosition ? _RouteSteps[routeStepIndex].EndPosition : _RouteSteps[routeStepIndex].StartPosition)));
            int closestIndex = _GMapRoutePoints.IndexOf(points.FirstOrDefault());
            return closestIndex;
        }

        private int GetRoutePointClosestIndex(Position point)
        {
            int closestIndex = _RouteSteps.IndexOf(_RouteSteps.OrderBy(y => y.StartPosition.DistanceTo(point)).FirstOrDefault());
            return closestIndex;
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0}|{1}|From {2} to {3}|{4}", _Distance, _Duration, _StartLocation, _EndLocation, _Summary);
        }

        #region XML

        /// <summary>
        /// Serializes a route to a xml string
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        static public string ToXML(NavigationRoute route)
        {
            return System.IO.XML.Serializer<NavigationRoute>.toXML(route, null);
        }

        /// <summary>
        /// Dezerializes a route from a xml string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        static public NavigationRoute FromXML(string xml)
        {
            return System.IO.XML.Serializer<NavigationRoute>.fromXML(xml, null);
        }

        #endregion
    }


}
