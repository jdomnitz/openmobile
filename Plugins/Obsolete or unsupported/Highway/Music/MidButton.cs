using System;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;

namespace Music
{
    public class MidButton:OMButton
    {
        imageItem icon;
        public imageItem Icon
        {
            get
            {
                return icon;
            }
            set
            {
                icon = value;
                raiseUpdate(false);
            }
        }
        /// <summary>
        /// Is the button on the left or right side
        /// </summary>
        public bool LeftAlign;
        public override void Render(OpenMobile.Graphics.Graphics g, OpenMobile.renderingParams e)
        {
            float alpha = 1;
            if (this.Mode == eModeType.transitioningIn)
                alpha = e.globalTransitionIn;
            else if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                alpha = e.globalTransitionOut;
            alpha *= ((float)transparency / 100);
            if (this.Mode == eModeType.Highlighted)
                g.DrawImage(focusImage.image, left, top, width, height, alpha, orientation);
            else if ((this.Mode == eModeType.Clicked) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                g.DrawImage(downImage.image, left, top, width, height, alpha, orientation);
            else
                g.DrawImage(image.image, left, top, width, height, alpha, orientation);
            if (LeftAlign)
            {
                if (textTexture == null)
                    textTexture = g.GenerateTextTexture(0, 0, width - 75, height, text, font, eTextFormat.Bold, Alignment.CenterRight, color, outlineColor);
                g.DrawImage(icon.image, left + 5, top + 10, 75, 75, alpha);
                g.DrawImage(textTexture, left+70, top, width - 80, height, alpha); //overlap a bit
            }
            else
            {
                if (textTexture == null)
                    textTexture = g.GenerateTextTexture(0, 0, width - 80, height, text, font, eTextFormat.Bold, Alignment.CenterLeft, color, outlineColor);
                g.DrawImage(icon.image, left + width-80, top + 10, 75, 75, alpha);
                g.DrawImage(textTexture, left+5, top, width - 80, height, alpha);
            }
        }
        public MidButton(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            font = new Font(Font.Arial, 24F);
        }
    }
}
