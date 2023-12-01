var digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
var digitWords = new string[]
{
    "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
};

var calibrationValues = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\calibration-values.txt"));
var sum = 0;

foreach (var calibrationValue in calibrationValues)
{
    var digitsArray = new char[2];
    var exceptionMessage = $"No digit representations were found in calibration value: {calibrationValue}.";

    var firstDigitIndex = GetFirstDigitIndex(calibrationValue, out char firstDigit);
    var firstWordDigitIndex = GetFirstWordDigitIndex(calibrationValue, out char firstWordDigit);

    if (firstDigitIndex >= 0 && firstWordDigitIndex >= 0)
    {
        if (firstDigitIndex > firstWordDigitIndex)
        {
            digitsArray[0] = firstWordDigit;
        }
        else
        {
            digitsArray[0] = firstDigit;
        }
    }
    else if (firstDigitIndex >= 0)
    {
        digitsArray[0] = firstDigit;
    }
    else if (firstWordDigitIndex >= 0)
    {
        digitsArray[0] = firstWordDigit;
    }
    else throw new Exception(exceptionMessage);

    var lastDigitIndex = GetLastDigitIndex(calibrationValue, out char lastDigit);
    var lastWordDigitIndex = GetLastWordDigitIndex(calibrationValue, out char lastWordDigit);

    if (lastDigitIndex >= 0 && lastWordDigitIndex >= 0)
    {
        if (lastDigitIndex > lastWordDigitIndex)
        {
            digitsArray[1] = lastDigit;
        }
        else
        {
            digitsArray[1] = lastWordDigit;
        }
    }
    else if (lastDigitIndex >= 0)
    {
        digitsArray[1] = lastDigit;
    }
    else if (lastWordDigitIndex >= 0)
    {
        digitsArray[1] = lastWordDigit;
    }
    else throw new Exception(exceptionMessage);

    var sumString = new string(digitsArray);
    sum += int.Parse(sumString);
}

Console.WriteLine(sum.ToString());

int GetFirstDigitIndex(string calibrationValue, out char wordDigit)
{
    var minIndex = -1;
    wordDigit = default;

    for (var count = 0; count < digits.Length; count++)
    {
        var digitChar = digits[count];
        var index = calibrationValue.IndexOf(digitChar);

        if (index >= 0 && (minIndex < 0 || index < minIndex))
        {
            minIndex = index;
            wordDigit = digitChar;
        }
    }

    return minIndex;
}

int GetFirstWordDigitIndex(string calibrationValue, out char wordDigit)
{
    var minIndex = -1;
    wordDigit = default;

    for (var count = 0; count < digitWords.Length; count++)
    {
        var digitWord = digitWords[count];
        var index = calibrationValue.IndexOf(digitWord);

        if (index >= 0 && (minIndex < 0 || index < minIndex))
        {
            minIndex = index;
            wordDigit = digits[count];
        }
    }

    return minIndex;
}

int GetLastDigitIndex(string calibrationValue, out char wordDigit)
{
    var maxIndex = -1;
    wordDigit = default;

    for (var count = 0; count < digits.Length; count++)
    {
        var digitChar = digits[count];
        var index = calibrationValue.LastIndexOf(digitChar);

        if (index >= 0 && (maxIndex < 0 || index > maxIndex))
        {
            maxIndex = index;
            wordDigit = digitChar;
        }
    }

    return maxIndex;
}

int GetLastWordDigitIndex(string calibrationValue, out char wordDigit)
{
    var maxIndex = -1;
    wordDigit = default;

    for (var count = 0; count < digitWords.Length; count++)
    {
        var digitWord = digitWords[count];
        var index = calibrationValue.LastIndexOf(digitWord);

        if (index >= 0 && (maxIndex < 0 || index > maxIndex))
        {
            maxIndex = index;
            wordDigit = digits[count];
        }
    }

    return maxIndex;
}
