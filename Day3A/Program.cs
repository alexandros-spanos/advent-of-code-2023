var frameLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\schematic.txt"));

var frame = new Frame();
frame.ParseLines(frameLines);
frame.ProcessData();

Console.WriteLine(frame.Sum.ToString());

class Frame
{
    private const char EmptySpace = '.';

    public int Sum { get; set; }
    public int Xsize { get; private set; }
    public int Ysize { get; private set; }
    public List<int> PartNumbers { get; private set; }

    private char[][] _data { get; set; }

    public void ProcessData()
    {
        PartNumbers = new List<int>();
        var digitsChars = new List<char>();

        for (int cursorY = 0; cursorY < _data.Length; cursorY++)
        {
            var dataLine = _data[cursorY];
            int cursorX = 0, numberStartIndex = 0, numberEndIndex = 0;

            while (cursorX < Xsize)
            {
                var dataPoint = dataLine[cursorX];

                if (char.IsDigit(dataPoint))
                {
                    if (digitsChars.Count == 0)
                    {
                        numberStartIndex = cursorX;
                    }

                    digitsChars.Add(dataPoint);

                    if (cursorX == Xsize - 1)
                    {
                        numberEndIndex = cursorX;
                    }
                }
                else if (digitsChars.Count > 0)
                {
                    numberEndIndex = cursorX - 1;
                }

                if (numberEndIndex > 0)
                {
                    if ((numberStartIndex > 0 && dataLine[numberStartIndex - 1] != EmptySpace) ||
                        (numberEndIndex < Xsize - 1 && dataLine[numberEndIndex + 1] != EmptySpace) ||
                        (cursorY > 0 && ContainsSymbol(cursorY - 1, numberStartIndex, numberEndIndex)) ||
                        (cursorY < Ysize - 1 && ContainsSymbol(cursorY + 1, numberStartIndex, numberEndIndex)))
                    {
                        PartNumbers.Add(int.Parse(new string(digitsChars.ToArray())));
                    }

                    numberStartIndex = numberEndIndex = 0;
                    digitsChars.Clear();
                }

                cursorX++;
            }
        }

        Sum = PartNumbers.Sum();
    }

    private bool ContainsSymbol(int lineYcoordinate, int startXcoordinate, int endXcoordinate)
    {
        var dataLine = _data[lineYcoordinate];

        if (startXcoordinate > 0)
        {
            startXcoordinate--;
        }

        if (endXcoordinate < Xsize - 1)
        {
            endXcoordinate++;
        }

        for (var index = startXcoordinate; index <= endXcoordinate; index++)
        {
            var dataPoint = dataLine[index];

            if (!(char.IsDigit(dataPoint) || dataPoint == EmptySpace))
            {
                return true;
            }
        }

        return false;
    }

    public void ParseLines(string[] lines)
    {
        Ysize = lines.Length;
        Xsize = lines.First().Length;
        _data = new char[Ysize][];

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            _data[i] = line.ToCharArray();
        }
    }
}
