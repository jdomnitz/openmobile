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
            string rank = deck.Draw();
            if (rank == null)
                return null;
            Card c = new Card(25, 110, 100, 130);
            c.OnDrop += new userInteraction(card_OnDrop);
            c.Tag = rank;
            if (isback)
            {
                c.Image = back;
                c.OnClick+=c_OnClick;
            }
            else
            {
                c.Dragable = true;
                c.Image = theHost.getSkinImage("Cards|" + rank);
            }
            return c;
        }

        void  c_OnClick(OMControl sender, int screen)
        {
            if (sender.Parent.controlAtPoint(new Point(sender.Left, sender.Top + sender.Height)) == sender)
            {
                Card c = (Card)sender;
                c.OnClick -= c_OnClick;
                c.Image = theHost.getSkinImage("Cards|" + c.Tag.ToString());
                c.Dragable = true;
            }
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
            spot1.Tag = " H";
            p.addControl(spot1);
            Card spot2 = new Card(505, 110, 100, 130);
            spot2.Image = blank;
            spot2.Tag = " S";
            p.addControl(spot2);
            Card spot3 = new Card(625, 110, 100, 130);
            spot3.Image = blank;
            spot3.Tag = " D";
            p.addControl(spot3);
            Card spot4 = new Card(745, 110, 100, 130);
            spot4.Image = blank;
            spot4.Tag = " C";
            p.addControl(spot4);
            Card pile = new Card(145, 110, 100, 130);
            string first=deck.Draw();
            pile.Tag = first;
            pile.Image = theHost.getSkinImage("Cards|"+first);
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
            OMControl c=sender.Parent.controlAtPoint(new Point(sender.Left-1, sender.Top-1));
            if (c==null)
                c = sender.Parent.controlAtPoint(new Point(sender.Left +sender.Width+1, sender.Top - 1));
            if (c == null)
            {
                if (sender.Tag.ToString()[0] == 'K')
                {
                    Point p=checkSpots(sender,sender.Left, sender.Top);
                    if (p != Point.Empty)
                    {
                        sender.Left = p.X;
                        sender.Top = p.Y;
                        Card source = (Card)sender;
                        if (source.DragParent != null)
                            source.DragParent.DragChild = null;
                        return;
                    }
                }
            }
            if (c!=null)
            {
                string value = c.Tag as string;
                if (value != null)
                {
                    if (sender.Top < 200)
                    {
                        if (checkTop(value, sender.Tag.ToString()))
                        {
                            sender.Left = c.Left;
                            sender.Top = c.Top;
                            Card source = (Card)sender;
                            if (source.DragParent != null)
                                source.DragParent.DragChild = null;
                            return;
                        }
                    }
                    else
                    {
                        if (checkSuit(value, sender.Tag.ToString()))
                        {
                            if (checkValue(value, sender.Tag.ToString()))
                            {
                                sender.Left = c.Left;
                                sender.Top = c.Top + 10;
                                Card source = (Card)sender;
                                if (source.DragParent != null)
                                    source.DragParent.DragChild = null;
                                ((Card)c).DragChild = source;
                                return;
                            }
                        }
                    }
                }
            }
            reset(sender);
        }

        private Point checkSpots(OMControl sender,int x, int y)
        {
            Point p;
            if (x<100)
                p=new Point(25,241);
            else if(x<210)
                p=new Point(145, 241);
            else if (x < 340)
                p=new Point(265, 241);
            else if (x < 470)
                p=new Point(385, 241);
            else if (x < 590)
                p=new Point(505, 241);
            else if (x < 705)
                p=new Point(625, 241);
            else
                p=new Point(745, 241);
            if (sender.Parent.controlAtPoint(p) == null)
                return p;
            else
                return Point.Empty;
        }

        private bool checkTop(string value, string sender)
        {
            bool success=false;
            if (value[0] == ' ')
                return true;
            if (value[0] == 'K')
                return false;
            if (((sender[0] == '2') && (value[0] == 'A')))
                success = true;
            else
                success= ((deck.getValue(value[0]) +1) == deck.getValue(sender[0]));
            if (!success)
                return false;
            return (value[value.Length - 1] == sender[sender.Length - 1]);
        }

        private void reset(OMControl sender)
        {
            Card c = (Card)sender;
            Point p = c.Anchor;
            sender.Left = p.X;
            sender.Top = p.Y;
            if (c.DragChild != null)
                reset(c.DragChild);
        }

        private bool checkValue(string value, string p)
        {
            if (value[0] == 'A')
                return false;
            if (p[0] == 'A')
                return ((value[0] == '2')||(value[0] == ' '));
            return ((deck.getValue(value[0]) - 1) == deck.getValue(p[0]));
        }

        private bool checkSuit(string value, string p)
        {
            char val=p[p.Length-1];
            switch (value[value.Length - 1])
            {
                case 'D':
                case 'H':
                    return (val == 'C' || val == 'S');
                case 'C':
                case 'S':
                    return (val == 'D' || val == 'H');
            }
            return false;
        }

        void draw_OnClick(OMControl sender, int screen)
        {
            OMControl pile = (Card)manager[screen][5];
            Card newCard=genCard(false);
            if (newCard == null)
            {
                ((Card)manager[screen][0]).Image = back;
                restack(pile, true);
            }
            else
            {
                sender.Parent.addControl(newCard);
                Animator.Move(newCard, 145, 110);
                addToPile((Card)pile, newCard);
                if (deck.Count == 0)
                    ((Card)manager[screen][0]).Image = blank;
            }
        }

        private void addToPile(Card pile, Card newCard)
        {
            if (pile.DragChild == null)
                pile.DragChild = newCard;
            else
                addToPile(pile.DragChild, newCard);
        }

        private void restack(OMControl c,bool bottom)
        {
            Card card=(Card)c;
            if (card.DragChild != null)
                restack(card.DragChild,false);
            if (c.Tag!=null)
                deck.putBack(c.Tag.ToString());
            card.DragChild = null;
            if (bottom == true)
            {
                card.Image = blank;
                card.Tag = null;
                card.Dragable = false;
            }
            else
            {
                card.Parent.Remove(card);
            }
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
