var patternLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\patterns.txt"));
var sum = 0;
var lineIndex = 0;
var patterns = new List<Pattern>();
var patternLinesCollection = new List<string>();

while (lineIndex < patternLines.Length)
{
    var patternLine = patternLines[lineIndex];
    lineIndex++;
    var isLastLine = lineIndex == patternLines.Length;

    if (string.IsNullOrWhiteSpace(patternLine) || isLastLine)
    {
        if (isLastLine)
        {
            patternLinesCollection.Add(patternLine);
        }
        if (patternLinesCollection.Count > 0)
        {
            var pattern = new Pattern();
            pattern.ParseLines(patternLinesCollection.ToArray());
            patternLinesCollection.Clear();
            patterns.Add(pattern);
        }
    }
    else
    {
        patternLinesCollection.Add(patternLine);
    }
}

foreach (var pattern in patterns)
{
    sum += pattern.Measure();
}

Console.WriteLine(sum);

class Pattern
{
    public int Heigth { get; set; }
    public int Width { get; set; }
    public char[,] Pixels { get; private set; }

    public void ParseLines(string[] lines)
    {
        Heigth = lines.Length;
        Width = lines[0].Length;
        Pixels = new char[Heigth, Width];

        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            var line = lines[rowIndex];

            for (var columnIndex = 0; columnIndex < Width; columnIndex++)
            {
                Pixels[rowIndex, columnIndex] = line[columnIndex];
            }
        }
    }

    public int Measure()
    {
        for (var rowIndex = 0; rowIndex < Heigth - 1; rowIndex++)
        {
            if (IsHorizontalReflection(rowIndex))
            {
                return (rowIndex + 1) * 100;
            }
        }

        for (var columnIndex = 0; columnIndex < Width - 1; columnIndex++)
        {
            if (IsVerticalReflection(columnIndex))
            {
                return columnIndex + 1;
            }
        }

        return 0;
    }

    private bool IsHorizontalReflection(int upperRowIndex)
    {
        var distance = 0;
        var maxDistance = Math.Min(upperRowIndex + 1, Heigth - upperRowIndex - 1);

        while (distance < maxDistance)
        {
            var upperRow = SliceRow(upperRowIndex - distance);
            distance++;
            var lowerRow = SliceRow(upperRowIndex + distance);

            if (!upperRow.SequenceEqual(lowerRow)) return false;
        }
        return true;
    }

    private bool IsVerticalReflection(int leftColumnIndex)
    {
        var distance = 0;
        var maxDistance = Math.Min(leftColumnIndex + 1, Width - leftColumnIndex - 1);

        while (distance < maxDistance)
        {
            var leftColumn = SliceColumn(leftColumnIndex - distance);
            distance++;
            var rightColumn = SliceColumn(leftColumnIndex + distance);

            if (!leftColumn.SequenceEqual(rightColumn)) return false;
        }
        return true;
    }

    private IEnumerable<char> SliceRow(int rowIndex)
    {
        for (var columnIndex = 0; columnIndex < Width; columnIndex++)
        {
            yield return Pixels[rowIndex, columnIndex];
        }
    }

    private IEnumerable<char> SliceColumn(int columnIndex)
    {
        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            yield return Pixels[rowIndex, columnIndex];
        }
    }
}
