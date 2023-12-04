using System.Text.RegularExpressions;

var scratchcardsLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\scratchcards.txt"));
var sum = 0;

foreach (var scratchcardLine in scratchcardsLines)
{
    var card = new Card();
    card.ParseLine(scratchcardLine);

    card.NumberWinnings();
    sum += card.PointsNumber;
}

Console.WriteLine(sum.ToString());

class Card
{
    public int Index { get; private set; }
    public List<int> Winning { get; private set; }
    public List<int> Own { get; private set; }
    public int PointsNumber { get; private set; }

    public void NumberWinnings()
    {
        var winningsNumber = 0;

        foreach (var winningNumber in Winning)
        {
            if (Own.Contains(winningNumber))
            {
                winningsNumber++;
            }
        }

        if (winningsNumber > 0)
        {
            if (winningsNumber == 1)
            {
                PointsNumber = 1;
            }
            else
            {
                PointsNumber = (int)Math.Pow(2, winningsNumber - 1);
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
