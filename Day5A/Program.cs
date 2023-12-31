﻿var almanac = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\almanac.txt"));
long minimumLocation = -1;

var seeds = almanac[0]
    .Split(':')[1]
    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToList().ConvertAll(long.Parse);

var map = new Map(almanac.Skip(1).ToList());
map.ParseLines();

foreach (var seed in seeds)
{
    var destination = map.MapSeed(seed);

    if (minimumLocation < 0 || minimumLocation > destination)
    {
        minimumLocation = destination;
    }
}

Console.WriteLine(minimumLocation.ToString());

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
}

class MaterialMap
{
    private readonly List<SectionMap> _sections;

    public MaterialMap()
    {
        _sections = new List<SectionMap>();
    }

    public long Map(long sourceIndex)
    {
        var sectionMap = _sections.Find(section =>
            section.SourceStartIndex <= sourceIndex &&
            section.SourceEndIndex >= sourceIndex);

        if (sectionMap == null)
        {
            return sourceIndex;
        }

        var shift = sourceIndex - sectionMap.SourceStartIndex;

        return sectionMap.DestinationStartIndex + shift;
    }

    public void ParseLines(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            _sections.Add(new SectionMap(line));
        }

        _sections.Sort((a, b) => a.SourceStartIndex.CompareTo(b.SourceStartIndex));
    }
}

class SectionMap
{
    public long SourceStartIndex { get; private set; }
    public long SourceEndIndex { get; private set; }
    public long DestinationStartIndex { get; private set; }
    public long IndexRange { get; private set; }

    public SectionMap(string line)
    {
        var lineParts = line.Split(' ');

        DestinationStartIndex = long.Parse(lineParts[0]);
        SourceStartIndex = long.Parse(lineParts[1]);
        IndexRange = long.Parse(lineParts[2]);
        SourceEndIndex = SourceStartIndex + IndexRange - 1;
    }
}
