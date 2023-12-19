var platformLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\platform.txt"));

var platform = new Platform();
platform.ParseLines(platformLines);
var sum = platform.Solve();

Console.WriteLine(sum);

class Platform
{
    private const int NumberOfSpinCycles = 1000000000;

    public int Heigth { get; private set; }
    public int Width { get; private set; }
    public char[,] Positions { get; private set; }

    private List<List<Tuple<int, int>>> _roundRocksPositions;

    public void ParseLines(string[] lines)
    {
        Heigth = lines.Length;
        Width = lines[0].Length;
        Positions = new char[Heigth, Width];

        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            var line = lines[rowIndex];

            for (var columnIndex = 0; columnIndex < Width; columnIndex++)
            {
                Positions[rowIndex, columnIndex] = line[columnIndex];
            }
        }
    }

    public int Solve()
    {
        int sameSpinIndex = -1, index = 1;
        _roundRocksPositions = [];
        _roundRocksPositions.Add(GatherRoundRocksPositions());

        //Console.WriteLine("initial setup:");
        //PrintAndWait();

        for (; index < NumberOfSpinCycles; index++)
        {
            //Console.WriteLine($"spin {index}:");
            Spin();
            _roundRocksPositions.Add(GatherRoundRocksPositions());
            sameSpinIndex = PreviousPositionsSameAsCurrent();
            if (sameSpinIndex != -1) break;
        }

        if (sameSpinIndex != -1)
        {
            var period = index - sameSpinIndex;
            var cycling = NumberOfSpinCycles - sameSpinIndex;
            var remainder = cycling % period;

            //Console.WriteLine($"performimg remainder spins with period: {period}, cycling: {cycling}, remainder: {remainder}:");
            for (index = 1; index <= remainder; index++)
            {
                //Console.WriteLine($"spin {index}:");
                Spin();
            }
        }

        return Measure();
    }

    private void PrintAndWait()
    {
        PrintPlatform();
        //Console.ReadKey();
    }

    private void PrintPlatform()
    {
        Console.WriteLine();
        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < Width; columnIndex++)
            {
                Console.Write(Positions[rowIndex, columnIndex]);
            }
            Console.WriteLine();
        }
    }

    private int PreviousPositionsSameAsCurrent()
    {
        var lastPositions = _roundRocksPositions[^1];

        for (var spinIndex = 0; spinIndex < _roundRocksPositions.Count - 1; spinIndex++)
        {
            var positions = _roundRocksPositions[spinIndex];

            if (positions.All(p => lastPositions.Any(l => l.Item1 == p.Item1 && l.Item2 == p.Item2)))
            {
                return spinIndex;
            }
        }

        return -1;
    }

    private List<Tuple<int, int>> GatherRoundRocksPositions()
    {
        var roundRocksPositions = new List<Tuple<int, int>>();

        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            roundRocksPositions.AddRange(GatherRoundRocksOnRow(rowIndex));
        }

        return roundRocksPositions;
    }

    private List<Tuple<int, int>> GatherRoundRocksOnRow(int rowIndex)
    {
        var result = new List<Tuple<int, int>>();

        for (var columnIndex = 0; columnIndex < Width; columnIndex++)
        {
            if (Positions[rowIndex, columnIndex] == 'O')
            {
                result.Add(new Tuple<int, int>(rowIndex, columnIndex));
            }
        }

        return result;
    }

    private int Measure()
    {
        var result = 0;

        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            result += MeasureRow(rowIndex);
        }

        return result;
    }

    private int MeasureRow(int rowIndex)
    {
        var count = 0;

        for (var columnIndex = 0; columnIndex < Width; columnIndex++)
        {
            if (Positions[rowIndex, columnIndex] == 'O')
            {
                count++;
            }
        }

        return count * (Heigth - rowIndex);
    }

    private void Spin()
    {
        FallNorth();
        //PrintAndWait();
        FallWest();
        //PrintAndWait();
        FallSouth();
        //PrintAndWait();
        FallEast();
        //PrintAndWait();
    }

    private void FallNorth()
    {
        for (var columnIndex = 0; columnIndex < Width; columnIndex++)
        {
            LetColumnFallNorth(columnIndex);
        }
    }

    private void FallWest()
    {
        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            LetRowFallWest(rowIndex);
        }
    }

    private void FallSouth()
    {
        for (var columnIndex = 0; columnIndex < Width; columnIndex++)
        {
            LetColumnFallSouth(columnIndex);
        }
    }

    private void FallEast()
    {
        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            LetColumnFallEast(rowIndex);
        }
    }

    private void LetColumnFallNorth(int columnIndex)
    {
        for (var rowIndex = 1; rowIndex < Heigth; rowIndex++)
        {
            var positionElement = Positions[rowIndex, columnIndex];

            if (positionElement == 'O')
            {
                LetRoundRockFallNorth(rowIndex, columnIndex);
            }
        }
    }

    private void LetRowFallWest(int rowIndex)
    {
        for (var columnIndex = 1; columnIndex < Width; columnIndex++)
        {
            var positionElement = Positions[rowIndex, columnIndex];

            if (positionElement == 'O')
            {
                LetRoundRockFallWest(rowIndex, columnIndex);
            }
        }
    }

    private void LetColumnFallSouth(int columnIndex)
    {
        for (var rowIndex = Heigth - 2; rowIndex >= 0; rowIndex--)
        {
            var positionElement = Positions[rowIndex, columnIndex];

            if (positionElement == 'O')
            {
                LetRoundRockFallSouth(rowIndex, columnIndex);
            }
        }
    }

    private void LetColumnFallEast(int rowIndex)
    {
        for (var columnIndex = Width - 2; columnIndex >= 0; columnIndex--)
        {
            var positionElement = Positions[rowIndex, columnIndex];

            if (positionElement == 'O')
            {
                LetRoundRockFallEast(rowIndex, columnIndex);
            }
        }
    }

    private void LetRoundRockFallNorth(int rowIndex, int columnIndex)
    {
        var stableRowIndex = rowIndex;
        var cursorRowIndex = rowIndex - 1;

        while (cursorRowIndex >= 0)
        {
            var cursorElement = Positions[cursorRowIndex, columnIndex];

            if (cursorElement == 'O' || cursorElement == '#')
            {
                break;
            }
            else
            {
                stableRowIndex = cursorRowIndex;
                cursorRowIndex--;
            }
        }

        if (stableRowIndex != rowIndex)
        {
            Positions[rowIndex, columnIndex] = '.';
            Positions[stableRowIndex, columnIndex] = 'O';
        }
    }

    private void LetRoundRockFallWest(int rowIndex, int columnIndex)
    {
        var stableColumnIndex = columnIndex;
        var cursorColumnIndex = columnIndex - 1;

        while (cursorColumnIndex >= 0)
        {
            var cursorElement = Positions[rowIndex, cursorColumnIndex];

            if (cursorElement == 'O' || cursorElement == '#')
            {
                break;
            }
            else
            {
                stableColumnIndex = cursorColumnIndex;
                cursorColumnIndex--;
            }
        }

        if (stableColumnIndex != columnIndex)
        {
            Positions[rowIndex, columnIndex] = '.';
            Positions[rowIndex, stableColumnIndex] = 'O';
        }
    }

    private void LetRoundRockFallSouth(int rowIndex, int columnIndex)
    {
        var stableRowIndex = rowIndex;
        var cursorRowIndex = rowIndex + 1;

        while (cursorRowIndex <= Heigth - 1)
        {
            var cursorElement = Positions[cursorRowIndex, columnIndex];

            if (cursorElement == 'O' || cursorElement == '#')
            {
                break;
            }
            else
            {
                stableRowIndex = cursorRowIndex;
                cursorRowIndex++;
            }
        }

        if (stableRowIndex != rowIndex)
        {
            Positions[rowIndex, columnIndex] = '.';
            Positions[stableRowIndex, columnIndex] = 'O';
        }
    }

    private void LetRoundRockFallEast(int rowIndex, int columnIndex)
    {
        var stableColumnIndex = columnIndex;
        var cursorColumnIndex = columnIndex + 1;

        while (cursorColumnIndex <= Width - 1)
        {
            var cursorElement = Positions[rowIndex, cursorColumnIndex];

            if (cursorElement == 'O' || cursorElement == '#')
            {
                break;
            }
            else
            {
                stableColumnIndex = cursorColumnIndex;
                cursorColumnIndex++;
            }
        }

        if (stableColumnIndex != columnIndex)
        {
            Positions[rowIndex, columnIndex] = '.';
            Positions[rowIndex, stableColumnIndex] = 'O';
        }
    }
}
