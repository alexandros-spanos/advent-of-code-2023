var races = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\races-records.txt"));
var times = ParseLine(races[0]);
var distances = ParseLine(races[1]);
var numberOfWays = new List<int>();

for (var index = 0; index < times.Count; index++)
{
    numberOfWays.Add(GetNumberOfWays(times[index], distances[index]));
}

Console.WriteLine(numberOfWays.Aggregate((a, b) => a * b).ToString());

int GetNumberOfWays(int time, int distance)
{
    var D = Math.Sqrt(Math.Pow(time, 2) - 4 * distance);
    var r1 = (int)Math.Ceiling((time + D) / 2);
    var r2 = (int)Math.Floor((time - D) / 2);
    return r1 - r2 - 1;
}

static List<int> ParseLine(string line)
{
    return line.Split(':')[1]
        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList().ConvertAll(int.Parse);
}
