using System.Collections.Generic;
using System;
public sealed class Deck
{
    public List<string> cards = new List<string>();
    public Deck()
    {
        newDeck();
    }

    public void newDeck()
    {
        cards.Clear();
        string[] suit = new string[] { "C", "S", "H", "D" };
        string[] value = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
        for (int i = 0; i < suit.Length; i++)
            for (int j = 0; j < value.Length; j++)
                cards.Add(value[j] + suit[i]);
        shuffle();
    }
    public string Draw()
    {
        int index = cards.Count - 1;
        if (index == -1)
            return null;
        string ret = cards[index];
        cards.RemoveAt(index);
        return ret;
    }
    public void shuffle()
    {
        Random rng = new Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }
    public int getValue(char val)
    {
        switch (val)
        {
            case ' ':
                return 1;
            case '2':
                return 2;
            case '3':
                return 3;
            case '4':
                return 4;
            case '5':
                return 5;
            case '6':
                return 6;
            case '7':
                return 7;
            case '8':
                return 8;
            case '9':
                return 9;
            case '1':
                return 10;
            case 'J':
                return 11;
            case 'Q':
                return 12;
            case 'K':
                return 13;
            default:
                return 14;
        }
    }

    public int calculateHand(List<string> hand)
    {
        hand.Sort();
        bool straight = true;
        for (int i = 1; i < 5; i++)
        {
            if (getValue(hand[i][0]) != getValue(hand[i - 1][0]) - 1)
                straight = false;
        }
        bool flush = true;
        char flushChar = '0';
        int[] count = new int[15];
        for (int i = 0; i < 5; i++)
        {
            if (flushChar == '0')
                flushChar = hand[i][hand[i].Length - 1];
            else if (flushChar != hand[i][hand[i].Length - 1])
                flush = false;
            count[getValue(hand[i][0])]++;
        }
        int highCard = getValue(hand[4][0]);
        if (hand.Exists(p => p[0] == 'A'))
            highCard = 14;
        else if (hand.Exists(p => p[0] == 'K'))
            highCard = 13;
        else if ((highCard < 10) && (hand.Exists(p => p[0] == '1')))
            highCard = 10;

        if (straight & flush)
            return 800 + highCard;
        else if (Array.Exists(count, p => p == 4))
            return 700 + highCard;
        else if (Array.Exists(count, p => (p == 3)) && Array.Exists(count, p => (p == 2)))
            return 600 + highCard;
        else if (flush)
            return 500 + highCard;
        else if (straight)
            return 400 + highCard;
        else if (Array.Exists(count, p => (p == 3)))
            return 300 + highCard;
        else if (countPairs(count) == 2)
            return 200 + highCard;
        else if (Array.Exists(count, p => (p == 2)))
            return 100 + highCard;
        return highCard;
    }
    private int countPairs(int[] matches)
    {
        int count = 0;
        for (int i = 0; i < matches.Length; i++)
            if (matches[i] == 2)
                count++;
        return count;
    }

    public void putBack(string p)
    {
        cards.Add(p);
    }
    public int Count
    {
        get
        {
            return cards.Count;
        }
    }
}