var tiles = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\tiles.txt"));

var grid = new Grid();
grid.ParseLines(tiles);
var count = grid.ProcessTiles();

Console.WriteLine(count);

class Grid
{
    private char[][] Tiles { get; set; }
    private GridCursor Cursor { get; set; }
    private Dictionary<int, List<int>> Path { get; set; }
    private Dictionary<int, List<int>> Insides { get; set; }

    public void ParseLines(string[] lines)
    {
        Tiles = new char[lines.Length][];

        for (var index = 0; index < Tiles.Length; index++)
        {
            Tiles[index] = lines[index].ToCharArray();
        }
    }

    public int ProcessTiles()
    {
        var insidesCount = 0;
        Cursor = new GridCursor(Tiles);
        Path = Cursor.FindPath();
        Insides = [];
        Tiles[Cursor.InitialY][Cursor.InitialX] = Cursor.StartingPointType;

        foreach (var rowIndex in Path.Keys)
        {
            insidesCount += CountInsidesOnRow(rowIndex);
        }

        PrintGrid();
        return insidesCount;
    }

    private int CountInsidesOnRow(int rowIndex)
    {
        var row = Tiles[rowIndex];
        var pathElements = Path[rowIndex];
        int countPathVerticals = 0, insidesCount = 0;
        char lastPathElement = ' ';

        for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
        {
            if (pathElements.Contains(columnIndex))
            {
                var element = row[columnIndex];

                if (element == '|'
                    || (element == '7' && lastPathElement == 'L')
                    || (element == 'J' && lastPathElement == 'F'))
                {
                    countPathVerticals++;
                }

                if (element == 'L' || element == 'F')
                {
                    lastPathElement = element;
                }
            }
            else if (countPathVerticals % 2 != 0)
            {
                insidesCount++;
                GridCursor.UpdateListDictionary(Insides, rowIndex, columnIndex);
            }
        }

        return insidesCount;
    }

    private void PrintGrid()
    {
        for (var i = 0; i < Tiles.Length; i++)
        {
            bool checkPath = false, checkInsides = false;

            if (Path.TryGetValue(i, out var pathElements))
            {
                checkPath = true;
            }
            if (Insides.TryGetValue(i, out var insides))
            {
                checkInsides = true;
            }

            for (var j = 0; j < Tiles[i].Length; j++)
            {
                if (checkPath && pathElements.Contains(j))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (checkInsides && insides.Contains(j))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.Write(Tiles[i][j]);
            }

            Console.WriteLine();
        }
    }
}

class GridCursor
{
    public int InitialX { get; set; }
    public int InitialY { get; set; }
    public char StartingPointType { get; set; }

    private char[][] Tiles { get; set; }
    private int GridWidth { get; set; }
    private int GridHeight { get; set; }
    private int X { get; set; }
    private int Y { get; set; }
    private Direction Direction { get; set; }

    public GridCursor(char[][] tiles)
    {
        Tiles = tiles;
        GridWidth = Tiles[0].Length;
        GridHeight = Tiles.Length;
    }

    public Dictionary<int, List<int>> FindPath()
    {
        FindStartingPoint();
        FindInitialDirection();

        var path = new Dictionary<int, List<int>>();
        do
        {
            Move(Direction);
            UpdateListDictionary(path, Y, X);
        }
        while (X != InitialX || Y != InitialY);

        return path;
    }

    private void FindStartingPoint()
    {
        for (var rowIndex = 0; rowIndex < Tiles.Length; rowIndex++)
        {
            var row = Tiles[rowIndex];
            var columnIndex = Array.IndexOf(row, 'S');

            if (columnIndex >= 0)
            {
                InitialX = X = columnIndex;
                InitialY = Y = rowIndex;
                return;
            }
        }

        throw new Exception("Starting point not found.");
    }

    private void FindInitialDirection()
    {
        Direction otherDirection = null;
        char point;
        var spaceBelow = Y + 1 < GridHeight;
        var spaceAbove = Y - 1 >= 0;
        var spaceRight = X + 1 < GridWidth;
        var spaceLeft = X - 1 >= 0;

        if (spaceBelow)
        {
            point = Tiles[Y + 1][X];

            if (point == '|'
                || (spaceLeft && point == 'J')
                || (spaceRight && point == 'L'))
            {
                Direction = new Direction(0, 1);
            }
        }

        if (spaceAbove && (Direction == null || otherDirection == null))
        {
            point = Tiles[Y - 1][X];

            if (point == '|'
                || (spaceLeft && point == '7')
                || (spaceRight && point == 'F'))
            {
                var direction = new Direction(0, -1);

                if (Direction == null) Direction = direction;
                else otherDirection = direction;
            }
        }

        if (spaceRight && (Direction == null || otherDirection == null))
        {
            point = Tiles[Y][X + 1];

            if (point == '-'
                || (spaceAbove && point == 'J')
                || (spaceBelow && point == '7'))
            {
                var direction = new Direction(1, 0);

                if (Direction == null) Direction = direction;
                else otherDirection = direction;
            }
        }

        if (spaceLeft && (Direction == null || otherDirection == null))
        {
            point = Tiles[Y][X - 1];

            if (point == '-'
                || (spaceAbove && point == 'L')
                || (spaceBelow && point == 'F'))
            {
                var direction = new Direction(-1, 0);

                if (Direction == null) Direction = direction;
                else otherDirection = direction;
            }
        }

        if (Direction == null || otherDirection == null)
        {
            throw new Exception("Grid coordinates overflown or path is single point.");
        }

        StartingPointType = GetType(Direction, otherDirection);
    }

    private void Move(Direction direction)
    {
        X += direction.X;
        Y += direction.Y;
        Direction.ApplyTransformation(Tiles[Y][X]);
    }

    private static char GetType(Direction enter, Direction exit)
    {
        if (enter.X != 0 && exit.X != 0)
        {
            return '-';
        }
        else if (enter.Y != 0 && exit.Y != 0)
        {
            return '|';
        }
        else if ((enter.X < 0 && exit.Y > 0) || (enter.Y < 0 && exit.X < 0))
        {
            return '7';
        }
        else if ((enter.X < 0 && exit.Y < 0) || (enter.Y > 0 && exit.X < 0))
        {
            return 'J';
        }
        else if ((enter.Y > 0 && exit.X > 0) || (enter.X > 0 && exit.Y < 0))
        {
            return 'L';
        }
        else if ((enter.Y < 0 && exit.X > 0) || (enter.X < 0 && exit.Y > 0))
        {
            return 'F';
        }
        throw new NotImplementedException();
    }

    public static void UpdateListDictionary(Dictionary<int, List<int>> dictionary, int y, int x)
    {
        if (dictionary.TryGetValue(y, out var list))
        {
            list.Add(x);
        }
        else
        {
            dictionary[y] = [x];
        }
    }
}

class Direction
{
    public short X { get; set; }
    public short Y { get; set; }

    public Direction(short x, short y)
    {
        X = x;
        Y = y;
    }

    public void ApplyTransformation(char pipeType)
    {
        switch (pipeType)
        {
            case 'S':
            case '|':
            case '-':
                break;
            case 'J':
                if (X == 0)
                {
                    X = -1; Y = 0;
                }
                else
                {
                    X = 0; Y = -1;
                }
                break;
            case 'L':
                if (X == 0)
                {
                    X = 1; Y = 0;
                }
                else
                {
                    X = 0; Y = -1;
                }
                break;
            case '7':
                if (X == 0)
                {
                    X = -1; Y = 0;
                }
                else
                {
                    X = 0; Y = 1;
                }
                break;
            case 'F':
                if (X == 0)
                {
                    X = 1; Y = 0;
                }
                else
                {
                    X = 0; Y = 1;
                }
                break;
            case '.':
                throw new Exception("Pipe leaks to ground.");
            default:
                throw new Exception($"Unknown pipe type found: {pipeType}.");
        }
    }
}
