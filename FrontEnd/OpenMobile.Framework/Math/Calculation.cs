﻿/*********************************************************************************
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

namespace OpenMobile.Framework.Math
{
    /// <summary>
    /// Unit types to represent bytes
    /// </summary>
    public enum byteTypes
    {
        /// <summary>
        /// Bytes
        /// </summary>
        bytes,
        /// <summary>
        /// Kilobytes (2^10)bytes
        /// </summary>
        kilobytes,
        /// <summary>
        /// Megabytes (2^20)bytes
        /// </summary>
        megabytes,
        /// <summary>
        /// Gigabytes (2^30)bytes
        /// </summary>
        gigabytes,
        /// <summary>
        /// Terabytes (2^40)bytes
        /// </summary>
        terabytes
    }
    /// <summary>
    /// Unit types to represent Area
    /// </summary>
    public enum areaTypes
    {
        /// <summary>
        /// 100m^2
        /// </summary>
        are,
        /// <summary>
        /// 1000sq. meters
        /// </summary>
        hectare,
        /// <summary>
        /// Square Kilometre
        /// </summary>
        squareKilometre,
        /// <summary>
        /// Square Megametres
        /// </summary>
        squareMegametre,
        /// <summary>
        /// Square Foot
        /// </summary>
        squareFoot,
        /// <summary>
        /// Square Yard
        /// </summary>
        squareYard,
        /// <summary>
        /// Square Perch
        /// </summary>
        squarePerch,
        /// <summary>
        /// Square Acre
        /// </summary>
        acre,
        /// <summary>
        /// Square Mile
        /// </summary>
        squareMile
    }

    /// <summary>
    /// Unit types to represent Volume
    /// </summary>
    public enum volumeTypes
    {
        /// <summary>
        /// Cubic Meters
        /// </summary>
        cubicMeters,
        /// <summary>
        /// Cubic Decimeters
        /// </summary>
        cubicDecimeters,
        /// <summary>
        /// Cubic Centimeters
        /// </summary>
        cubicCentimeters,
        /// <summary>
        /// Cubic Millimeters
        /// </summary>
        cubicMillimeters,
        /// <summary>
        /// Hectoliters
        /// </summary>
        hectoliters,
        /// <summary>
        /// Liters
        /// </summary>
        liters,
        /// <summary>
        /// Centiliters
        /// </summary>
        centiliters,
        /// <summary>
        /// Milliliters
        /// </summary>
        milliliters,
        /// <summary>
        /// Cubic Inches
        /// </summary>
        cubicInches,
        /// <summary>
        /// Cubic Feet
        /// </summary>
        cubicFeet,
        /// <summary>
        /// Cubic Yard
        /// </summary>
        cubicYards,
        /// <summary>
        /// Liquid Gallons (US)
        /// </summary>
        usLiquidGallons,
        /// <summary>
        /// Dry Gallons (US)
        /// </summary>
        usDryGallons,
        /// <summary>
        /// Liquid Gallons (US)
        /// </summary>
        impLiquidGallons,
        /// <summary>
        /// Barrels of oil
        /// </summary>
        barrels,
        /// <summary>
        /// Cups
        /// </summary>
        cups,
        /// <summary>
        /// Fluid Onces (UK)
        /// </summary>
        fluidOuncesUK,
        /// <summary>
        /// Fluid Onces (US)
        /// </summary>
        fluidOuncesUS,
        /// <summary>
        /// Pints (UK)
        /// </summary>
        pintsUK
    }
    /// <summary>
    /// Unit types to represent Speed
    /// </summary>
    public enum speedTypes
    {
        /// <summary>
        /// Meters/Second
        /// </summary>
        metersPerSecond,
        /// <summary>
        /// Kilometers/Hour
        /// </summary>
        kilometersPerHour,
        /// <summary>
        /// Miles/Hour
        /// </summary>
        milesPerHour,
        /// <summary>
        /// Knots
        /// </summary>
        knots,
        /// <summary>
        /// Mach
        /// </summary>
        Mach
    }
    /// <summary>
    /// Unit types to represent distance
    /// </summary>
    public enum distanceTypes
    {
        /// <summary>
        /// Centimeters
        /// </summary>
        centimeters,
        /// <summary>
        /// Millimeters
        /// </summary>
        millimeters,
        /// <summary>
        /// Inches
        /// </summary>
        inches,
        /// <summary>
        /// Feet
        /// </summary>
        feet,
        /// <summary>
        /// Meters
        /// </summary>
        meters,
        /// <summary>
        /// Kilometers
        /// </summary>
        kilometers,
        /// <summary>
        /// Miles
        /// </summary>
        miles
    }

    /// <summary>
    /// Unit types to represent Fuel Consumption
    /// </summary>
    public enum fuelConsumptionTypes
    {
        /// <summary>
        /// Miles/US Gallon
        /// </summary>
        milesPerGallonUS, 
        /// <summary>
        /// KM/US Gallon
        /// </summary>
        kmPerGallonUS,
        /// <summary>
        /// Miles/UK Gallon
        /// </summary>
        milesPerGallonUK,
        /// <summary>
        /// KM/UK Gallon
        /// </summary>
        kmPerGallonUK,
        /// <summary>
        /// Miles/Liter
        /// </summary>
        milesPerLiter,
        /// <summary>
        /// KM/Liter
        /// </summary>
        kmPerLiter,
        /// <summary>
        /// US Gallons/100 Miles
        /// </summary>
        gallonsUSPer100miles,
        /// <summary>
        /// US Gallons/100 KM
        /// </summary>
        gallonsUSPer100km,
        /// <summary>
        /// UK Gallons/100 Miles
        /// </summary>
        gallonsUKPer100miles,
        /// <summary>
        /// UK Gallons/100 KM
        /// </summary>
        gallonsUKPer100km,
        /// <summary>
        /// Liters/100 Miles
        /// </summary>
        litersPer100miles
    }

    /// <summary>
    /// Math class on steroids
    /// </summary>
    public class Calculation
    {
        Evaluator e = new Evaluator();
        /// <summary>
        /// Converts a string containing a formula to mathmatical operations and returns the result.
        /// </summary>
        /// <param name="formula">The formula to be solved as a string</param>
        /// <returns>Answer when the formula is solved.  Invalid Formula otherwise.</returns>
        public string evaluateFormula(string formula)
        {
            return e.Evaluate(formula);
        }
        /// <summary>
        /// Returns the number of steps required to solve the last formula.  Useful for formula optimizing.
        /// </summary>
        /// <returns></returns>
        public int getFormulaSteps()
        {
            return e.steps;
        }
        /// <summary>
        /// Converts the value in the source unit of measure to a value in the target unit of measure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double convertArea(double value, areaTypes source, areaTypes result)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Converts the value in the source unit of measure to a value in the target unit of measure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double convertVolume(double value, volumeTypes source, volumeTypes result)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Converts the value in the source unit of measure to a value in the target unit of measure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double convertFuelConsumption(double value, fuelConsumptionTypes source, fuelConsumptionTypes result)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Converts the value in the source unit of measure to a value in the target unit of measure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double convertSpeed(double value, speedTypes source, speedTypes target)
        {
            if (source == target)
                return value;
            switch (source)
            {
                case speedTypes.metersPerSecond:
                    break;
                case speedTypes.milesPerHour:
                    value = value * 0.44704;
                    break;
                case speedTypes.kilometersPerHour:
                    value = value / 3.6;
                    break;
                case speedTypes.knots:
                    value = value * 0.514444444;
                    break;
                case speedTypes.Mach:
                    value = value * 340;
                    break;
            }
            switch (target)
            {
                case speedTypes.metersPerSecond:
                    return value;
                case speedTypes.Mach:
                    return value / 340;
                case speedTypes.kilometersPerHour:
                    return value * 3.6;
                case speedTypes.milesPerHour:
                    return value * 2.23693629;
                case speedTypes.knots:
                    return value * 1.943845;
            }
            throw new ArgumentException("Unknown Type");
        }
        /// <summary>
        /// Converts the value in the source unit of measure to a value in the target unit of measure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double convertDistance(double value, distanceTypes source, distanceTypes result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value in the source unit of measure to a value in the target unit of measure.
        /// </summary>
        /// <param name="value">The source value</param>
        /// <param name="source">The source unit of measure</param>
        /// <param name="target">The target unit of measure</param>
        /// <returns></returns>
        public static double convertBytes(double value, byteTypes source, byteTypes target)
        {
            if (source==target)
                return value;
            switch (source)
            {
                case byteTypes.bytes:
                    break;
                case byteTypes.kilobytes:
                    value=value * 1024;
                    break;
                case byteTypes.megabytes:
                    value = value * 1048576;
                    break;
                case byteTypes.gigabytes:
                    value = value * 1073741824;
                    break;
                default:
                    value = value * 1099511627776;
                    break;
            }
            switch(target)
            {
                case byteTypes.bytes:
                    return value;
                case byteTypes.kilobytes:
                    return value/1024;
                case byteTypes.megabytes:
                    return value / 1048576;
                case byteTypes.gigabytes:
                    return value / 1073741824;
                default:
                    return value / 1099511627776;

            }
          }
        /// <summary>
        /// Converts a size in the given unit of measure to the best unit for display.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="source">Type of units</param>
        /// <returns>The converted value using the best unit of measure</returns>
        public static string convertBytes(double value, byteTypes source)
        {
            if (source != byteTypes.bytes)
            {
                value = convertBytes(value, source, byteTypes.bytes);
            }
            if ((value / 1099511627776) >= 1)
                return convertBytes(value, byteTypes.bytes, byteTypes.terabytes).ToString("#.##") + "TB";
            if ((value / 1073741824) >= 1)
                return convertBytes(value, byteTypes.bytes, byteTypes.gigabytes).ToString("#.##") + "GB";
            if ((value / 1048576) >= 1)
                return convertBytes(value, byteTypes.bytes, byteTypes.megabytes).ToString("#.##") + "MB";
            if ((value / 1024) >= 1)
                return convertBytes(value, byteTypes.bytes, byteTypes.kilobytes).ToString("#.##") + "KB";
            return value.ToString("0.##") + "B";
        }
        /// <summary>
        /// Converts centigrade to farenheit. Returns 0 if conversion is invalid.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double CtoF(double degrees)
        {
            try
            {
                return ((9F / 5F) * degrees) + 32;
            }
            catch (Exception) { return 0; }
        }
        /// <summary>
        /// Converts farenheit to centigrade. Returns 0 if conversion is invalid.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double FtoC(double degrees)
        {
            try
            {
                return (5F / 9F) * (degrees - 32);
            }
            catch (Exception) { return 0; }
        }
        /// <summary>
        /// Calculates the sunrise time for the given location
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public static DateTime getSunrise(double longitude,double latitude)
        {
            DateTime sr = DateTime.Now;
            DateTime ss = DateTime.Now;
            SunTimes s = new SunTimes();
            s.CalculateSunRiseSetTimes(latitude, longitude, ref sr, ref ss);
            return sr;
        }
        /// <summary>
        /// Calculates the sunset time for the given location
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public static DateTime getSunset(double longitude, double latitude)
        {
            DateTime sr = DateTime.Now;
            DateTime ss = DateTime.Now;
            SunTimes s = new SunTimes();
            s.CalculateSunRiseSetTimes(latitude, longitude, ref sr, ref ss);
            return ss;
        }
        /// <summary>
        /// Returns a random number between min and max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}