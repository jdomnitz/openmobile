using System;
using System.Collections.Generic;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;

namespace MainMenu
{
    internal sealed class MainMenuButton:OMButton
    {
        Font localFont;
        public MainMenuButton(int x,int y)
        {
            top = y;
            left = x;
            width = 184;
            height = 160;
            localFont = new Font(Font.Arial, 24F);
            transition = eButtonTransition.None;
        }
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
            }
        }
        private string ourText;
        public override string Text
        {
            get
            {
                return ourText;
            }
            set
            {
                ourText = value;
                ourTextTex = null;
            }
        }
        OImage ourTextTex;
        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            base.Render(g, e);
            g.DrawImage(icon.image, left + 48, top+18, 92, 92);
            if (ourTextTex == null)
                ourTextTex = g.GenerateTextTexture(0, 0, width, 34, ourText, localFont, eTextFormat.Bold, Alignment.CenterCenter, Color.White, Color.Gray);
            g.DrawImage(ourTextTex, left, top + 112, width, 34);
        }
    }
}
