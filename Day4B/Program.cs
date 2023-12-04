using System.Text.RegularExpressions;

var scratchcardsLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\scratchcards.txt"));
var cards = new List<Card>();

foreach (var scratchcardLine in scratchcardsLines)
{
    var card = new Card();
    card.ParseLine(scratchcardLine);
    card.NumberWinnings();
    cards.Add(card);
}

for (var counter = 0; counter < cards.Count; counter++)
{
    var card = cards[counter];

    if (card.WinningsNumber > 0)
    {
        for (var index = counter + 1; index < Math.Min(counter + card.WinningsNumber + 1, cards.Count); index++)
        {
            cards[index].IncreaseMultiplicity(card.Multiplicity);
        }
    }
}

Console.WriteLine(cards.Select(card => card.Multiplicity).Sum().ToString());

class Card
{
    public int Index { get; private set; }
    public List<int> Winning { get; private set; }
    public List<int> Own { get; private set; }
    public int WinningsNumber { get; private set; }
    public int Multiplicity { get; private set; } = 1;

    public void IncreaseMultiplicity(int increment)
    {
        Multiplicity += increment;
    }

    public void NumberWinnings()
    {
        foreach (var winningNumber in Winning)
        {
            if (Own.Contains(winningNumber))
            {
                WinningsNumber++;
            }
        }
    }

    public void ParseLine(string line)
    {
        var cardParts = line.Split(':');
        var cardDescription = cardParts[0];
        var numbersDescription = cardParts[1];

        Index = int.Parse(Regex.Match(cardDescription, @"\d+", RegexOptions.RightToLeft).Value);

        var numbersParts = numbersDescription.Split('|');

        Winning = ParseNumbers(numbersParts[0]);
        Own = ParseNumbers(numbersParts[1]);
    }

    private List<int> ParseNumbers(string linePart)
    {
        return linePart
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList().ConvertAll(int.Parse);
    }
}
