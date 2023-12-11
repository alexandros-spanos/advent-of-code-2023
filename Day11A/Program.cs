var observableUniverse = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\stellar-map.txt"));

var universe = new Universe();
universe.ParseLines(observableUniverse);
universe.ExpandUniverse();
var sum = universe.CalculateDistansesSum();

Console.WriteLine(sum);

class Universe
{
    private List<Galaxy> Galaxies { get; set; }
    private List<int> EmptyRowsIndexes { get; set; }
    private List<int> EmptyColumnsIndexes { get; set; }

    public void ParseLines(string[] lines)
    {
        Galaxies = [];
        EmptyRowsIndexes = [];
        EmptyColumnsIndexes = [];

        for (var rowIndex = 0; rowIndex < lines.Length; rowIndex++)
        {
            var coordinates = GetCoordinateX(lines[rowIndex]);

            foreach (var coordinate in coordinates)
            {
                Galaxies.Add(new Galaxy(rowIndex, coordinate));
            }

            if (coordinates.Count == 0)
            {
                EmptyRowsIndexes.Add(rowIndex);
            }
        }

        for (var columnIndex = 0; columnIndex < lines[0].Length; columnIndex++)
        {
            var columnEmpty = true;

            for (var rowIndex = 0; rowIndex < lines.Length; rowIndex++)
            {
                if (lines[rowIndex][columnIndex] == '#')
                {
                    columnEmpty = false;
                }
            }

            if (columnEmpty)
            {
                EmptyColumnsIndexes.Add(columnIndex);
            }
        }
    }

    public void ExpandUniverse()
    {
        var shift = 0;
        foreach (var emptyRowIndex in EmptyRowsIndexes)
        {
            foreach (var galaxy in Galaxies.Where(g => g.Y > emptyRowIndex + shift))
            {
                galaxy.Y += 1;
            }
            shift++;
        }

        shift = 0;
        foreach (var emptyColumnIndex in EmptyColumnsIndexes)
        {
            foreach (var galaxy in Galaxies.Where(g => g.X > emptyColumnIndex + shift))
            {
                galaxy.X += 1;
            }
            shift++;
        }
    }

    public int CalculateDistansesSum()
    {
        var sum = 0;

        for (var outerIndex = 0; outerIndex < Galaxies.Count; outerIndex++)
        {
            var galaxy = Galaxies[outerIndex];

            for (var innerIndex = outerIndex + 1; innerIndex < Galaxies.Count; innerIndex++)
            {
                sum += GetDistanse(galaxy, Galaxies[innerIndex]);
            }
        }

        return sum;
    }

    private int GetDistanse(Galaxy g1, Galaxy g2)
    {
        return Math.Abs(g1.Y - g2.Y) + Math.Abs(g1.X - g2.X);
    }

    private List<int> GetCoordinateX(string line)
    {
        var coordinates = new List<int>();
        var index = 0;

        while (index < line.Length)
        {
            if (line[index] == '#')
            {
                coordinates.Add(index);
            }
            index++;
        }
        return coordinates;
    }
}

class Galaxy
{
    public int Y { get; set; }
    public int X { get; set; }

    public Galaxy(int y, int x)
    {
        Y = y;
        X = x;
    }
}
