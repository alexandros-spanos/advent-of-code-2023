var history = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\history.txt"));
var predictions = new List<int>();

foreach (var line in history)
{
    var sequence = new SensorSequence();
    sequence.ParseLine(line);
    predictions.Add(sequence.FindPrediction());
}

Console.WriteLine(predictions.Sum());

class SensorSequence
{
    public int[] Sequence { get; private set; }

    public void ParseLine(string line)
    {
        Sequence = line
            .Split(' ', StringSplitOptions.TrimEntries)
            .ToList().ConvertAll(int.Parse).ToArray();
    }

    public int FindPrediction()
    {
        var sequences = new List<int[]>();
        var differences = Sequence;

        while (!IsBottomSequence(differences))
        {
            differences = NextSequence(differences);
            sequences.Add(differences);
        }

        var prediction = 0;
        var elements = sequences.Select(s => s.First()).ToList();
        elements.Insert(0, Sequence.First());

        for (var index = elements.Count - 1; index >= 0; index--)
        {
            prediction = elements[index] - prediction;
        }

        return prediction;
    }

    private static bool IsBottomSequence(int[] differences)
    {
        var difference = differences[0];
        return differences.All(diff => diff == difference);
    }

    private static int[] NextSequence(int[] differences)
    {
        var nextDifferencesLength = differences.Length - 1;
        var nextDifferences = new int[nextDifferencesLength];

        for (var index = 0; index < nextDifferencesLength; index++)
        {
            nextDifferences[index] = differences[index + 1] - differences[index];
        }

        return nextDifferences;
    }
}
