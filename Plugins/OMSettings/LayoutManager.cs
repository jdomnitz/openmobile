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
using System.Collections.Generic;
using OpenMobile.Graphics;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace OMSettings
{
    sealed class LayoutManager
    {
        IPluginHost theHost;
        OpenMobile.Plugin.Settings collection;
        int ofset=87;
        internal OMPanel[] layout(IPluginHost host, OpenMobile.Plugin.Settings s)
        {
            if (s == null)
                return null;
            theHost = host;
            collection = s;
            collection.OnSettingChanged += new SettingChanged(collection_OnSettingChanged);
            OMButton OK = new OMButton(13, 56, 200, 110);
            OK.Image = theHost.getSkinImage("Full");
            OK.FocusImage = theHost.getSkinImage("Full.Highlighted");
            OK.Text = "OK";
            OK.Name = "OMSettings.OK";
            OK.OnClick += new userInteraction(Save_OnClick);
            OK.Transition = eButtonTransition.None;
            OMLabel Heading = new OMLabel(200, 0, 800, 100);
            Heading.Font = new Font(Font.GenericSansSerif, 36F);
            Heading.Text = s.Title;
            Heading.Name = "Label";
            OMPanel[] ret=new OMPanel[theHost.ScreenCount];
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                ret[i]= new OMPanel(s.Title);
                controls.Add(new List<OMControl>());
                OMScrollingContainer container = new OMScrollingContainer(0, 80, 1000, 450);
                foreach (Setting setting in s)
                    foreach (OMControl c in generate(setting, s.Title,i))
                        container.Add(c);
                Rectangle area = container.ControlArea;
                area.Height = ofset;
                container.ControlArea = area;
                container.ScrollBarWidth = 10;
                container.Add(OK); //Always on top
                container.Add(Heading);
                ret[i].addControl(container);
            }
            return ret;
        }

        void collection_OnSettingChanged(int screen,Setting setting)
        {
            OMControl c = controls[screen].Find(s => ((s.Tag != null) && (s.Tag.ToString() == setting.Name)));
            if (typeof(OMCheckbox).IsInstanceOfType(c))
                ((OMCheckbox)c).Checked = bool.Parse(setting.getInstanceValue(screen));
            else if (typeof(OMTextBox).IsInstanceOfType(c))
            {
                if (setting.Type == SettingTypes.MultiChoice)
                {
                    string query = setting.getInstanceValue(screen);
                    ((OMTextBox)c).Text = setting.Options[setting.Values.FindIndex(p => p == query)];
                }
                else
                    ((OMTextBox)c).Text = setting.getInstanceValue(screen);
            }
            else if (typeof(OMSlider).IsInstanceOfType(c))
            {
                ((OMSlider)c).Value = int.Parse(setting.getInstanceValue(screen));
                OMLabel lbl = ((OMScrollingContainer)c.Parent[0])["dsc" + setting.Name] as OMLabel;
                if (lbl != null)
                    lbl.Text = setting.Description.Replace("%value%", setting.getInstanceValue(screen));
            }
        }
        List<List<OMControl>> controls = new List<List<OMControl>>();
        List<OMControl> generate(Setting s,string title,int screen)
        {
            List<OMControl> ret = new List<OMControl>();
            switch (s.Type)
            {
                case SettingTypes.MultiChoice:
                    if (s.Values == Setting.BooleanList)
                    {
                        OMCheckbox cursor = new OMCheckbox(220, ofset, 700, 50);
                        cursor.Text = s.Description;
                        cursor.Font = new Font(Font.GenericSansSerif, 24F);
                        cursor.OutlineColor = Color.Red;
                        cursor.Name = title;
                        cursor.Tag = s.Name;
                        cursor.OnClick += new userInteraction(cursor_OnClick);
                        if (s.getInstanceValue(screen) == "True")
                            cursor.Checked = true;
                        ofset += 70;
                        ret.Add(cursor); controls[screen].Add(cursor);
                    }
                    else if((s.Values.Count>=s.Options.Count)&&(s.Options.Count>0))
                    {
                        OMButton scrollRight = new OMButton(918, ofset, 70, 60);
                        scrollRight.Image = theHost.getSkinImage("Play");
                        scrollRight.DownImage = theHost.getSkinImage("Play.Highlighted");
                        scrollRight.OnClick += new userInteraction(scrollRight_OnClick);
                        scrollRight.Tag = "btn" + s.Name;
                        scrollRight.Transition = eButtonTransition.None;
                        OMButton scrollLeft = new OMButton(370, ofset, 70, 60);
                        scrollLeft.Image = scrollRight.Image;
                        scrollLeft.DownImage = scrollRight.DownImage;
                        scrollLeft.Orientation = eAngle.FlipHorizontal;
                        scrollLeft.OnClick += new userInteraction(scrollLeft_OnClick);
                        scrollLeft.Tag = "btn" + s.Name;
                        scrollLeft.Transition = eButtonTransition.None;
                        OMTextBox txtchoice = new OMTextBox(449, ofset+5, 460, 50);
                        txtchoice.Flags = textboxFlags.EllipsisEnd;
                        int index = s.Values.FindIndex(def => def == s.getInstanceValue(screen));
                        if (index != -1)
                            txtchoice.Text = s.Options[index];
                        txtchoice.Font = new Font(Font.GenericSansSerif, 24F);
                        txtchoice.Tag = txtchoice.Name = s.Name;
                        OMLabel mcTitle = new OMLabel((ofset > 170 ? 100 : 220), ofset, (ofset > 170 ? 270 : 150), 50);
                        mcTitle.Font = new Font(Font.GenericSansSerif, 32.25F, FontStyle.Bold);
                        mcTitle.Text = s.Header + ((s.Header!=null)?":":"");
                        mcTitle.TextAlignment = Alignment.CenterRight;
                        if ((s.Description != null) && (s.Description.Length > 0))
                        {
                            OMLabel mcDescription = new OMLabel(450, ofset+57, 460, 30);
                            mcDescription.Font = new Font(Font.GenericSansSerif, 20);
                            mcDescription.Text = s.Description;
                            ret.Add(mcDescription);
                            ofset += 30;
                        }
                        ofset += 65;
                        ret.Add(mcTitle);
                        controls[screen].Add(mcTitle);
                        ret.Add(txtchoice);
                        controls[screen].Add(txtchoice);
                        ret.Add(scrollLeft);
                        controls[screen].Add(scrollLeft);
                        ret.Add(scrollRight);
                        controls[screen].Add(scrollRight);
                    }
                    break;
                case SettingTypes.Text:
                    OMLabel tdesc = new OMLabel((ofset > 170 ? 100 : 220), ofset, (ofset > 170 ? 300 : 180), 50);
                    tdesc.Text = (s.Header == "" ? s.Description + ":" : s.Header + ":");
                    tdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    tdesc.Name = title;
                    tdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(tdesc);
                    controls[screen].Add(tdesc);
                    OMTextBox text = new OMTextBox(450, ofset, 500, 50);
                    text.Font = new Font(Font.GenericSansSerif, 28F);
                    text.TextAlignment = Alignment.CenterLeft;
                    text.OnClick += new userInteraction(text_OnClick);
                    text.Name = title;
                    text.Text = s.getInstanceValue(screen);
                    text.Tag = s.Name;
                    ret.Add(text);
                    controls[screen].Add(text);
                    if (s.Header != "")
                    {
                        if ((s.Description != null) && (s.Description.Length > 0))
                        {
                            OMLabel mcDescription = new OMLabel(text.Left, ofset + 50, text.Width, 30);
                            mcDescription.Font = new Font(Font.GenericSansSerif, 20);
                            mcDescription.Text = s.Description;
                            ret.Add(mcDescription);
                            ofset += 30;
                        }
                    }
                    ofset += 60;
                    break;
                case SettingTypes.Folder:
                    OMLabel fdesc = new OMLabel(220, ofset, 200, 50);
                    fdesc.Text = s.Description + ":";
                    fdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    fdesc.Name = title;
                    fdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(fdesc);
                    controls[screen].Add(fdesc);
                    OMTextBox folder = new OMTextBox(450, ofset, 500, 50);
                    folder.Font = new Font(Font.GenericSansSerif, 28F);
                    folder.TextAlignment = Alignment.CenterLeft;
					folder.Flags=textboxFlags.EllipsisCenter;
                    folder.OnClick += new userInteraction(folder_OnClick);
                    folder.Name = title;
                    folder.Text = s.getInstanceValue(screen);
                    folder.Tag = s.Name;
                    ret.Add(folder);
                    controls[screen].Add(folder);
                    ofset += 60;
                    break;
                case SettingTypes.File:
                    OMLabel fldesc = new OMLabel(220, ofset, 200, 50);
                    fldesc.Text = s.Description + ":";
                    fldesc.Font = new Font(Font.GenericSansSerif, 24F);
                    fldesc.Name = title;
                    fldesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(fldesc);
                    controls[screen].Add(fldesc);
                    OMTextBox file = new OMTextBox(450, ofset, 500, 50);
                    file.Font = new Font(Font.GenericSansSerif, 28F);
                    file.TextAlignment = Alignment.CenterLeft;
                    file.OnClick += new userInteraction(file_OnClick);
                    file.Name = title;
                    file.Text = s.getInstanceValue(screen);
					file.Flags=textboxFlags.EllipsisCenter;
                    file.Tag = s.Name;
                    ret.Add(file);
                    controls[screen].Add(file);
                    ofset += 60;
                    break;
                case SettingTypes.Password:
                    OMLabel pdesc = new OMLabel((ofset > 170 ? 100 : 220), ofset, (ofset > 170 ? 300 : 180), 50);
                    pdesc.Text = s.Description + ":";
                    pdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    pdesc.Name = title;
                    pdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(pdesc);
                    controls[screen].Add(pdesc);
                    OMTextBox ptext = new OMTextBox(450, ofset, 500, 50);
                    ptext.Font = new Font(Font.GenericSansSerif, 28F);
                    ptext.TextAlignment = Alignment.CenterLeft;
                    ptext.OnClick += new userInteraction(text_OnClick);
                    ptext.Text = s.getInstanceValue(screen);
                    ptext.Name = title;
                    ptext.Tag = s.Name;
                    ptext.Flags = textboxFlags.Password;
                    ret.Add(ptext);
                    controls[screen].Add(ptext);
                    ofset += 60;
                    break;
                case SettingTypes.Range:
                    OMLabel rdesc = new OMLabel(220, ofset, 200, 50);
                    rdesc.Text = s.Header + ":";
                    rdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    rdesc.Name = title;
                    rdesc.TextAlignment = Alignment.CenterRight;
                    if (s.Values.Count != 2)
                        break;
                    OMSlider range = new OMSlider(450, ofset+15, 500, 30,12,20);
                    range.Slider = theHost.getSkinImage("Slider");
                    range.SliderBar = theHost.getSkinImage("Slider.Bar");
                    int val;
                    if (!int.TryParse(s.Values[0], out val))
                        break;
                    range.Minimum = val;
                    if (!int.TryParse(s.Values[1], out val))
                        break;
                    range.Maximum = val;
                    if ((s.getInstanceValue(screen) == string.Empty))
                        val = 0;
                    else if (!int.TryParse(s.getInstanceValue(screen), out val))
                        break;
                    range.Value = val;
                    range.Tag = s.Name;
                    range.OnSliderMoved += new OMSlider.slidermoved(range_OnSliderMoved);
                    if ((s.Description != null) && (s.Description.Length > 0))
                    {
                        OMLabel rDescription = new OMLabel(450, ofset + 35, 500, 30);
                        rDescription.Font = new Font(Font.GenericSansSerif, 20);
                        rDescription.Text = s.Description.Replace("%value%", s.getInstanceValue(screen));
                        rDescription.Name = "dsc" + s.Name;
                        ret.Add(rDescription);
                        ofset += 30;
                    }
                    ret.Add(range);
                    ret.Add(rdesc);
                    controls[screen].Add(range);
                    controls[screen].Add(rdesc);
                    ofset += 60;
                    break;
                case SettingTypes.Button:
                    OMButton button = new OMButton(220, ofset, 600, 50);
                    button.Text = s.Description;
                    button.Font = new Font(Font.GenericSansSerif, 24F);
                    button.Width=(int)(Graphics.MeasureString(button.Text, button.Font).Width+0.5F)+20;
                    button.Name = title;
                    button.Tag = s.Name;
                    button.OnClick += new userInteraction(button_OnClick);
                    button.Image = theHost.getSkinImage("Full");
                    button.FocusImage = theHost.getSkinImage("Full.Highlighted");
                    ofset += 70;
                    ret.Add(button);
                    controls[screen].Add(button);
                    break;
            }
            return ret;
        }

        void button_OnClick(OMControl sender, int screen)
        {
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            collection.changeSetting(screen,s);
        }

        void range_OnSliderMoved(OMSlider sender, int screen)
        {
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.setInstanceValue(screen,((OMSlider)sender).Value.ToString());
            collection.changeSetting(screen,s);
        }

        void file_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath path = new OpenMobile.helperFunctions.General.getFilePath(theHost);
            ((OMTextBox)sender).Text = path.getFile(screen, "OMSettings", sender.Name);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.setInstanceValue(screen, ((OMTextBox)sender).Text);
            collection.changeSetting(screen,s);
        }

        void folder_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath path = new OpenMobile.helperFunctions.General.getFilePath(theHost);
            ((OMTextBox)sender).Text = path.getFolder(screen, "OMSettings", sender.Name);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.setInstanceValue(screen,((OMTextBox)sender).Text);
            collection.changeSetting(screen,s);
        }

        void cursor_OnClick(OMControl sender, int screen)
        {
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.setInstanceValue(screen, ((OMCheckbox)sender).Checked.ToString());
            collection.changeSetting(screen,s);
        }

        void text_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput();
            OMTextBox tb = ((OMTextBox)sender);
            if ((tb.Flags&textboxFlags.Password)==textboxFlags.Password)
                tb.Text = input.getPassword(screen, "OMSettings", sender.Parent.Name,tb.Text);
            else
                tb.Text = input.getText(screen, "OMSettings", sender.Parent.Name,tb.Text);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.setInstanceValue(screen, tb.Text);
            collection.changeSetting(screen,s);
        }
        void Save_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }
        void scrollRight_OnClick(OMControl sender, int screen)
        {
            Setting setting=collection.Find(s => s.Name == sender.Tag.ToString().Substring(3));
            OMTextBox tb = (OMTextBox)((OMScrollingContainer)sender.Parent[0])[setting.Name];
            int index = setting.Options.FindIndex(s => s == tb.Text);
            if (index >= setting.Options.Count - 1)
                return;
            tb.Text = setting.Options[index+1];
            setting.setInstanceValue(screen, setting.Values[index+1]);
            collection.changeSetting(screen,setting);
        }
        void scrollLeft_OnClick(OMControl sender, int screen)
        {
            Setting setting = collection.Find(s => s.Name == sender.Tag.ToString().Substring(3));
            OMTextBox tb = (OMTextBox)((OMScrollingContainer)sender.Parent[0])[setting.Name];
            int index = setting.Options.FindIndex(s => s == tb.Text);
            if (index <= 0)
                return;
            tb.Text = setting.Options[index - 1];
            setting.setInstanceValue(screen,setting.Values[index - 1]);
            collection.changeSetting(screen,setting);
        }
    }
}
