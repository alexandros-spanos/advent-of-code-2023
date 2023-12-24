var mirrorPositions = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\mirror-grid.txt"));
var grid = new Grid();
grid.ParseLines(mirrorPositions);
grid.Flow();
var sum = grid.CountEnergized();
grid.Draw();
Console.WriteLine();
Console.WriteLine(sum);

class Grid
{
    public int Heigth { get; set; }
    public int Width { get; set; }
    public char[,] Pixels { get; private set; }

    private readonly List<Position> _intersections = [];
    private readonly List<Mirror> _mirrors = [];
    private Dictionary<int, LinkedList<Mirror>> _mirrorsByRow;
    private Dictionary<int, LinkedList<Mirror>> _mirrorsByColumn;
    private Dictionary<int, List<BeamPart>> _horizontalBeamsByRow;
    private Dictionary<int, List<BeamPart>> _verticalBeamsByColumn;

    public void ParseLines(string[] lines)
    {
        Heigth = lines.Length;
        Width = lines[0].Length;
        Pixels = new char[Heigth, Width];
        _mirrorsByRow = new Dictionary<int, LinkedList<Mirror>>(Heigth);
        _mirrorsByColumn = new Dictionary<int, LinkedList<Mirror>>(Width);
        _horizontalBeamsByRow = new Dictionary<int, List<BeamPart>>(Heigth);
        _verticalBeamsByColumn = new Dictionary<int, List<BeamPart>>(Width);

        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            var line = lines[rowIndex];
            for (var columnIndex = 0; columnIndex < Width; columnIndex++)
            {
                var pixel = line[columnIndex];
                Pixels[rowIndex, columnIndex] = pixel;
                if (pixel != '.')
                {
                    IndexMirror(rowIndex, columnIndex, pixel);
                }
            }
        }
    }

    public void Draw()
    {
        for (var rowIndex = 0; rowIndex < Heigth; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < Width; columnIndex++)
            {
                var pixel = Pixels[rowIndex, columnIndex];
                if (_intersections.Any(p => p.RowIndex == rowIndex && p.ColumnIndex == columnIndex))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else switch (pixel)
                    {
                        case '^':
                        case '<':
                        case 'v':
                        case '>':
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                        case '.':
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case '-':
                        case '|':
                        case '/':
                        case '\\':
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                    }
                Console.Write(pixel);
            }
        }
    }

    private void MarkBeam(BeamPart beam)
    {
        var position = beam.Start;
        var pixel = GetPixelType(beam.Direction);

        for (var i = 0; i < beam.Length; i++)
        {
            Pixels[position.RowIndex, position.ColumnIndex] = pixel;
            position = position.AdvanceByOneTile(beam.Direction);
        }
    }

    private static char GetPixelType(LightDirection direction) => direction switch
    {
        LightDirection.Up => '^',
        LightDirection.Left => '<',
        LightDirection.Down => 'v',
        LightDirection.Right => '>',
        _ => throw new NotImplementedException(Mirror.GetDirectionError(direction)),
    };

    public int CountEnergized()
    {
        var energizedNumber = 0;

        foreach (var horizontal in _horizontalBeamsByRow.Values.SelectMany(c => c))
        {
            energizedNumber += horizontal.Length;
            energizedNumber -= VerticalIntersections(horizontal);
        }

        foreach (var verticalBeams in _verticalBeamsByColumn.Values)
        {
            energizedNumber += verticalBeams.Select(b => b.Length).Where(l => l > 1).Sum();
        }

        energizedNumber += _mirrors.Count(m => m.OutwardBeams.Count > 0 || m.InwardBeams.Count > 0);

        return energizedNumber;
    }

    private int VerticalIntersections(BeamPart horizontal)
    {
        var intersectionsNumber = 0;
        var rowIndex = horizontal.Start.RowIndex;
        var columBounds = horizontal.GetBounds(true);

        for (var columnIndex = columBounds.Item1; columnIndex <= columBounds.Item2; columnIndex++)
        {
            if (_verticalBeamsByColumn.TryGetValue(columnIndex, out var verticalBeams))
            {
                foreach (var vertical in verticalBeams)
                {
                    if (horizontal.IsUnitary && vertical.IsUnitary && horizontal.Start.IsEqual(vertical.Start))
                    {
                        continue;
                    }
                    var verticalBounds = vertical.GetBounds(false);

                    if (verticalBounds.Item1 <= rowIndex && rowIndex <= verticalBounds.Item2)
                    {
                        intersectionsNumber++;
                        _intersections.Add(new Position(rowIndex, columnIndex));
                        break;
                    }
                }
            }
        }
        return intersectionsNumber;
    }

    public void Flow()
    {
        FlowBeam(new Position(0, 0), LightDirection.Right);
    }

    private void FlowBeam(Position startPosition, LightDirection startDirection, Mirror? originMirror = null)
    {
        var endMirror = GetProjectedMirror(startPosition, startDirection);

        if (endMirror == null)
        {
            CreateBeam(startDirection, originMirror, startPosition, GetEndPosition(startPosition, startDirection));
        }
        else
        {
            CreateBeamTo(startDirection, originMirror, startPosition, endMirror);

            foreach (var outgoingDirection in endMirror.GetOutgoingDirections(startDirection))
            {
                if (!endMirror.OutwardBeams.TryGetValue(outgoingDirection, out _) &&
                    !endMirror.InwardBeams.TryGetValue(outgoingDirection, out _))
                {
                    var newStartPosition = endMirror.Position.AdvanceByOneTile(outgoingDirection);

                    if (PositionIsValid(newStartPosition))
                    {
                        FlowBeam(newStartPosition, outgoingDirection, endMirror);
                    }
                }
            }
        }
    }

    private void CreateBeamTo(LightDirection startDirection, Mirror? originMirror, Position start, Mirror endMirror)
    {
        BeamPart newBeam;
        var reverseDirection = ReverseDirection(startDirection);

        newBeam = start.IsEqual(endMirror.Position)
            ? SaveDirectionBeam(BeamPart.Empty, startDirection, originMirror)
            : CreateBeam(startDirection, originMirror, start, endMirror.Position.AdvanceByOneTile(reverseDirection));

        endMirror.InwardBeams[reverseDirection] = newBeam;
    }

    private BeamPart CreateBeam(LightDirection startDirection, Mirror? originMirror, Position start, Position end)
    {
        var newBeam = new BeamPart(start, end, startDirection);
        IndexBeam(newBeam);
        MarkBeam(newBeam);
        return SaveDirectionBeam(newBeam, startDirection, originMirror);
    }

    private static BeamPart SaveDirectionBeam(BeamPart beam, LightDirection startDirection, Mirror? originMirror)
    {
        if (originMirror != null)
        {
            originMirror.OutwardBeams[startDirection] = beam;
        }
        return beam;
    }

    private void IndexBeam(BeamPart newBeam)
    {
        var start = newBeam.Start;
        var end = newBeam.End;

        if (start.RowIndex == end.RowIndex)
        {
            AddBeamToIndex(_horizontalBeamsByRow, start.RowIndex, newBeam);
        }
        if (start.ColumnIndex == end.ColumnIndex)
        {
            AddBeamToIndex(_verticalBeamsByColumn, start.ColumnIndex, newBeam);
        }
    }

    private bool PositionIsValid(Position position)
    {
        return 0 <= position.RowIndex && position.RowIndex < Heigth
            && 0 <= position.ColumnIndex && position.ColumnIndex < Width;
    }

    private Position GetEndPosition(Position position, LightDirection direction)
    {
        return direction switch
        {
            LightDirection.Up => new Position(0, position.ColumnIndex),
            LightDirection.Left => new Position(position.RowIndex, 0),
            LightDirection.Down => new Position(Heigth - 1, position.ColumnIndex),
            LightDirection.Right => new Position(position.RowIndex, Width - 1),
            _ => throw new NotImplementedException(Mirror.GetDirectionError(direction)),
        };
    }

    private Mirror? GetProjectedMirror(Position position, LightDirection direction)
    {
        return direction switch
        {
            LightDirection.Up => GetUpMirror(position),
            LightDirection.Left => GetLeftMirror(position),
            LightDirection.Down => GetDownMirror(position),
            LightDirection.Right => GetRightMirror(position),
            _ => throw new NotImplementedException(Mirror.GetDirectionError(direction)),
        };
    }

    private Mirror? GetUpMirror(Position position)
    {
        return GetPreviousMirror(_mirrorsByColumn, position.ColumnIndex, position.RowIndex, (mirror) => mirror.RowIndex);
    }

    private Mirror? GetLeftMirror(Position position)
    {
        return GetPreviousMirror(_mirrorsByRow, position.RowIndex, position.ColumnIndex, (mirror) => mirror.ColumnIndex);
    }

    private Mirror? GetDownMirror(Position position)
    {
        return GetNextMirror(_mirrorsByColumn, position.ColumnIndex, position.RowIndex, (mirror) => mirror.RowIndex);
    }

    private Mirror? GetRightMirror(Position position)
    {
        return GetNextMirror(_mirrorsByRow, position.RowIndex, position.ColumnIndex, (mirror) => mirror.ColumnIndex);
    }

    private void IndexMirror(int rowIndex, int columnIndex, char pixel)
    {
        var mirror = new Mirror(rowIndex, columnIndex, pixel);
        _mirrors.Add(mirror);
        AddMirrorToIndex(_mirrorsByRow, rowIndex, mirror);
        AddMirrorToIndex(_mirrorsByColumn, columnIndex, mirror);
    }

    private static Mirror? GetNextMirror(Dictionary<int, LinkedList<Mirror>> mirrorsIndex,
        int index, int startIndex, Func<Mirror, int> provider)
    {
        if (mirrorsIndex.TryGetValue(index, out var mirrors))
        {
            var mirror = mirrors.First;

            while (mirror != null)
            {
                if (provider(mirror.Value) >= startIndex)
                {
                    return mirror.Value;
                }
                else
                {
                    mirror = mirror.Next;
                }
            }
        }
        return null;
    }

    private static Mirror? GetPreviousMirror(Dictionary<int, LinkedList<Mirror>> mirrorsIndex,
        int index, int startIndex, Func<Mirror, int> provider)
    {
        if (mirrorsIndex.TryGetValue(index, out var mirrors))
        {
            var mirror = mirrors.Last;

            while (mirror != null)
            {
                if (provider(mirror.Value) <= startIndex)
                {
                    return mirror.Value;
                }
                else
                {
                    mirror = mirror.Previous;
                }
            }
        }
        return null;
    }

    private static void AddMirrorToIndex(Dictionary<int, LinkedList<Mirror>> mirrorsIndex,
        int index, Mirror mirror)
    {
        if (mirrorsIndex.TryGetValue(index, out var columnMirrors))
        {
            columnMirrors.AddLast(mirror);
        }
        else
        {
            mirrorsIndex[index] = new LinkedList<Mirror>(new Mirror[1] { mirror });
        }
    }

    private static void AddBeamToIndex(Dictionary<int, List<BeamPart>> beamsIndex,
        int index, BeamPart beam)
    {
        if (beamsIndex.TryGetValue(index, out var beams))
        {
            beams.Add(beam);
        }
        else
        {
            beamsIndex[index] = [beam];
        }
    }

    private static LightDirection ReverseDirection(LightDirection direction)
    {
        return direction switch
        {
            LightDirection.Up => LightDirection.Down,
            LightDirection.Left => LightDirection.Right,
            LightDirection.Down => LightDirection.Up,
            LightDirection.Right => LightDirection.Left,
            _ => throw new NotImplementedException(Mirror.GetDirectionError(direction)),
        };
    }
}

class Mirror(int rowIndex, int columnIndex, char pixel)
{
    public int RowIndex { get; private set; } = rowIndex;
    public int ColumnIndex { get; private set; } = columnIndex;
    public char Pixel { get; private set; } = pixel;
    public MirrorOrientation Orientation { get; private set; } = ParseOrientation(pixel);
    public Dictionary<LightDirection, BeamPart> OutwardBeams { get; private set; } = new Dictionary<LightDirection, BeamPart>(4);
    public Dictionary<LightDirection, BeamPart> InwardBeams { get; private set; } = new Dictionary<LightDirection, BeamPart>(4);

    public Position Position => _position ??= new Position(RowIndex, ColumnIndex);
    private Position? _position;

    public List<LightDirection> GetOutgoingDirections(LightDirection incomingDirection)
    {
        return Orientation switch
        {
            MirrorOrientation.Vertical => incomingDirection switch
            {
                LightDirection.Up or LightDirection.Down => [incomingDirection],
                LightDirection.Left or LightDirection.Right => [LightDirection.Up, LightDirection.Down],
                _ => throw new NotImplementedException(GetDirectionError(incomingDirection)),
            },
            MirrorOrientation.Horizontal => incomingDirection switch
            {
                LightDirection.Up or LightDirection.Down => [LightDirection.Left, LightDirection.Right],
                LightDirection.Left or LightDirection.Right => [incomingDirection],
                _ => throw new NotImplementedException(GetDirectionError(incomingDirection)),
            },
            MirrorOrientation.PositiveLean => incomingDirection switch
            {
                LightDirection.Up => [LightDirection.Right],
                LightDirection.Down => [LightDirection.Left],
                LightDirection.Left => [LightDirection.Down],
                LightDirection.Right => [LightDirection.Up],
                _ => throw new NotImplementedException(GetDirectionError(incomingDirection)),
            },
            MirrorOrientation.NegativeLean => incomingDirection switch
            {
                LightDirection.Up => [LightDirection.Left],
                LightDirection.Down => [LightDirection.Right],
                LightDirection.Left => [LightDirection.Up],
                LightDirection.Right => [LightDirection.Down],
                _ => throw new NotImplementedException(GetDirectionError(incomingDirection)),
            },
            _ => throw new NotImplementedException(GetOrientationError(Orientation)),
        };
    }

    private static MirrorOrientation ParseOrientation(char pixel) => pixel switch
    {
        '-' => MirrorOrientation.Horizontal,
        '|' => MirrorOrientation.Vertical,
        '/' => MirrorOrientation.PositiveLean,
        '\\' => MirrorOrientation.NegativeLean,
        _ => throw new NotImplementedException($"pixel type '{pixel}' is not supported."),
    };

    public static string GetOrientationError(MirrorOrientation orientation)
    {
        return $"orientation type '{orientation}' is not supported.";
    }

    public static string GetDirectionError(LightDirection direction)
    {
        return $"direction type '{direction}' is not supported.";
    }
}

class BeamPart
{
    public Position Start { get; private set; }
    public Position End { get; private set; }
    public LightDirection Direction { get; private set; }
    public bool IsHorizontal => _isHorizontal ??= Start.RowIndex == End.RowIndex;
    public bool IsUnitary => _isUnitary ??= Start.IsEqual(End);

    private bool? _isHorizontal, _isUnitary;
    private int _minColumn = -1, _maxColumn = -1, _minRow = -1, _maxRow = -1, _length = -1;

    public BeamPart(Position start, Position end, LightDirection direction)
    {
        Start = start;
        End = end;
        Direction = direction;
    }

    private BeamPart() { }

    public int Length
    {
        get
        {
            if (_length == -1)
            {
                if (IsUnitary) _length = 1;
                else
                {
                    var bounds = GetBounds(IsHorizontal);
                    _length = bounds.Item2 - bounds.Item1 + 1;
                }
            }
            return _length;
        }
    }

    public Tuple<int, int> GetBounds(bool byColumnn)
    {
        if (byColumnn)
        {
            if (_minColumn == -1)
            {
                _minColumn = Math.Min(Start.ColumnIndex, End.ColumnIndex);
                _maxColumn = Math.Max(Start.ColumnIndex, End.ColumnIndex);
            }
            return Tuple.Create(_minColumn, _maxColumn);
        }
        else
        {
            if (_minRow == -1)
            {
                _minRow = Math.Min(Start.RowIndex, End.RowIndex);
                _maxRow = Math.Max(Start.RowIndex, End.RowIndex);
            }
            return Tuple.Create(_minRow, _maxRow);
        }
    }

    public static BeamPart Empty = new();
}

class Position(int rowIndex, int columnIndex)
{
    public int RowIndex { get; private set; } = rowIndex;
    public int ColumnIndex { get; private set; } = columnIndex;

    public bool IsEqual(Position other)
    {
        return RowIndex == other.RowIndex
            && ColumnIndex == other.ColumnIndex;
    }

    public Position AdvanceByOneTile(LightDirection direction)
    {
        return direction switch
        {
            LightDirection.Up => new Position(RowIndex - 1, ColumnIndex),
            LightDirection.Left => new Position(RowIndex, ColumnIndex - 1),
            LightDirection.Down => new Position(RowIndex + 1, ColumnIndex),
            LightDirection.Right => new Position(RowIndex, ColumnIndex + 1),
            _ => throw new NotImplementedException(Mirror.GetDirectionError(direction)),
        };
    }
}

enum MirrorOrientation
{
    Vertical,
    Horizontal,
    PositiveLean,
    NegativeLean
}

enum LightDirection
{
    Up,
    Left,
    Down,
    Right
}
