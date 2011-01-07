using System;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Threading;
using System.Threading;

namespace Solitaire
{
    public sealed class Solitaire:IHighLevel
    {
        IPluginHost theHost;
        ScreenManager manager;
        Deck deck;
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen];
        }

        public string displayName
        {
            get { return "Solitaire"; }
        }
        imageItem blank;
        imageItem back;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
            OMPanel p = new OMPanel("");
            p.BackgroundColor1 = Color.FromArgb(0, 75, 0);
            p.BackgroundColor2 = Color.FromArgb(0, 35, 0);
            p.BackgroundType = backgroundStyle.Gradiant;
            blank = theHost.getSkinImage("Cards|blank");
            back = theHost.getSkinImage("Cards|b1fv");
            manager.loadPanel(p);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.TransitionToPanel)
            {
                if (arg2 == "Solitaire")
                    SafeThread.Asynchronous(delegate() { deal(int.Parse(arg1)); }, theHost);
            }
        }
        private Card genCard(bool isback)
        {
            Card c = new Card(25, 110, 100, 130);
            c.OnDrop += new userInteraction(card_OnDrop);
            string rank = deck.Draw();
            c.Tag = rank;
            if (isback)
                c.Image = back;
            else
            {
                c.Dragable = true;
                c.Image = theHost.getSkinImage("Cards|" + rank);
            }
            return c;
        }
        private void deal(int screen)
        {
            OMPanel p = manager[screen];
            p.clear();
            deck = new Deck();
            Card draw = new Card(25, 110, 100, 130);
            draw.Image = back;
            draw.OnClick += new userInteraction(draw_OnClick);
            p.addControl(draw);
            Card spot1 = new Card(385, 110, 100, 130);
            spot1.Image = blank;
            p.addControl(spot1);
            Card spot2 = new Card(505, 110, 100, 130);
            spot2.Image = blank;
            p.addControl(spot2);
            Card spot3 = new Card(625, 110, 100, 130);
            spot3.Image = blank;
            p.addControl(spot3);
            Card spot4 = new Card(745, 110, 100, 130);
            spot4.Image = blank;
            p.addControl(spot4);
            Card pile = new Card(145, 110, 100, 130);
            pile.Image = theHost.getSkinImage("Cards|"+deck.Draw());
            pile.Dragable = true;
            pile.OnDrop += new userInteraction(card_OnDrop);
            p.addControl(pile);
            Card c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 25, 240);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 145, 240);
            c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 145, 250);
            Thread.Sleep(100);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 265, 240);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 265, 250);
            c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 265, 260);
            Thread.Sleep(100);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 385, 240);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 385, 250);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 385, 260);
            c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 385, 270);
            Thread.Sleep(100);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 505, 240);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 505, 250);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 505, 260);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 505, 270);
            c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 505, 280);
            Thread.Sleep(100);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 625, 240);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 625, 250);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 625, 260);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 625, 270);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 625, 280);
            c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 625, 290);
            Thread.Sleep(100);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 745, 240);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 745, 250);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 745, 260);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 745, 270);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 745, 280);
            c = genCard(true);
            p.addControl(c);
            Animator.Move(c, 745, 290);
            c = genCard(false);
            p.addControl(c);
            Animator.Move(c, 745, 300);
            Thread.Sleep(100);
        }

        void card_OnDrop(OMControl sender, int screen)
        {
            Point p = ((Card)sender).Anchor;
            sender.Left = p.X;
            sender.Top = p.Y;
        }

        void draw_OnClick(OMControl sender, int screen)
        {
            throw new NotImplementedException();
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return string.Empty; }
        }

        public string pluginName
        {
            get { return "Solitaire"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Solitaire"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public void Dispose()
        {
            //
        }
    }
}
