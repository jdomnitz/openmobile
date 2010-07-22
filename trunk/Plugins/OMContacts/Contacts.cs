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
using System;
using System.Drawing;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Graphics;

namespace OMContacts
{
    public class Contacts:IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            manager = new ScreenManager(host.ScreenCount);
            theHost = host;
            OMList list = new OMList(275, 105, 700, 420);
            list.ListStyle = eListStyle.TransparentImageList;
            list.OnClick += new userInteraction(list_OnClick);
            list.ListItemHeight = 80;
            OMPanel main = new OMPanel("main");
            main.addControl(list);
            manager.loadPanel(main);
            OMPanel p = new OMPanel("contact");
            OMImage border = new OMImage(281, 100, 700, 430);
            border.Image = theHost.getSkinImage("MediaBorder");
            border.Name = "border";
            OMImage picture = new OMImage(351, 137, 125, 150);
            picture.Name = "picture";
            Font f = new Font("Microsoft Sans Serif", 24F);
            OMLabel home = new OMLabel(342, 284, 500, 50);
            home.Font = f;
            home.TextAlignment = Alignment.CenterLeft;
            home.Name = "home";
            OMLabel cell1 = new OMLabel(356, 321, 500, 50);
            cell1.Font = f;
            cell1.TextAlignment = Alignment.CenterLeft;
            cell1.Name = "cell1";
            OMLabel cell2 = new OMLabel(357, 354, 500, 50);
            cell2.Font = f;
            cell2.TextAlignment = Alignment.CenterLeft;
            cell2.Name = "cell2";
            OMLabel work1 = new OMLabel(338, 392, 500, 50);
            work1.Font = f;
            work1.TextAlignment = Alignment.CenterLeft;
            work1.Name = "work1";
            OMLabel work2 = new OMLabel(339, 422, 500, 50);
            work2.Font = f;
            work2.TextAlignment = Alignment.CenterLeft;
            work2.Name = "work2";
            OMLabel email = new OMLabel(352, 455, 600, 50);
            email.Color = Color.Blue;
            email.Font = f;
            email.TextAlignment = Alignment.CenterLeft;
            email.Name = "email";
            OMLabel name = new OMLabel(463, 124, 500, 50);
            name.Font = new Font("Microsoft Sans Serif", 27.75F);
            name.Format = eTextFormat.Outline;
            name.Name = "name";
            OMLabel Address = new OMLabel(563, 204, 300, 80);
            Address.Name = "Address";
            OMLabel birthday = new OMLabel(550,163,400,40);
            birthday.Font = new Font("Microsoft Sans Serif", 21.75F);
            birthday.Name = "birthday";
            birthday.TextAlignment = Alignment.CenterLeft;
            p.addControl(border);
            p.addControl(picture);
            p.addControl(home);
            p.addControl(cell1);
            p.addControl(cell2);
            p.addControl(work1);
            p.addControl(work2);
            p.addControl(email);
            p.addControl(name);
            p.addControl(Address);
            p.addControl(birthday);
            manager.loadPanel(p);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void list_OnClick(OMControl sender, int screen)
        {
            OMList l=((OMList)sender);
            OMPanel p = manager[screen, "contact"];
            ((OMImage)p["picture"]).Image = new imageItem(l[l.SelectedIndex].image);
            ((OMLabel)p["name"]).Text = Collections.contacts[l.SelectedIndex].name;
            ((OMLabel)p["cell1"]).Text = "Cell1: "+Collections.contacts[l.SelectedIndex].cell1;
            ((OMLabel)p["cell2"]).Text = "Cell2: " + Collections.contacts[l.SelectedIndex].cell2;
            ((OMLabel)p["work1"]).Text = "Work1: " + Collections.contacts[l.SelectedIndex].work1;
            ((OMLabel)p["work2"]).Text = "Work2: " + Collections.contacts[l.SelectedIndex].work2;
            ((OMLabel)p["home"]).Text = "Home: "+Collections.contacts[l.SelectedIndex].home;
            ((OMLabel)p["email"]).Text = "Email: "+Collections.contacts[l.SelectedIndex].email;
            ((OMLabel)p["Address"]).Text = Collections.contacts[l.SelectedIndex].Address;
            if (Collections.contacts[l.SelectedIndex].Birthday == DateTime.MinValue)
                ((OMLabel)p["birthday"]).Text = "Birthday: ";
            else
                ((OMLabel)p["birthday"]).Text = "Birthday: "+Collections.contacts[l.SelectedIndex].Birthday.ToShortDateString();

            theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"OMContacts", "contact");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
        }

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            if (name == "contact")
                return manager[screen, "contact"];
            refreshList(screen);
            return manager[screen,"main"];
        }

        private void refreshList(int screen)
        {
            using (OpenMobile.Data.Contacts con = new OpenMobile.Data.Contacts())
            {
                con.beginRead();
                Collections.contacts.Clear();
                OMList l = ((OMList)manager[screen][0]);
                l.Clear();
                contact c=con.readNext(true);
                while (c.name!=null)
                {
                    OImage img;
                    if (c.imageURL=="")
                        img=theHost.getSkinImage("questionMark").image;
                    else
                        img=OImage.FromFile(c.imageURL);
                    l.Add(new OMListItem(c.name,img));
                    c = con.readNext(true);
                }
            }
        }

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }

        #region IBasePlugin Members
        public string displayName
        {
            get { return "Contacts"; }
        }
        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "OMContacts"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Displays contacts from the Database"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Dispose()
        {
            //
        }

    }
}
