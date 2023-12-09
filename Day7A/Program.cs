var camelCards = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\camel-cards.txt")).ToList();

var hands = camelCards
    .ConvertAll(card => card.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .ConvertAll(tuple => new Hand(tuple[0], int.Parse(tuple[1])));

hands.Sort(new HandComparer());
Console.WriteLine(hands.Select((hand, index) => (index + 1) * hand.Bid).Aggregate((a, b) => a + b));

class Hand
{
    public string Cards { get; private set; }
    public int Bid { get; private set; }

    public Hand(string cards, int bid)
    {
        Cards = cards;
        Bid = bid;
    }
}

class HandComparer : IComparer<Hand>
{
    private readonly char[] Cards = ['A', 'K', 'Q', 'J', 'T', '9', '8', '7', '6', '5', '4', '3', '2'];

    public int Compare(Hand a, Hand b)
    {
        var x = a.Cards;
        var y = b.Cards;

        var handTypeX = GetHandType(x);
        var handTypeY = GetHandType(y);

        if (handTypeX == handTypeY)
        {
            return CompareHands(x, y);
        }

        return handTypeX.CompareTo(handTypeY);
    }

    private static int GetHandType(string hand)
    {
        var groups = hand.GroupBy(c => c).ToList();

        // Five of a kind
        if (groups.Count == 1)
        {
            return 6;
        }
        // Four of a kind
        else if (groups.Count == 2 && groups.Any(group => group.Count() == 1))
        {
            return 5;
        }
        // Full house
        else if (groups.Count == 2 && groups.Any(group => group.Count() == 2))
        {
            return 4;
        }
        // Three of a kind
        else if (groups.Count == 3 && groups.Any(group => group.Count() == 3))
        {
            return 3;
        }
        // Two pair
        else if (groups.Count == 3 && groups.Any(group => group.Count() == 2))
        {
            return 2;
        }
        // One pair
        else if (groups.Count == 4)
        {
            return 1;
        }
        // High card
        else
        {
            return 0;
        }
    }

    private int CompareHands(string x, string y)
    {
        for (int i = 0; i < 5; i++)
        {
            int indexX = Array.IndexOf(Cards, x[i]);
            int indexY = Array.IndexOf(Cards, y[i]);

            var comparison = indexY.CompareTo(indexX);

            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }
}
