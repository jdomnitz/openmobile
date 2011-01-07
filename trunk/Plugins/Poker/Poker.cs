using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile;
using System.Threading;
using System.Collections.Generic;

namespace Poker
{
    public sealed class Poker:IHighLevel
    {
        IPluginHost theHost;
        ScreenManager manager;
        Deck deck = new Deck();
        int state; //todo - screen specific
        int currentBet = 0;
        int total = 1000;
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen,name];
        }

        public string displayName
        {
            get { return "Poker"; }
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
            OMPanel p = new OMPanel("");
            p.BackgroundColor1 = Color.FromArgb(0, 75, 0);
            p.BackgroundColor2 = Color.FromArgb(0, 35, 0);
            p.BackgroundType = backgroundStyle.Gradiant;
            OMBasicShape border = new OMBasicShape(15, 130, 160, 360);
            border.BorderColor = Color.Silver;
            border.BorderSize = 2F;
            border.CornerRadius = 6;
            p.addControl(border);
            OMButton full = new OMButton(25, 400, 130, 75);
            full.Image = theHost.getSkinImage("Full");
            full.FocusImage = theHost.getSkinImage("Full.Highlighted");
            full.Text = "Deal";
            full.OnClick += new userInteraction(full_OnClick);
            p.addControl(full);
            OMLabel amount = new OMLabel(15, 130, 160, 75);
            amount.Text = "Total:\n$1000";
            p.addControl(amount);
            OMLabel bet = new OMLabel(15, 200, 160, 75);
            bet.Text = "Current Bet:\n$0";
            p.addControl(bet);
            OMButton bet10 = new OMButton(30, 270, 60, 60);
            bet10.Image = theHost.getSkinImage("Chips|10");
            bet10.OnClick += new userInteraction(bet10_OnClick);
            p.addControl(bet10);
            OMButton bet25 = new OMButton(105, 270, 60, 60);
            bet25.Image = theHost.getSkinImage("Chips|25");
            bet25.OnClick += new userInteraction(bet25_OnClick);
            p.addControl(bet25);
            OMButton bet100 = new OMButton(65, 330, 60, 60);
            bet100.Image = theHost.getSkinImage("Chips|100");
            bet100.OnClick += new userInteraction(bet100_OnClick);
            p.addControl(bet100);
            Card c2 = new Card(200, 130, 130, 170);
            c2.Image = theHost.getSkinImage("Cards|blank");
            p.addControl(c2);
            Card c3 = new Card(200, 325, 130, 170);
            c3.Image = theHost.getSkinImage("Cards|blank");
            c3.OnClick += new userInteraction(card_OnClick);
            Card c4 = new Card(350, 130, 130, 170);
            c4.Image = theHost.getSkinImage("Cards|blank");
            p.addControl(c4);
            Card c5 = new Card(350, 325, 130, 170);
            c5.Image = theHost.getSkinImage("Cards|blank");
            c5.OnClick += new userInteraction(card_OnClick);
            Card c6 = new Card(500, 130, 130, 170);
            c6.Image = theHost.getSkinImage("Cards|blank");
            p.addControl(c6);
            Card c7 = new Card(500, 325, 130, 170);
            c7.Image = theHost.getSkinImage("Cards|blank");
            c7.OnClick += new userInteraction(card_OnClick);
            Card c8 = new Card(650, 130, 130, 170);
            c8.Image = theHost.getSkinImage("Cards|blank");
            p.addControl(c8);
            Card c9 = new Card(650, 325, 130, 170);
            c9.Image = theHost.getSkinImage("Cards|blank");
            c9.OnClick += new userInteraction(card_OnClick);
            Card c10 = new Card(800, 130, 130, 170);
            c10.Image = theHost.getSkinImage("Cards|blank");
            p.addControl(c10);
            Card c11 = new Card(800, 325, 130, 170);
            c11.Image = theHost.getSkinImage("Cards|blank");
            c11.OnClick += new userInteraction(card_OnClick);
            p.addControl(c3);
            p.addControl(c5);
            p.addControl(c7);
            p.addControl(c9);
            p.addControl(c11);
            manager.loadPanel(p);
            OMPanel winner = new OMPanel("winner");
            winner.BackgroundColor1=Color.FromArgb(140,Color.Black);
            winner.Forgotten=true;
            winner.BackgroundType=backgroundStyle.SolidColor;
            OMLabel label = new OMLabel(0, 0, 1000, 600);
            label.Text = "YOU WIN!";
            label.Font = new Font(Font.GenericSansSerif, 50F);
            winner.addControl(label);
            manager.loadPanel(winner);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }
        private void bet(int amount,int screen)
        {
            if (amount > total)
                return;
            total -= amount;
            currentBet += amount;
            ((OMLabel)manager[screen][3]).Text = "Current Bet:\n$" + currentBet.ToString();
            ((OMLabel)manager[screen][2]).Text = "Total:\n$" + total.ToString();
            if (state == 3)
                ((OMButton)manager[screen][1]).Text = "Raise";
        }
        void bet25_OnClick(OMControl sender, int screen)
        {
            bet(25,screen);
        }

        void bet100_OnClick(OMControl sender, int screen)
        {
            bet(100,screen);
        }

        void bet10_OnClick(OMControl sender, int screen)
        {
            bet(10,screen);
        }

        void full_OnClick(OMControl sender, int screen)
        {
            OMButton btn = (OMButton)sender;
            OMPanel p = manager[screen];
            switch (btn.Text)
            {
                case "Deal":
                    state = 1; 
                    for (int i = 7; i <= 11; i++)
                        ((Card)p[i]).Image = theHost.getSkinImage("Cards|b1fv");
                    for (int i = 12; i <= 16; i++)
                        ((Card)p[i]).Image = theHost.getSkinImage("Cards|" + deck.Draw(), true);
                    btn.Text = "Bet";
                    currentBet = 0;
                    bet(10,screen);
                    break;
                case "Bet":
                    state = 2;
                    btn.Text = "Discard";
                    break;
                case "Done":
                case "Raise":
                    for (int i = 7; i <= 11; i++)
                        ((Card)p[i]).Image = theHost.getSkinImage("Cards|" + deck.Draw(), true);
                    state = 0;
                    btn.Text = "Deal";
                    deck.newDeck();
                    int win= calculateWinner(screen);
                    if (win>0)
                    {
                        ((OMLabel)manager[screen][2]).Text = "Total:\n$" + total.ToString();
                        if (win==1)
                            ((OMLabel)manager[screen, "winner"][0]).Text = "YOU PUSH!";
                        else
                            ((OMLabel)manager[screen, "winner"][0]).Text = "YOU WIN!";
                    }
                    else
                        ((OMLabel)manager[screen, "winner"][0]).Text = "YOU LOSE!";
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Poker", "winner");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    Thread.Sleep(2000);
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "Poker", "winner");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    break;
                case "Discard":
                    for (int i = 12; i <= 16; i ++)
                        if (p[i].Top == 300)
                        {
                            ((Card)p[i]).Image = theHost.getSkinImage("Cards|" + deck.Draw(), true);
                            p[i].Top = 325;
                        }
                    state = 3;
                    btn.Text = "Done";
                    break;
            }
        }

        private int calculateWinner(int screen)
        {
            OMPanel p = manager[screen];
            List<string> dealer=new List<string>();
            for (int i = 7; i <= 11; i++)
                dealer.Add(((Card)p[i]).Image.name.Replace("Cards|",""));
            List<string> you = new List<string>();
            for (int i = 12; i <= 16; i++)
                you.Add(((Card)p[i]).Image.name.Replace("Cards|", ""));
            int deal=deck.calculateHand(dealer);
            int play=deck.calculateHand(you);
            if (deal > play)
                return 0;
            else if (deal == play)
            {
                total += currentBet;
                return 1;
            }
            else
                total += currentBet * 2;
            return 2;
        }

        void card_OnClick(OMControl sender, int screen)
        {
            if (state == 2)
            {
                if (sender.Top == 325)
                    sender.Top = 300;
                else
                    sender.Top = 325;
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
            get { return "Poker"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Poker"; }
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
