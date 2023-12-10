var tiles = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\tiles.txt"));

var grid = new Grid();
grid.ParseLines(tiles);
var count = grid.ProcessTiles();

Console.WriteLine(count);

class Grid
{
    private char[][] Tiles { get; set; }
    private GridCursor Cursor { get; set; }

    public int ProcessTiles()
    {
        Cursor = new GridCursor(Tiles);
        Cursor.FindStartingPoint();
        var count = Cursor.CountLoop();
        return (int)Math.Ceiling((double)count / 2);
    }

    public void ParseLines(string[] lines)
    {
        Tiles = new char[lines.Length][];

        for (var index = 0; index < Tiles.Length; index++)
        {
            Tiles[index] = lines[index].ToCharArray();
        }
    }
}

class GridCursor
{
    public int X { get; private set; }
    public int Y { get; private set; }

    private char[][] Tiles { get; set; }
    private int InitialX { get; set; }
    private int InitialY { get; set; }
    private int GridWidth { get; set; }
    private int GridHeight { get; set; }
    private Direction Direction { get; set; }

    public GridCursor(char[][] tiles)
    {
        Tiles = tiles;
        GridWidth = Tiles[0].Length;
        GridHeight = Tiles.Length;
    }

    public void FindStartingPoint()
    {
        for (var rowIndex = 0; rowIndex < Tiles.Length; rowIndex++)
        {
            var row = Tiles[rowIndex];
            var columnIndex = Array.IndexOf(row, 'S');

            if (columnIndex >= 0)
            {
                X = columnIndex;
                Y = rowIndex;
                InitialX = X;
                InitialY = Y;
                return;
            }
        }

        throw new Exception("Starting point not found.");
    }

    public int CountLoop()
    {
        var count = 0;
        FindInitialDirection();

        do
        {
            Move(Direction);
            count++;
        }
        while (X != InitialX || Y != InitialY);

        return count;
    }

    private void FindInitialDirection()
    {
        char point;
        var found = false;
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
                found = true;
            }
        }

        if (spaceAbove && !found)
        {
            point = Tiles[Y - 1][X];

            if (point == '|'
                || (spaceLeft && point == '7')
                || (spaceRight && point == 'F'))
            {
                Direction = new Direction(0, -1);
                found = true;
            }
        }

        if (spaceRight && !found)
        {
            point = Tiles[Y][X + 1];

            if (point == '-'
                || (spaceAbove && point == 'J')
                || (spaceBelow && point == '7'))
            {
                Direction = new Direction(1, 0);
                found = true;
            }
        }

        if (spaceLeft && !found)
        {
            point = Tiles[Y][X - 1];

            if (point == '-'
                || (spaceAbove && point == 'L')
                || (spaceBelow && point == 'F'))
            {
                Direction = new Direction(-1, 0);
                found = true;
            }
        }

        if (!found)
        {
            throw new Exception("Grid coordinates overflown or path is single point.");
        }
    }

    private void Move(Direction direction)
    {
        X += direction.X;
        Y += direction.Y;
        Direction.ApplyTransformation(Tiles[Y][X]);
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
                    X = -1;
                    Y = 0;
                }
                else
                {
                    X = 0;
                    Y = -1;
                }
                break;
            case 'L':
                if (X == 0)
                {
                    X = 1;
                    Y = 0;
                }
                else
                {
                    X = 0;
                    Y = -1;
                }
                break;
            case '7':
                if (X == 0)
                {
                    X = -1;
                    Y = 0;
                }
                else
                {
                    X = 0;
                    Y = 1;
                }
                break;
            case 'F':
                if (X == 0)
                {
                    X = 1;
                    Y = 0;
                }
                else
                {
                    X = 0;
                    Y = 1;
                }
                break;
            case '.':
                throw new Exception("Pipe leaks to ground.");
            default:
                throw new Exception($"Unknown pipe type found: {pipeType}.");
        }
    }
}
