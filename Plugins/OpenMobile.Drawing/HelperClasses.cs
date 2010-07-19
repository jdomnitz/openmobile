using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OpenMobile.Drawing
{
   
    public class OImage:IDisposable
    {
        Bitmap img;
        public int Texture = 0;
        public OImage(System.Drawing.Bitmap i)
        {
            img = i;
        }
        //public BitmapData LockBits
        //{
        //    get
        //    {
        //        return img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
        //        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    }
        //}
        //public void UnlockBits(BitmapData data)
        //{
        //    img.UnlockBits(data);
        //}
        public Size Size
        {
            get
            {
                if (img == null)
                    return new Size();
                return img.Size;
            }
        }
        //[DefaultValue("")]
        //[TypeConverter(typeof(StringConverter))]
        //[Localizable(false)]
        //[Bindable(true)]
        public object Tag
        {
            get
            {
                if (img == null)
                    return null;
                return img.Tag;
            }
            set
            {
                if (img == null)
                    return;
                img.Tag = value;
            }
        }


        //Need to check all these later---------------------
       
        public int Height()
        {
            return img.Height;
        }

        public int Width()
        {
            return img.Width;
        }

        public OImage Clone()
        {
            return this;
        }

        public void RotateFlip(object o)
        {
        }

        public Bitmap image
        {
            get { return img; }
            set { img = value;}
        }



        public static OImage FromFile(string filename)
        {
            return new OImage(new Bitmap(filename));
        }
        public static OImage FromStream(System.IO.Stream stream)
        {
            return new OImage(new Bitmap(stream));
        }
        public static OImage FromStream(System.IO.Stream stream,bool useEmbeddedColorManagement)
        {
            return new OImage(new Bitmap(stream,useEmbeddedColorManagement));
        }
        public static OImage FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement, bool validateImageData)
        {
            return new OImage(new Bitmap(stream, useEmbeddedColorManagement));            
        }
        ~OImage()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (img == null)
                return;
            if (Texture > 0)
                //Graphics.DeleteTexture(Texture); //Invoke this on the main thread
            img.Dispose();
            img = null;
        }
    }
}
