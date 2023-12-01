var calibrationValues = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\calibration-values.txt"));
var sum = 0;

foreach (var calibrationValue in calibrationValues)
{
    var digitsArray = new char[2];
    digitsArray[0] = calibrationValue.First(char.IsDigit);
    digitsArray[1] = calibrationValue.Last(char.IsDigit);
    var sumString = new string(digitsArray);
    sum += int.Parse(sumString);
}

Console.WriteLine(sum.ToString());