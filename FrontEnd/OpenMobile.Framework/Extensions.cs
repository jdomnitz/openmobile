using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
