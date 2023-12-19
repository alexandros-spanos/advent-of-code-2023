var stepsSequence = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\initialization-sequence.txt"))[0];
var steps = stepsSequence.Split(',');
var sum = 0;

foreach (var step in steps)
{
    sum += Solver.GetHashCode(step);
}

Console.WriteLine(sum);

class Solver
{
    public static int GetHashCode(string step)
    {
        var result = 0;

        for (int i = 0; i < step.Length; i++)
        {
            var asciiCode = (int)step[i];
            result += asciiCode;
            result = 17 * result;
            result %= 256;
        }

        return result;
    }
}
