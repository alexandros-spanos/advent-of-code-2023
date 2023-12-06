var races = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\races-records.txt"));

Console.WriteLine(GetNumberOfWays(ParseLine(races[0]), ParseLine(races[1])).ToString());

long GetNumberOfWays(long time, long distance)
{
    var D = Math.Sqrt(Math.Pow(time, 2) - 4 * distance);
    var r1 = (long)Math.Ceiling((time + D) / 2);
    var r2 = (long)Math.Floor((time - D) / 2);
    return r1 - r2 - 1;
}

static long ParseLine(string line)
{
    return long.Parse(string.Join("", line.Split(':')[1]
        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)));
}
