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
using System.Xml.Serialization;

namespace OpenMobile.NavEngine
{
    /// <summary>
    /// The maneuver type
    /// </summary>
    public enum NavigationStepManeuver
    {
        Unknown,
        TurnSharpLeft,
        UTurnRight,
        TurnSlightRight,
        Merge,
        RoundaboutLeft,
        RoundaboutRight,
        UTurnLeft,
        TurnSlightLeft,
        TurnLeft,
        RampRight,
        TurnRight,
        ForkRight,
        Straight,
        ForkLeft,
        FerryTrain,
        TurnSharpRight,
        RampLeft,
        Ferry,    
        KeepLeft,
        KeepRight,
        Take1stRight,
        Take2ndRight,
        Take1stLeft,
        Take2ndLeft
    }

    /// <summary>
    /// A navigation step 
    /// </summary>
    public class NavigationStep
    {
        private GDirectionStep _DirectionStep;

        #region Public properties

        /// <summary>
        /// The distance of this step
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
        /// Duration of this step
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
        /// End address for this step
        /// </summary>
        public Position EndPosition
        {
            get
            {
                return this._EndPosition;
            }
            set
            {
                if (this._EndPosition != value)
                {
                    this._EndPosition = value;
                }
            }
        }
        private Position _EndPosition;

        /// <summary>
        /// Start location for this step
        /// </summary>
        public Position StartPosition
        {
            get
            {
                return this._StartPosition;
            }
            set
            {
                if (this._StartPosition != value)
                {
                    this._StartPosition = value;
                }
            }
        }
        private Position _StartPosition;

        /// <summary>
        /// The travelmode for this step
        /// </summary>
        public NavigationTravelMode TravelMode
        {
            get
            {
                return this._TravelMode;
            }
            set
            {
                if (this._TravelMode != value)
                {
                    this._TravelMode = value;
                }
            }
        }
        private NavigationTravelMode _TravelMode;

        /// <summary>
        /// The instructions for this step
        /// </summary>
        public string Instructions
        {
            get
            {
                return this._Instructions;
            }
            set
            {
                if (this._Instructions != value)
                {
                    this._Instructions = value;
                }
            }
        }
        private string _Instructions;

        /// <summary>
        /// Any additional instructions for any sub steps
        /// </summary>
        public List<string> SubInstructions
        {
            get
            {
                return this._SubInstructions;
            }
            set
            {
                if (this._SubInstructions != value)
                {
                    this._SubInstructions = value;
                }
            }
        }
        private List<string> _SubInstructions = new List<string>();

        /// <summary>
        /// The short text indicating the maneuver type
        /// </summary>
        public string ManeuverText
        {
            get
            {
                return this._ManeuverText;
            }
            set
            {
                if (this._ManeuverText != value)
                {
                    this._ManeuverText = value;
                }
            }
        }
        private string _ManeuverText;

        /// <summary>
        /// The navigation manuever to perform
        /// </summary>
        public NavigationStepManeuver Maneuver
        {
            get
            {
                return this._Maneuver;
            }
            set
            {
                if (this._Maneuver != value)
                {
                    this._Maneuver = value;
                }
            }
        }
        private NavigationStepManeuver _Maneuver;        

        /// <summary>
        /// Latitude and longitude points for this step
        /// </summary>
        public List<Position> PositionPoints
        {
            get
            {
                return this._PositionPoints;
            }
            set
            {
                if (this._PositionPoints != value)
                {
                    this._PositionPoints = value;
                }
            }
        }
        private List<Position> _PositionPoints;

        /// <summary>
        /// True indicates this navigation steps is executed
        /// </summary>
        [XmlIgnore]
        public bool StepExecuted
        {
            get
            {
                return this._StepExecuted;
            }
            set
            {
                if (this._StepExecuted != value)
                {
                    this._StepExecuted = value;
                }
            }
        }
        private bool _StepExecuted;

        /// <summary>
        /// The distance from the beginning of the route to this step
        /// </summary>
        public Distance DistanceFromStart
        {
            get
            {
                return this._DistanceFromStart;
            }
            set
            {
                if (this._DistanceFromStart != value)
                {
                    this._DistanceFromStart = value;
                }
            }
        }
        private Distance _DistanceFromStart;

        /// <summary>
        /// The distance from this step to the next step.
        /// </summary>
        public Distance DistanceToNextStep
        {
            get
            {
                return this._DistanceToNextStep;
            }
            set
            {
                if (this._DistanceToNextStep != value)
                {
                    this._DistanceToNextStep = value;
                }
            }
        }
        private Distance _DistanceToNextStep;

        #endregion

        #region Constructors

        private NavigationStep()
        {
        }

        public NavigationStep(GDirectionStep directionStep)
        {
            _DirectionStep = directionStep;
            _Distance = new Distance(directionStep.DistanceValue, DistanceUnit.Meters);
            _DistanceText = directionStep.Distance;
            _Duration = new Duration(directionStep.DurationValue);
            _DurationText = directionStep.Duration;
            _EndPosition = new Position(new Latitude(directionStep.EndLocation.Lat), new Longitude(directionStep.EndLocation.Lng));
            _StartPosition = new Position(new Latitude(directionStep.StartLocation.Lat), new Longitude(directionStep.StartLocation.Lng));
            _TravelMode = (NavigationTravelMode)Enum.Parse(typeof(NavigationTravelMode), directionStep.TravelMode, true);
            _Instructions = StripHTML(directionStep.HtmlInstructions);

            // Split instructions into any sub instructions
            if (_Instructions.Contains('|'))
            {
                string[] subInstructions = _Instructions.Split('|');
                _Instructions = subInstructions[0];
                for (int i = 1; i < subInstructions.Length; i++)
                    _SubInstructions.Add(subInstructions[0]);
            }
            _PositionPoints = directionStep.Points.ConvertAll(x=>new Position(new Latitude(x.Lat), new Longitude(x.Lng))).ToList();

            _ManeuverText = directionStep.Maneuver;
            _Maneuver = Parse_Maneuver(_ManeuverText, _Instructions);
        }

        #endregion

        #region Step parsing

        private NavigationStepManeuver Parse_Maneuver(string maneuver, string instructions)
        {
            if (!String.IsNullOrEmpty(maneuver))
            {
                maneuver = maneuver.ToLower();
                switch (maneuver)
                {
                    case "turn-sharp-left":
                        return NavigationStepManeuver.TurnSharpLeft;
                    case "uturn-right":
                        return NavigationStepManeuver.UTurnRight;
                    case "turn-slight-right":
                        return NavigationStepManeuver.TurnSlightRight;
                    case "merge":
                        return NavigationStepManeuver.Merge;
                    case "roundabout-left":
                        return NavigationStepManeuver.RoundaboutLeft;
                    case "roundabout-right":
                        return NavigationStepManeuver.RoundaboutRight;
                    case "uturn-left":
                        return NavigationStepManeuver.UTurnLeft;
                    case "turn-slight-left":
                        return NavigationStepManeuver.TurnSlightLeft;
                    case "turn-left":
                        return NavigationStepManeuver.TurnLeft;
                    case "ramp-right":
                        return NavigationStepManeuver.RampRight;
                    case "turn-right":
                        return NavigationStepManeuver.TurnRight;
                    case "fork-right":
                        return NavigationStepManeuver.ForkRight;
                    case "straight":
                        return NavigationStepManeuver.Straight;
                    case "fork-left":
                        return NavigationStepManeuver.ForkLeft;
                    case "ferry-train":
                        return NavigationStepManeuver.FerryTrain;
                    case "turn-sharp-right":
                        return NavigationStepManeuver.TurnSharpRight;
                    case "ramp-left":
                        return NavigationStepManeuver.RampLeft;
                    case "ferry":
                        return NavigationStepManeuver.Ferry;
                    case "keep-left":
                        return NavigationStepManeuver.KeepLeft;
                    case "keep-right":
                        return NavigationStepManeuver.KeepRight;
                }
            }

            // If we got this far then no matching maneuver was detected, try to find additional maneuvers by parsing the instruction text
            instructions = instructions.ToLower();
            if (instructions.StartsWith("head "))
                return NavigationStepManeuver.Straight;
            if (instructions.StartsWith("take the 1st right "))
                return NavigationStepManeuver.Take1stRight;
            if (instructions.StartsWith("take the 2nd right "))
                return NavigationStepManeuver.Take2ndRight;
            if (instructions.StartsWith("take the 1st left "))
                return NavigationStepManeuver.Take1stLeft;
            if (instructions.StartsWith("take the 2nd left "))
                return NavigationStepManeuver.Take2ndLeft;
            if (instructions.StartsWith("take the ramp "))
                return NavigationStepManeuver.Straight;

            
            // No match, return unknown
            return NavigationStepManeuver.Unknown;

        }

        private string StripHTML(string htmlString)
        {
            return System.Text.RegularExpressions.Regex.Replace(htmlString.Replace("&nbsp;", " ").Replace("/", " ").Replace("<div", "| <div"), "<.*?>", string.Empty);
        }

        #endregion
        
        public override string ToString()
        {
            StringBuilder instructions = new StringBuilder();
            instructions.Append(_Instructions);
            foreach (var subInstruction in _SubInstructions)
                instructions.Append(subInstruction);

            return String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", _TravelMode, _Maneuver, _Distance, _DistanceFromStart, _DistanceToNextStep, _Duration, instructions);
        }
    }
}
