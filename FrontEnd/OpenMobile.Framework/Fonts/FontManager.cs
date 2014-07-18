using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;

namespace OpenMobile.Fonts
{
    public class FontManager : IDisposable
    {
        /// <summary>
        /// Loaded fonts
        /// </summary>
        public static PrivateFontCollection Fonts
        {
            get
            {
                return _Fonts;
            }
        }
        private static PrivateFontCollection _Fonts;

        /// <summary>
        /// The century font family
        /// </summary>
        public static FontFamily Century
        {
            get
            {
                return _Century;
            }
        }
        private static FontFamily _Century;

        ///// <summary>
        ///// Comment
        ///// </summary>
        //public static OpenMobile.Graphics.Font CenturyGothic
        //{
        //    get
        //    {
        //        return new Graphics.Font(;
        //    }
        //}
        //private static OpenMobile.Graphics.Font _CenturyGothic;
        
        public FontManager()
        {
            // Load fonts
            //_Century = LoadFontFamily("Fonts\\GOTHIC.TTF", out _Fonts);
            //_CenturyGothic = new Font(_Century, 18f);
        }

        public static FontFamily LoadFontFamily(string fileName, out PrivateFontCollection fontCollection)
        {
            fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fileName);
            return fontCollection.Families[0];
        }

        public void Dispose()
        {
            //_Fonts.Dispose();
        }
    }
}
