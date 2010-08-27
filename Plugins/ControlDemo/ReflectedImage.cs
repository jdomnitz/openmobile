using System;
using OpenMobile.Controls;

namespace ControlDemo
{
    class ReflectedImage:OMImage
    {
        public override void Render(OpenMobile.Graphics.Graphics g, OpenMobile.renderingParams e)
        {
            base.Render(g, e);
            g.DrawReflection(left,top+height+(int)(height*0.025),width,height, base.Image.image, 0.9F, 0F);
        }
    }
}
