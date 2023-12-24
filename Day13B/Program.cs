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

    private bool _hasHorizontalInitialReflection;
    private int _initialReflectionIndex;

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
        MeasureInitial();
        return MeasureSmudge();
    }

    private void MeasureInitial()
    {
        for (var rowIndex = 0; rowIndex < Heigth - 1; rowIndex++)
        {
            if (IsHorizontalReflection(rowIndex))
            {
                _hasHorizontalInitialReflection = true;
                _initialReflectionIndex = rowIndex;
                return;
            }
        }

        for (var columnIndex = 0; columnIndex < Width - 1; columnIndex++)
        {
            if (IsVerticalReflection(columnIndex))
            {
                _initialReflectionIndex = columnIndex;
                return;
            }
        }
    }

    private int MeasureSmudge()
    {
        for (var rowIndex = 0; rowIndex < Heigth - 1; rowIndex++)
        {
            if (!(_hasHorizontalInitialReflection
                && _initialReflectionIndex == rowIndex)
                && IsSmudgedHorizontalReflection(rowIndex))
            {
                return (rowIndex + 1) * 100;
            }
        }

        for (var columnIndex = 0; columnIndex < Width - 1; columnIndex++)
        {
            if (!(!_hasHorizontalInitialReflection
                && _initialReflectionIndex == columnIndex)
                && IsSmudgedVerticalReflection(columnIndex))
            {
                return columnIndex + 1;
            }
        }

        return 0;
    }

    private bool IsSmudgedHorizontalReflection(int upperRowIndex)
    {
        return IsSmudgedReflection(upperRowIndex, Heigth, SliceRow);
    }

    private bool IsSmudgedVerticalReflection(int leftColumnIndex)
    {
        return IsSmudgedReflection(leftColumnIndex, Width, SliceColumn);
    }

    private bool IsHorizontalReflection(int upperRowIndex)
    {
        return IsReflection(upperRowIndex, Heigth, SliceRow);
    }

    private bool IsVerticalReflection(int leftColumnIndex)
    {
        return IsReflection(leftColumnIndex, Width, SliceColumn);
    }

    private static bool IsSmudgedReflection(int leftColumnIndex, int maxDimention, Func<int, IEnumerable<char>> sliceMethod)
    {
        var hasFoundSmudge = false;
        var distance = 0;
        var maxDistance = Math.Min(leftColumnIndex + 1, maxDimention - leftColumnIndex - 1);

        while (distance < maxDistance)
        {
            var leftColumn = sliceMethod(leftColumnIndex - distance);
            distance++;
            var rightColumn = sliceMethod(leftColumnIndex + distance);

            if (hasFoundSmudge)
            {
                if (!leftColumn.SequenceEqual(rightColumn)) return false;
            }
            else
            {
                var differencesNumber = SequencesSimilarity(leftColumn, rightColumn);

                if (differencesNumber == 1)
                {
                    hasFoundSmudge = true;
                }
                else if (differencesNumber > 1)
                {
                    return false;
                }
            }
        }

        return hasFoundSmudge;
    }

    private static bool IsReflection(int sequenceIndex, int maxDimention, Func<int, IEnumerable<char>> sliceMethod)
    {
        var distance = 0;
        var maxDistance = Math.Min(sequenceIndex + 1, maxDimention - sequenceIndex - 1);

        while (distance < maxDistance)
        {
            var firstSequence = sliceMethod(sequenceIndex - distance);
            distance++;
            var secondSequence = sliceMethod(sequenceIndex + distance);

            if (!firstSequence.SequenceEqual(secondSequence)) return false;
        }
        return true;
    }

    private static int SequencesSimilarity(IEnumerable<char> s1, IEnumerable<char> s2)
    {
        var differencesNumber = 0;
        var enumerator1 = s1.GetEnumerator();
        var enumerator2 = s2.GetEnumerator();

        while (enumerator1.MoveNext() && enumerator2.MoveNext())
        {
            if (enumerator1.Current != enumerator2.Current)
            {
                differencesNumber++;
            }

            if (differencesNumber == 2)
            {
                return differencesNumber;
            }
        }

        return differencesNumber;
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
