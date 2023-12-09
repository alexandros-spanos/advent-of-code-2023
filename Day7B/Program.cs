var camelCards = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\camel-cards.txt")).ToList();

var hands = camelCards
    .ConvertAll(card => card.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .ConvertAll(tuple => new Hand(tuple[0], int.Parse(tuple[1])));

hands.Sort(new HandComparer());
Console.WriteLine(hands.Select((hand, index) => (index + 1) * hand.Bid).Aggregate((a, b) => a + b));

class Hand
{
    public static readonly char[] Figures = ['A', 'K', 'Q', 'T', '9', '8', '7', '6', '5', '4', '3', '2', 'J'];

    private static readonly Dictionary<int, List<char[]>> Wildcards = [];

    public char[] Cards { get; private set; }
    public int Bid { get; private set; }
    public int Kind { get; private set; } = -1;

    public Hand(string cards, int bid)
    {
        Cards = cards.ToCharArray();
        Bid = bid;
    }

    public int GetHandType()
    {
        if (Kind != -1)
        {
            return Kind;
        }

        var kind = -1;
        var otherCards = Cards.Where(c => c != 'J').ToArray();
        var jokersCount = 5 - otherCards.Length;

        if (jokersCount > 0)
        {
            var maximumKind = -1;
            var combinations = GetCombinations(jokersCount);

            for (var index = 0; index < combinations.Count; index++)
            {
                var combination = combinations[index];
                var cards = new char[5];

                Array.Copy(combination, cards, jokersCount);
                Array.Copy(otherCards, 0, cards, jokersCount, otherCards.Length);

                var kindInner = GetHandTypeInner(cards);

                if (maximumKind < 0 || kindInner > maximumKind)
                {
                    maximumKind = kindInner;
                }
            }

            kind = maximumKind;
        }
        else
        {
            kind = GetHandTypeInner(Cards);
        }

        Kind = kind;
        return Kind;
    }

    public static int GetHandTypeInner(char[] cards)
    {
        var groups = cards.GroupBy(c => c).ToList();

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

    private static List<char[]> GetCombinations(int cardCount)
    {
        if (Wildcards.TryGetValue(cardCount, out var cache))
        {
            return cache;
        }

        var list = new List<char[]>();

        if (cardCount == 1)
        {
            list.AddRange(Figures.ToList().ConvertAll(c => new char[] { c }));
        }
        else
        {
            var lesserCardsCombinations = GetCombinations(cardCount - 1);

            for (var lesserCardIndex = 0; lesserCardIndex < lesserCardsCombinations.Count; lesserCardIndex++)
            {
                var lesserCards = lesserCardsCombinations[lesserCardIndex];

                for (var cardIndex = 0; cardIndex < Figures.Length; cardIndex++)
                {
                    var combination = new char[cardCount];

                    Array.Copy(lesserCards, combination, cardCount - 1);
                    combination[cardCount - 1] = Figures[cardIndex];
                    list.Add(combination);
                }
            }
        }

        Wildcards.Add(cardCount, list);

        return list;
    }
}

class HandComparer : IComparer<Hand>
{
    public int Compare(Hand a, Hand b)
    {
        var handTypeX = a.GetHandType();
        var handTypeY = b.GetHandType();

        if (handTypeX == handTypeY)
        {
            return CompareHands(a.Cards, b.Cards);
        }

        return handTypeX.CompareTo(handTypeY);
    }

    private static int CompareHands(char[] x, char[] y)
    {
        for (int i = 0; i < 5; i++)
        {
            var indexX = Array.IndexOf(Hand.Figures, x[i]);
            var indexY = Array.IndexOf(Hand.Figures, y[i]);
            var comparison = indexY.CompareTo(indexX);

            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }
}
