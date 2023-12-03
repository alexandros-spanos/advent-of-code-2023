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
    public List<int> GearRatios { get; private set; }

    private char[][] _data { get; set; }

    public void ProcessData()
    {
        GearRatios = new List<int>();

        for (int cursorY = 0; cursorY < _data.Length; cursorY++)
        {
            var dataLine = _data[cursorY];
            var cursorX = 0;

            while (cursorX < Xsize)
            {
                var dataPoint = dataLine[cursorX];

                if (!(char.IsDigit(dataPoint) || dataPoint == EmptySpace))
                {
                    var numbers = new List<int>();

                    if (HasNumberLeft(dataLine, cursorX, out var numberLeft))
                    {
                        numbers.Add(numberLeft);
                    }

                    if (HasNumberRight(dataLine, cursorX, out var numberRight))
                    {
                        numbers.Add(numberRight);
                    }

                    if (cursorY > 0 && HasNumbersAdjacentLine(_data[cursorY - 1], cursorX, out var aboveNumbers))
                    {
                        numbers.AddRange(aboveNumbers);
                    }

                    if (cursorY < Ysize - 1 && HasNumbersAdjacentLine(_data[cursorY + 1], cursorX, out var belowNumbers))
                    {
                        numbers.AddRange(belowNumbers);
                    }

                    if (numbers.Count == 2)
                    {
                        GearRatios.Add(numbers.Aggregate((a, b) => a * b));
                    }
                }

                cursorX++;
            }
        }

        Sum = GearRatios.Sum();
    }

    private bool HasNumbersAdjacentLine(char[] dataLine, int xCoordinate, out List<int> numbers)
    {
        numbers = new List<int>();
        var dataPoint = dataLine[xCoordinate];
        var digitsLeft = GetDigitsLeft(dataLine, xCoordinate);
        var digitsRight = GetDigitsRight(dataLine, xCoordinate);

        if (char.IsDigit(dataPoint))
        {
            var digits = new List<char>() { dataPoint };
            digits.InsertRange(0, digitsLeft);
            digits.AddRange(digitsRight);
            numbers.Add(int.Parse(new string(digits.ToArray())));
        }
        else
        {
            if (digitsLeft.Count > 0)
            {
                numbers.Add(int.Parse(new string(digitsLeft.ToArray())));
            }

            if (digitsRight.Count > 0)
            {
                numbers.Add(int.Parse(new string(digitsRight.ToArray())));
            }
        }

        return numbers.Count > 0;
    }

    private bool HasNumberLeft(char[] dataLine, int xCoordinate, out int number)
    {
        number = -1;

        if (xCoordinate == 0)
        {
            return false;
        }

        var digitsChars = GetDigitsLeft(dataLine, xCoordinate);

        if (digitsChars.Count > 0)
        {
            number = int.Parse(new string(digitsChars.ToArray()));
        }

        return number >= 0;
    }

    private bool HasNumberRight(char[] dataLine, int xCoordinate, out int number)
    {
        number = -1;

        if (xCoordinate == Xsize - 1)
        {
            return false;
        }

        var digitsChars = GetDigitsRight(dataLine, xCoordinate);

        if (digitsChars.Count > 0)
        {
            number = int.Parse(new string(digitsChars.ToArray()));
        }

        return number >= 0;
    }

    private static List<char> GetDigitsLeft(char[] dataLine, int xCoordinate)
    {
        var digitsChars = new List<char>();

        while (xCoordinate > 0)
        {
            xCoordinate--;
            var dataPoint = dataLine[xCoordinate];

            if (char.IsDigit(dataPoint))
            {
                digitsChars.Insert(0, dataPoint);
            }
            else break;
        }

        return digitsChars;
    }

    private List<char> GetDigitsRight(char[] dataLine, int xCoordinate)
    {
        var digitsChars = new List<char>();

        while (xCoordinate < Xsize - 1)
        {
            xCoordinate++;
            var dataPoint = dataLine[xCoordinate];

            if (char.IsDigit(dataPoint))
            {
                digitsChars.Add(dataPoint);
            }
            else break;
        }

        return digitsChars;
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
