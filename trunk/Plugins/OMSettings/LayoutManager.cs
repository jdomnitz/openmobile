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
using OpenMobile.Graphics;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions.Controls;

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
            OMPanel[] ret=new OMPanel[theHost.ScreenCount];
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                ofset = 87;
                ret[i]= new OMPanel(s.Title);
                controls.Add(new List<OMControl>());
                OMContainer container = new OMContainer("SettingsContainer", 0, 100, 1000, 500);
                ret[i].addControl(container);

                //OMButton OK = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("OMSettings.OK", 13, 56, 200, 110, "", "OK");
                //OK.OnClick += new userInteraction(Save_OnClick);
                //OK.Transition = eButtonTransition.None;
                OMLabel Heading = new OMLabel(200, 0, 800, 50);
                Heading.Font = new Font(Font.GenericSansSerif, 36F);
                Heading.Text = s.Title;
                Heading.Name = "Label";
                
                //container.addControlRelative(OK); //Always on top
                container.addControlRelative(Heading);
                foreach (Setting setting in s)
                    foreach (OMControl c in generate(setting, s.Title,i))
                        container.addControlRelative(c);
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
                OMLabel lbl = ((OMContainer)c.Parent[0])["dsc" + setting.Name][0] as OMLabel;
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
                        OMCheckbox cursor = new OMCheckbox(330, ofset, 620, 50);
                        cursor.Text = s.Description;
                        cursor.Font = new Font(Font.GenericSansSerif, 24F);
                        cursor.OutlineColor = Color.Red;
                        cursor.Name = title;
                        cursor.Tag = s.Name;
                        cursor.OnClick += new userInteraction(cursor_OnClick);
                        if (s.getInstanceValue(screen) == "True")
                            cursor.Checked = true;
                        OMLabel mcTitle = new OMLabel(20, ofset, 300, 50);
                        mcTitle.Font = new Font(Font.GenericSansSerif, 24F, FontStyle.Regular);
                        mcTitle.Text = s.Header + (!string.IsNullOrEmpty(s.Header) ? ":" : "");
                        mcTitle.TextAlignment = Alignment.CenterRight;
                        ret.Add(mcTitle);
                        controls[screen].Add(mcTitle);
                        ret.Add(cursor); 
                        controls[screen].Add(cursor);
                        ofset += 70;
                    }
                    else if((s.Values.Count>=s.Options.Count)&&(s.Options.Count>0))
                    {
                        OMButton scrollRight = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("", 910, ofset, 70, 60, "", ">");
                        scrollRight.OnClick += new userInteraction(scrollRight_OnClick);
                        scrollRight.Tag = "btn" + s.Name;
                        scrollRight.Transition = eButtonTransition.None;
                        OMButton scrollLeft = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("", 330, ofset, 70, 60, "", "<");
                        scrollLeft.OnClick += new userInteraction(scrollLeft_OnClick);
                        scrollLeft.Tag = "btn" + s.Name;
                        scrollLeft.Transition = eButtonTransition.None;
                        OMTextBox txtchoice = new OMTextBox(395, ofset+5, 520, 50);
                        txtchoice.Flags = textboxFlags.EllipsisEnd;
                        int index = s.Values.FindIndex(def => def == s.getInstanceValue(screen));
                        if (index != -1)
                            txtchoice.Text = s.Options[index];
                        txtchoice.Font = new Font(Font.GenericSansSerif, 24F);
                        txtchoice.Tag = txtchoice.Name = s.Name;
                        OMLabel mcTitle = new OMLabel(20, ofset, 300, 50);
                        mcTitle.Font = new Font(Font.GenericSansSerif, 24F, FontStyle.Regular);
                        mcTitle.Text = s.Header + (!string.IsNullOrEmpty(s.Header) ? ":" : "");
                        mcTitle.TextAlignment = Alignment.CenterRight;
                        if ((s.Description != null) && (s.Description.Length > 0))
                        {
                            OMLabel mcDescription = new OMLabel(330, ofset+57, 640, 30);
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
                    OMLabel tdesc = new OMLabel(20, ofset, 300, 50);
                    tdesc.Text = (s.Header == "" ? s.Description + ":" : s.Header + ":");
                    tdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    tdesc.Name = title;
                    tdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(tdesc);
                    controls[screen].Add(tdesc);
                    OMTextBox text = new OMTextBox(330, ofset, 650, 50);
                    text.Font = new Font(Font.GenericSansSerif, 28F);
                    text.TextAlignment = Alignment.CenterLeft;
                    text.OSKDescription = (s.Header == "" ? s.Description : s.Header);
                    text.OSKType = OSKInputTypes.Keypad;
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
                    OMLabel fdesc = new OMLabel(20, ofset, 300, 50);
                    fdesc.Text = s.Description + ":";
                    fdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    fdesc.Name = title;
                    fdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(fdesc);
                    controls[screen].Add(fdesc);
                    OMTextBox folder = new OMTextBox(330, ofset, 650, 50);
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
                    OMLabel fldesc = new OMLabel(20, ofset, 300, 50);
                    fldesc.Text = s.Description + ":";
                    fldesc.Font = new Font(Font.GenericSansSerif, 24F);
                    fldesc.Name = title;
                    fldesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(fldesc);
                    controls[screen].Add(fldesc);
                    OMTextBox file = new OMTextBox(330, ofset, 650, 50);
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
                    OMLabel pdesc = new OMLabel(20, ofset, 300, 50);
                    pdesc.Text = s.Description + ":";
                    pdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    pdesc.Name = title;
                    pdesc.TextAlignment = Alignment.CenterRight;
                    ret.Add(pdesc);
                    controls[screen].Add(pdesc);
                    OMTextBox ptext = new OMTextBox(330, ofset, 650, 50);
                    ptext.Font = new Font(Font.GenericSansSerif, 28F);
                    ptext.TextAlignment = Alignment.CenterLeft;
                    ptext.OSKDescription = "Enter password";
                    ptext.OSKHelpText = "password";
                    ptext.OSKType = OSKInputTypes.Keypad;
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
                    OMLabel rdesc = new OMLabel(20, ofset, 300, 50);
                    rdesc.Text = s.Header + ":";
                    rdesc.Font = new Font(Font.GenericSansSerif, 24F);
                    rdesc.Name = title;
                    rdesc.TextAlignment = Alignment.CenterRight;
                    if (s.Values.Count != 2)
                        break;
                    OMSlider range = new OMSlider(330, ofset+15, 650, 30,12,20);
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
                        OMLabel rDescription = new OMLabel(330, ofset + 35, 650, 30);
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
                    {
                        OMButton button = new OMButton(330, ofset, 650, 50);
                        button.Text = s.Description;
                        button.Font = new Font(Font.GenericSansSerif, 24F);
                        button.Width = (int)(Graphics.MeasureString(button.Text, button.Font).Width + 0.5F) + 20;
                        button = DefaultControls.GetButton(title, button.Left, button.Top, button.Width, button.Height, "", s.Description);
                        //button.Name = title;
                        button.Tag = s.Name;
                        button.OnClick += new userInteraction(button_OnClick);
                        //button.Image = theHost.getSkinImage("Full");
                        //button.FocusImage = theHost.getSkinImage("Full.Highlighted");
                        OMLabel mcTitle = new OMLabel(20, ofset, 250, 50);
                        mcTitle.Font = new Font(Font.GenericSansSerif, 24F, FontStyle.Regular);
                        mcTitle.Text = s.Header + (!string.IsNullOrEmpty(s.Header) ? ":" : "");
                        mcTitle.TextAlignment = Alignment.CenterRight;

                        ret.Add(mcTitle);
                        controls[screen].Add(mcTitle);
                        ret.Add(button);
                        controls[screen].Add(button);

                        ofset += 70;
                    }
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
            OMTextBox tb = ((OMTextBox)sender);
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
            OMTextBox tb = (OMTextBox)((OMContainer)sender.Parent[0])[setting.Name][0];
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
            OMTextBox tb = (OMTextBox)((OMContainer)sender.Parent[0])[setting.Name][0];
            int index = setting.Options.FindIndex(s => s == tb.Text);
            if (index <= 0)
                return;
            tb.Text = setting.Options[index - 1];
            setting.setInstanceValue(screen,setting.Values[index - 1]);
            collection.changeSetting(screen,setting);
        }
    }
}
