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
using OpenMobile;
using OpenMobile.Controls;
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
