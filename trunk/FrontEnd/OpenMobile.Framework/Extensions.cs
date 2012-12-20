using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace OpenMobile.Framework
{
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
}
