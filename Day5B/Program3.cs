/*using System.Collections.Concurrent;
using System.Diagnostics;

var almanac = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\almanac.txt"));
var timer = new Stopwatch();
timer.Start();

var seedPairs = almanac[0]
    .Split(':')[1]
    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToList().ConvertAll(long.Parse)
    .Chunk(2).ToList();

var map = new Map(almanac.Skip(1).ToList());
map.ParseLines();

var minimumLocations = Map.InitializeMinimums(seedPairs.Count);
var rangePartitioner = Partitioner.Create(0, seedPairs.Count);

Parallel.ForEach(rangePartitioner, range =>
{
    for (var index = range.Item1; index < range.Item2; index++)
    {
        var seedPair = seedPairs[index];
        var rangeStart = seedPair[0];
        var rangeSize = seedPair[1];
        var rangeEnd = rangeStart + rangeSize - 1;

        var minimum = map.MinRangeLocation(rangeStart, rangeEnd, 0, (index + 1).ToString());
        Map.UpdateMinimum(minimumLocations, index, minimum);
    }
});

timer.Stop();
Console.WriteLine($"Minimum: {minimumLocations.Min()}. Elapsed time: {timer.Elapsed}");

class Map
{
    public List<MaterialMap> MaterialMaps { get; private set; }
    private readonly List<string> _lines;
    private int _cursor;

    public Map(List<string> lines)
    {
        MaterialMaps = new List<MaterialMap>();
        _lines = lines;
    }

    public long MinRangeLocation(long seedStartIndex, long seedEndIndex, int mapIndex, string logId)
    {
        var logLabel = $"{logId} -{mapIndex + 1}-";
        Console.WriteLine($"start: {logLabel} [{seedStartIndex},{seedEndIndex}].");

        long minimumLocation;
        var map = MaterialMaps[mapIndex];
        var ranges = map.ProjectRange(seedStartIndex, seedEndIndex);

        if (mapIndex < MaterialMaps.Count - 1)
        {
            var minimumLocations = InitializeMinimums(ranges.Count);
            var rangePartitioner = Partitioner.Create(0, ranges.Count);

            Parallel.ForEach(rangePartitioner, rangeSet =>
            {
                for (int index = rangeSet.Item1; index < rangeSet.Item2; index++)
                {
                    var range = ranges[index];
                    var rangeStart = range.Item1;
                    var rangeEnd = range.Item2;
                    var minimum = MinRangeLocation(rangeStart, rangeEnd, mapIndex + 1, $"{logId}.{index + 1}");
                    UpdateMinimum(minimumLocations, index, minimum);
                }
            });

            minimumLocation = minimumLocations.Min();
        }
        else
        {
            minimumLocation = ranges.Min(range => range.Item1);
        }

        Console.WriteLine($"  end: {logLabel} [{seedStartIndex},{seedEndIndex}] => min: {minimumLocation}.");
        return minimumLocation;
    }

    public long MapSeed(long theValue)
    {
        foreach (var map in MaterialMaps)
        {
            theValue = map.Map(theValue);
        }

        return theValue;
    }

    public void ParseLines()
    {
        var mapStartLineIndex = 0;
        _cursor = 0;

        while (ExistsNextEmptyLine(out var nextEmptyLineIndex))
        {
            AddMaterialMap(mapStartLineIndex, nextEmptyLineIndex);
            mapStartLineIndex = nextEmptyLineIndex;
        }

        AddMaterialMap(mapStartLineIndex, _lines.Count);
    }

    private void AddMaterialMap(int mapStartLineIndex, int nextEmptyLineIndex)
    {
        var map = new MaterialMap();
        map.ParseLines(_lines.Skip(mapStartLineIndex + 2).Take(nextEmptyLineIndex - mapStartLineIndex - 2));
        MaterialMaps.Add(map);
    }

    private bool ExistsNextEmptyLine(out int emptyLineIndex)
    {
        emptyLineIndex = -1;
        while (_cursor < _lines.Count - 1)
        {
            _cursor++;
            var line = _lines[_cursor];

            if (string.IsNullOrWhiteSpace(line))
            {
                emptyLineIndex = _cursor;
                return true;
            }
        }
        return false;
    }

    public static long[] InitializeMinimums(int collectionLength)
    {
        var minimumLocations = new long[collectionLength];

        for (var index = 0; index < collectionLength; index++)
        {
            minimumLocations[index] = -1;
        }

        return minimumLocations;
    }

    public static void UpdateMinimum(long[] minimumLocations, int index, long minimum)
    {
        var minimumLocation = minimumLocations[index];

        if (minimumLocation < 0 || minimumLocation > minimum)
        {
            minimumLocations[index] = minimum;
        }
    }
}

class MaterialMap
{
    private LinkedList<SectionMap> _sections = new LinkedList<SectionMap>();

    public List<Tuple<long, long>> ProjectRange(long start, long end)
    {
        var previous = start;
        var result = new List<Tuple<long, long>>();
        var node = _sections.First;

        if (node == null)
        {
            return new List<Tuple<long, long>>() { Tuple.Create(start, end) };
        }

        while (node != null)
        {
            var section = node.Value;
            var intersection = section.GetIntersection(start, end);

            if (intersection.Item1 != -1)
            {
                if (intersection.Item1 > previous)
                {
                    result.Add(Tuple.Create(previous, intersection.Item1 - 1));
                }

                result.Add(section.MapRange(intersection.Item1, intersection.Item2));
                previous = intersection.Item2 + 1;
            }

            node = node.Next;
        }

        if (previous <= end)
        {
            result.Add(Tuple.Create(previous, end));
        }

        return result;
    }

    public long Map(long sourceIndex)
    {
        var sectionMap = GetSection(sourceIndex);

        if (sectionMap == null)
        {
            return sourceIndex;
        }

        return sectionMap.Value.Map(sourceIndex);
    }

    private LinkedListNode<SectionMap>? GetSection(long sourceIndex)
    {
        var node = _sections.First;

        while (node != null)
        {
            var section = node.Value;

            if (section.SourceStartIndex <= sourceIndex)
            {
                if (section.SourceEndIndex >= sourceIndex)
                {
                    return node;
                }
                else
                {
                    node = node.Next;
                }
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    public void ParseLines(IEnumerable<string> lines)
    {
        var sections = new List<SectionMap>();

        foreach (var line in lines)
        {
            sections.Add(new SectionMap(line));
        }

        sections.Sort((a, b) => a.SourceStartIndex.CompareTo(b.SourceStartIndex));
        _sections = new LinkedList<SectionMap>(sections);
    }
}

class SectionMap
{
    public long SourceStartIndex { get; private set; }
    public long SourceEndIndex { get; private set; }
    public long DestinationStartIndex { get; private set; }
    public long RangeSize { get; private set; }

    public Tuple<long, long> GetIntersection(long start, long end)
    {
        long rangeStart = -1, rangeEnd = -1;

        if (start < SourceStartIndex && end > SourceEndIndex)
        {
            rangeStart = SourceStartIndex;
            rangeEnd = SourceEndIndex;
        }
        else if (start < SourceStartIndex && end >= SourceStartIndex && end <= SourceEndIndex)
        {
            rangeStart = SourceStartIndex;
            rangeEnd = end;
        }
        else if (start >= SourceStartIndex && start <= SourceEndIndex && end > SourceEndIndex)
        {
            rangeStart = start;
            rangeEnd = SourceEndIndex;
        }
        else if (start >= SourceStartIndex && end <= SourceEndIndex)
        {
            rangeStart = start;
            rangeEnd = end;
        }

        return Tuple.Create(rangeStart, rangeEnd);
    }

    public Tuple<long, long> MapRange(long start, long end)
    {
        return Tuple.Create(Map(start), Map(end));
    }

    public long Map(long source)
    {
        return DestinationStartIndex + source - SourceStartIndex;
    }

    public SectionMap(string line)
    {
        var lineParts = line.Split(' ');
        DestinationStartIndex = long.Parse(lineParts[0]);
        SourceStartIndex = long.Parse(lineParts[1]);
        RangeSize = long.Parse(lineParts[2]);
        SourceEndIndex = SourceStartIndex + RangeSize - 1;
    }
}*/