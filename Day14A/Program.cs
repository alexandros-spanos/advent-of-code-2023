var platformLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\platform.txt"));

var platform = new Platform();
platform.ParseLines(platformLines);
var sum = platform.Solve();

Console.WriteLine(sum);

class Platform
{
    public int Heigth { get; set; }
    public int Width { get; set; }
    public char[,] Positions { get; set; }

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
        var result = 0;

        for (var columnIndex = 0; columnIndex < Width; columnIndex++)
        {
            LetColumnFall(columnIndex);
        }

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

    private void LetColumnFall(int columnIndex)
    {
        for (var rowIndex = 1; rowIndex < Heigth; rowIndex++)
        {
            var positionElement = Positions[rowIndex, columnIndex];

            if (positionElement == 'O')
            {
                LetRoundRockFall(rowIndex, columnIndex);
            }
        }
    }

    private void LetRoundRockFall(int rowIndex, int columnIndex)
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
}
