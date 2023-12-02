using System.Text.RegularExpressions;

var gamesLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\games.txt"));
var sum = 0;

foreach (var gameLine in gamesLines)
{
    var game = new Game();
    game.ParseLine(gameLine);

    game.FindMinimums();
    sum += game.Power();
}

Console.WriteLine(sum.ToString());

class Hand
{
    public int RedCubesNumber { get; set; }
    public int GreenCubesNumber { get; set; }
    public int BlueCubesNumber { get; set; }

    public bool Validate(int maxRedCubesNumber, int maxGreenCubesNumber, int maxBlueCubesNumber)
    {
        return RedCubesNumber <= maxRedCubesNumber
            && GreenCubesNumber <= maxGreenCubesNumber
            && BlueCubesNumber <= maxBlueCubesNumber;
    }

    public void ParseText(string text)
    {
        var cubesDescriptions = text.Split(',');

        foreach (var cubeDescription in cubesDescriptions)
        {
            var cubeParts = cubeDescription.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var cubeNumber = int.Parse(cubeParts[0]);
            var cubeColorName = cubeParts[1];

            switch (cubeColorName)
            {
                case "red": RedCubesNumber = cubeNumber; break;
                case "green": GreenCubesNumber = cubeNumber; break;
                case "blue": BlueCubesNumber = cubeNumber; break;
                default: throw new Exception("Cannot parse game description.");
            }
        }
    }
}

class Game
{
    public int Index { get; set; }
    public List<Hand> Hands { get; set; }

    public int MinRedCubesNumber { get; set; }
    public int MinGreenCubesNumber { get; set; }
    public int MinBlueCubesNumber { get; set; }

    public int Power()
    {
        return (MinRedCubesNumber > 0 ? MinRedCubesNumber : 1)
            * (MinGreenCubesNumber > 0 ? MinGreenCubesNumber : 1)
            * (MinBlueCubesNumber > 0 ? MinBlueCubesNumber : 1);
    }

    public void FindMinimums()
    {
        MinRedCubesNumber = Hands.Max(hand => hand.RedCubesNumber);
        MinGreenCubesNumber = Hands.Max(hand => hand.GreenCubesNumber);
        MinBlueCubesNumber = Hands.Max(hand => hand.BlueCubesNumber);
    }

    public bool Validate(int maxRedCubesNumber, int maxGreenCubesNumber, int maxBlueCubesNumber)
    {
        return Hands.All(hand => hand.Validate(maxRedCubesNumber, maxGreenCubesNumber, maxBlueCubesNumber));
    }

    public void ParseLine(string line)
    {
        var gameParts = line.Split(':');
        var gameDescription = gameParts[0];
        var handsDescription = gameParts[1];

        Index = int.Parse(Regex.Match(gameDescription, @"\d+", RegexOptions.RightToLeft).Value);

        var handsDescriptions = handsDescription.Split(';');
        Hands = new List<Hand>(handsDescriptions.Length);

        foreach (var handDescription in handsDescriptions)
        {
            var hand = new Hand();
            hand.ParseText(handDescription);
            Hands.Add(hand);
        }
    }
}
