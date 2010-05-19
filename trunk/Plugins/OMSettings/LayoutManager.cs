using System;
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile;
using System.Drawing;
using System.Collections.Generic;

namespace OMSettings
{
    sealed class LayoutManager
    {
        IPluginHost theHost;
        OpenMobile.Plugin.Settings collection;
        int ofset = 180;
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
            OMLabel Heading = new OMLabel(300, 80, 600, 100);
            Heading.Font = new Font("Microsoft Sans Serif", 36F);
            Heading.Text = s.Title;
            Heading.Name = "Label";
            ret.addControl(OK);
            ret.addControl(Heading);
            foreach (Setting setting in s)
                foreach (OMControl c in generate(setting,s.Title))
                    ret.addControl(c);
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
                    break;
                case SettingTypes.Text:
                    OMLabel tdesc = new OMLabel(220, ofset, 200, 50);
                    tdesc.Text = s.Description+":";
                    tdesc.Font = new Font("Microsoft Sans Serif", 24F);
                    tdesc.Name = title;
                    tdesc.Tag = s.Name;
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
                case SettingTypes.Password:
                    OMLabel pdesc = new OMLabel(220, ofset, 200, 50);
                    pdesc.Text = s.Description + ":";
                    pdesc.Font = new Font("Microsoft Sans Serif", 24F);
                    pdesc.Name = title;
                    pdesc.Tag = s.Name;
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
            }
            return ret;
        }

        void cursor_OnClick(OMControl sender, int screen)
        {
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMCheckbox)sender).Checked.ToString();
            collection.OnSettingChanged(s);
        }

        void text_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
            ((OMTextBox)sender).Text = input.getText(screen, "OMSettings", sender.Name);
            Setting s = collection.Find(p => p.Name == sender.Tag.ToString());
            s.Value = ((OMTextBox)sender).Text;
            collection.OnSettingChanged(s);
        }
        void Save_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

    }
}
