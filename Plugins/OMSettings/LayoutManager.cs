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
using System.Collections.Generic;
using OpenMobile.Drawing;
using System.Drawing;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace OMSettings
{
    sealed class LayoutManager
    {
        IPluginHost theHost;
        OpenMobile.Plugin.Settings collection;
        int ofset = 165;
        internal OMPanel layout(IPluginHost host, OpenMobile.Plugin.Settings s)
        {
            if (s == null)
                return null;
            theHost = host;
            collection = s;
            OMPanel ret = new OMPanel(s.Title);
            OMButton OK = new OMButton(13, 136, 200, 110);
            OK.Image = theHost.getSkinImage("Full");
            OK.FocusImage = theHost.getSkinImage("Full.Highlighted");
            OK.Text = "OK";
            OK.Name = "OMSettings.OK";
            OK.OnClick += new userInteraction(Save_OnClick);
            OK.Transition = eButtonTransition.None;
            OMLabel Heading = new OMLabel(200, 80, 800, 100);
            Heading.Font = new Font("Microsoft Sans Serif", 36F);
            Heading.Text = s.Title;
            Heading.Name = "Label";
            foreach (Setting setting in s)
                foreach (OMControl c in generate(setting,s.Title))
                    ret.addControl(c);
            ret.addControl(OK); //Always on top
            ret.addControl(Heading);
            return ret;
        }
        List<OMControl> generate(Setting s,string title)
        {
            List<OMControl> ret = new List<OMControl>();
            switch (s.Type)
            {
                case SettingTypes.MultiChoice:
                    if (s.Values == Setting.BooleanList)
                    {
                        OMCheckbox cursor = new OMCheckbox(220, ofset, 600, 50);
                        cursor.Text = s.Description;
                        cursor.Font = new Font("Microsoft Sans Serif", 24F);
                        cursor.OutlineColor = Color.Red;
                        cursor.Name = title;
                        cursor.Tag = s.Name;
                        cursor.OnClick += new userInteraction(cursor_OnClick);
                        if (s.Value == "True")
                            cursor.Checked = true;
                        ofset += 70;
                        ret.Add(cursor);
                    }
                    else if((s.Values.Count>=s.Options.Count)&&(s.Options.Count>0))
                    {
                        OMButton scrollRight = new OMButton(920, ofset, 70, 60);
                        scrollRight.Image = theHost.getSkinImage("Play");
                        scrollRight.DownImage = theHost.getSkinImage("Play.Highlighted");
                        scrollRight.OnClick += new userInteraction(scrollRight_OnClick);
                        scrollRight.Tag = s.Name;
                        scrollRight.Transition = eButtonTransition.None;
                        OMButton scrollLeft = new OMButton(370, ofset, 70, 60);
                        scrollLeft.Image = scrollRight.Image;
                        scrollLeft.DownImage = scrollRight.DownImage;
                        scrollLeft.Orientation = eAngle.FlipHorizontal;
                        scrollLeft.OnClick += new userInteraction(scrollLeft_OnClick);
                        scrollLeft.Tag = s.Name;
                        scrollLeft.Transition = eButtonTransition.None;
                        OMTextBox txtchoice = new OMTextBox(450, ofset+5, 460, 50);
                        txtchoice.Flags = textboxFlags.EllipsisEnd;
                        int index = s.Values.FindIndex(def => def == s.Value);
                        if (index != -1)
                            txtchoice.Text = s.Options[index];
                        txtchoice.Font = new Font("Microsoft Sans Serif", 24F);
                        txtchoice.Name = "txt"+s.Name;
                        OMLabel mcTitle = new OMLabel(100, ofset, 270, 50);
                        mcTitle.Font = new Font("Microsoft Sans Serif", 32.25F, FontStyle.Bold);
                        mcTitle.Text = s.Header + ((s.Header!=null)?":":"");
                        mcTitle.TextAlignment = Alignment.CenterRight;
                        if ((s.Description != null) && (s.Description.Length > 0))
                        {
                            OMLabel mcDescription = new OMLabel(450, ofset+57, 460, 30);
                            mcDescription.Font = new Font("Microsoft Sans Serif", 20);
                            mcDescription.Text = s.Description;
                            ret.Add(mcDescription);
                            ofset += 30;
                        }
                        ofset += 65;
                        ret.Add(mcTitle);
                        ret.Add(txtchoice);
                        ret.Add(scrollLeft);
                        ret.Add(scrollRight);
                    }
                    break;
                case SettingTypes.Text:
                    OMLabel tdesc = new OMLabel(220, ofset, 200, 50);
                    tdesc.Text = s.Description+":";
                    tdesc.Font = new Font("Microsoft Sans Serif", 24F);
                    tdesc.Name = title;
                    tdesc.Tag = s.Name;
                    tdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(tdesc);
                    OMTextBox text = new OMTextBox(450, ofset, 500, 50);
                    text.Font = new Font("Microsoft Sans Serif", 28F);
                    text.TextAlignment = Alignment.CenterLeft;
                    text.OnClick += new userInteraction(text_OnClick);
                    text.Name = title;
                    text.Text = s.Value;
                    text.Tag = s.Name;
                    ret.Add(text);
                    ofset += 60;
                    break;
                case SettingTypes.Folder:
                    OMLabel fdesc = new OMLabel(220, ofset, 200, 50);
                    fdesc.Text = s.Description + ":";
                    fdesc.Font = new Font("Microsoft Sans Serif", 24F);
                    fdesc.Name = title;
                    fdesc.Tag = s.Name;
                    fdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(fdesc);
                    OMTextBox folder = new OMTextBox(450, ofset, 500, 50);
                    folder.Font = new Font("Microsoft Sans Serif", 28F);
                    folder.TextAlignment = Alignment.CenterLeft;
                    folder.OnClick += new userInteraction(folder_OnClick);
                    folder.Name = title;
                    folder.Text = s.Value;
                    folder.Tag = s.Name;
                    ret.Add(folder);
                    ofset += 60;
                    break;
                case SettingTypes.File:
                    OMLabel fldesc = new OMLabel(220, ofset, 200, 50);
                    fldesc.Text = s.Description + ":";
                    fldesc.Font = new Font("Microsoft Sans Serif", 24F);
                    fldesc.Name = title;
                    fldesc.Tag = s.Name;
                    fldesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(fldesc);
                    OMTextBox file = new OMTextBox(450, ofset, 500, 50);
                    file.Font = new Font("Microsoft Sans Serif", 28F);
                    file.TextAlignment = Alignment.CenterLeft;
                    file.OnClick += new userInteraction(file_OnClick);
                    file.Name = title;
                    file.Text = s.Value;
                    file.Tag = s.Name;
                    ret.Add(file);
                    ofset += 60;
                    break;
                case SettingTypes.Password:
                    OMLabel pdesc = new OMLabel(220, ofset, 200, 50);
                    pdesc.Text = s.Description + ":";
                    pdesc.Font = new Font("Microsoft Sans Serif", 24F);
                    pdesc.Name = title;
                    pdesc.Tag = s.Name;
                    pdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(pdesc);
                    OMTextBox ptext = new OMTextBox(450, ofset, 500, 50);
                    ptext.Font = new Font("Microsoft Sans Serif", 28F);
                    ptext.TextAlignment = Alignment.CenterLeft;
                    ptext.OnClick += new userInteraction(text_OnClick);
                    ptext.Text = s.Value;
                    ptext.Name = title;
                    ptext.Tag = s.Name;
                    ptext.Flags = textboxFlags.Password;
                    ret.Add(ptext);
                    ofset += 60;
                    break;
                case SettingTypes.Range:
                    OMLabel rdesc = new OMLabel(220, ofset, 200, 50);
                    rdesc.Text = s.Header + ":";
                    rdesc.Font = new Font("Microsoft Sans Serif", 24F);
                    rdesc.Name = title;
                    rdesc.Tag = s.Name;
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
                    if (!int.TryParse(s.Value, out val))
                        break;
                    range.Value = val;
                    range.Tag = s.Name;
                    range.OnSliderMoved += new OMSlider.slidermoved(range_OnSliderMoved);
                    if ((s.Description != null) && (s.Description.Length > 0))
                    {
                        OMLabel rDescription = new OMLabel(450, ofset + 35, 500, 30);
                        rDescription.Font = new Font("Microsoft Sans Serif", 20);
                        rDescription.Tag= s.Description;
                        rDescription.Text = s.Description.Replace("%value%", s.Value);
                        rDescription.Name = "dsc" + s.Name;
                        ret.Add(rDescription);
                        ofset += 30;
                    }
                    ret.Add(range);
                    ret.Add(rdesc);
                    ofset += 60;
                    break;
            }
            return ret;
        }

        void range_OnSliderMoved(OMSlider sender, int screen)
        {
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMSlider)sender).Value.ToString();
            OMLabel lbl = (OMLabel)sender.Parent["dsc" + s.Name];
            if (lbl!=null)
                lbl.Text = lbl.Tag.ToString().Replace("%value%", s.Value);
            collection.changeSetting(s);
        }

        void file_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath path = new OpenMobile.helperFunctions.General.getFilePath(theHost);
            ((OMTextBox)sender).Text = path.getFile(screen, "OMSettings", sender.Name);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMTextBox)sender).Text;
            collection.changeSetting(s);
        }

        void folder_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath path = new OpenMobile.helperFunctions.General.getFilePath(theHost);
            ((OMTextBox)sender).Text = path.getFolder(screen, "OMSettings", sender.Name);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMTextBox)sender).Text;
            collection.changeSetting(s);
        }

        void cursor_OnClick(OMControl sender, int screen)
        {
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMCheckbox)sender).Checked.ToString();
            collection.changeSetting(s);
        }

        void text_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
            ((OMTextBox)sender).Text = input.getText(screen, "OMSettings", sender.Name);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMTextBox)sender).Text;
            collection.changeSetting(s);
        }
        void Save_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }
        void scrollRight_OnClick(OMControl sender, int screen)
        {
            Setting setting=collection.Find(s => s.Name == sender.Tag.ToString());
            OMTextBox tb = (OMTextBox)sender.Parent["txt" + setting.Name];
            int index = setting.Options.FindIndex(s => s == tb.Text);
            if (index >= setting.Options.Count - 1)
                return;
            tb.Text = setting.Options[index+1];
            setting.Value = setting.Values[index+1];
            collection.changeSetting(setting);
        }
        void scrollLeft_OnClick(OMControl sender, int screen)
        {
            Setting setting = collection.Find(s => s.Name == sender.Tag.ToString());
            OMTextBox tb = (OMTextBox)sender.Parent["txt" + setting.Name];
            int index = setting.Options.FindIndex(s => s == tb.Text);
            if (index <= 0)
                return;
            tb.Text = setting.Options[index - 1];
            setting.Value = setting.Values[index - 1];
            collection.changeSetting(setting);
        }
    }
}
