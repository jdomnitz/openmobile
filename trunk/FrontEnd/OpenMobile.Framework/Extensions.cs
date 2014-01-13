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
using System.Reflection;

namespace OpenMobile
{
    /// <summary>
    /// Provides methods for deep copying objects (Deep cloning)
    /// </summary>
    public static class DeepCopy
    {
        /// <summary>
        /// Deep copies an object using binary serialization
        /// <para>NB! This requires the [System.Serializable] attribute on each object that should be copied</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T DeepCopyBinary<T>(T item)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            formatter.Serialize(stream, item);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            T result = (T)formatter.Deserialize(stream);
            stream.Close();
            return result;
        }

        /// <summary>
        /// Deep copies an object using XML serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopyXML<T>(T obj)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                xs.Serialize(ms, obj);
                ms.Position = 0;

                return (T)xs.Deserialize(ms);
            }
        }

        /// <summary>
        /// Create a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public static T CloneGeneric<T>(T obj)
        {
            T returnData = (T)obj; //.MemberwiseClone();
            Type type = returnData.GetType();

            // Clone fields
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                try
                {
                    //Clone IClonable object
                    if (fieldInfo.FieldType.GetInterface("ICloneable", true) != null)
                    {
                        ICloneable clone = (ICloneable)fieldInfo.GetValue(obj);
                        fieldInfo.SetValue(returnData, (clone != null ? clone.Clone() : clone));
                    }
                    else
                    {
                        fieldInfo.SetValue(returnData, fieldInfo.GetValue(obj));
                    }
                }
                catch (TargetInvocationException) { }
            }

            // Clone properties
            foreach (PropertyInfo propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (propInfo.CanWrite && propInfo.CanRead && (propInfo.GetGetMethod().GetParameters().Length == 0))
                    try
                    {
                        //Clone IClonable object
                        if (propInfo.PropertyType.GetInterface("ICloneable", true) != null)
                        {
                            ICloneable clone = (ICloneable)propInfo.GetValue(obj, null);
                            propInfo.SetValue(returnData, (clone != null ? clone.Clone() : clone), null);
                        }
                        else
                        {
                            propInfo.SetValue(returnData, propInfo.GetValue(obj, null), null);
                        }
                    }
                    catch (TargetInvocationException) { }
            }
            return returnData;
        }

        /// <summary>
        /// Clone a generic list's elements provided they have a clone method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToClone"></param>
        /// <returns></returns>
        public static IList<T> Clone<T>(IList<T> listToClone) where T : ICloneable
        {
            return ((List<T>)listToClone).ConvertAll(x => (T)x.Clone());
        }

    }

    /// <summary>
    /// Extensions for enums
    /// </summary>
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

        /// <summary>
        /// Returns the enums values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }

    /// <summary>
    /// Extensions for collections
    /// </summary>
    public static class Collections
    {
        static readonly Random Random = new Random();

        /// <summary>
        /// Shuffles the contents of a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles the contents of a list, starting at the given index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="startIndex"></param>
        public static void Shuffle<T>(this IList<T> list, int startIndex)
        {
            int n = list.Count;

            // Do we have something to shuffle?
            if (startIndex > n)
                return;

            // Shuffle items
            while (n > startIndex)
            {
                n--;
                int k = Random.Next(startIndex, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


    }

    public static class DateTimeExtensions
    {
        public static bool IsBetween(this DateTime time, DateTime startTime, DateTime endTime)
        {
            if (time.TimeOfDay == startTime.TimeOfDay) return true;
            if (time.TimeOfDay == endTime.TimeOfDay) return true;

            if (startTime.TimeOfDay <= endTime.TimeOfDay)
                return (time.TimeOfDay >= startTime.TimeOfDay && time.TimeOfDay <= endTime.TimeOfDay);
            else
                return !(time.TimeOfDay >= endTime.TimeOfDay && time.TimeOfDay <= startTime.TimeOfDay);
        }
    }
}
