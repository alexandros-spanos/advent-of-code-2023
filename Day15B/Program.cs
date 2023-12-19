var stepsSequence = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\initialization-sequence.txt"))[0];
var steps = stepsSequence.Split(',');
var solver = new Solver();
var sum = solver.ProcessSteps(steps);
Console.WriteLine(sum);

class Solver
{
    private readonly List<Step> Steps = [];
    private readonly Dictionary<int, Box> Boxes = [];

    public int ProcessSteps(string[] stepInstructions)
    {
        ReadSteps(stepInstructions);
        foreach (var step in Steps)
        {
            var boxIndex = step.GetBoxIndex();
            var box = GetBox(boxIndex);
            box.ExecuteStep(step);
        }

        var sum = 0;
        foreach (var boxInfo in Boxes)
        {
            var boxIndex = boxInfo.Key;
            var box = boxInfo.Value;
            sum += (boxIndex + 1) * box.Measure();
        }
        return sum;
    }

    private Box GetBox(int index)
    {
        if (Boxes.TryGetValue(index, out var box)) return box;
        else
        {
            box = new Box();
            Boxes[index] = box;
            return box;
        }
    }

    private void ReadSteps(string[] stepInstructions)
    {
        foreach (var stepInstruction in stepInstructions)
        {
            var step = new Step();
            step.ReadInstructions(stepInstruction);
            Steps.Add(step);
        }
    }

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

class Step
{
    public string Label { get; private set; }
    public int FocalLength { get; private set; } = 0;

    public bool IsRemoval => FocalLength == 0;

    public int GetBoxIndex()
    {
        return Solver.GetHashCode(Label);
    }

    public void ReadInstructions(string stepInstructions)
    {
        if (stepInstructions.EndsWith('-'))
        {
            Label = stepInstructions.TrimEnd('-');
        }
        else
        {
            var parts = stepInstructions.Split('=');
            Label = parts[0];
            FocalLength = int.Parse(parts[1]);
        }
    }
}

class Box
{
    public List<Lens> Lenses { get; private set; } = [];

    public int Measure()
    {
        if (Lenses.Count == 0) return 0;
        return Lenses
            .Select((lens, index) => lens.FocalLength * (index + 1))
            .Aggregate((a, b) => a + b);
    }

    public void ExecuteStep(Step step)
    {
        if (step.IsRemoval)
        {
            var lensIndex = Lenses.FindIndex(l => l.Label == step.Label);

            if (lensIndex != -1)
            {
                Lenses.RemoveAt(lensIndex);
            }
        }
        else
        {
            var lens = Lenses.Find(l => l.Label == step.Label);

            if (lens == null)
            {
                Lenses.Add(new Lens(step.Label, step.FocalLength));
            }
            else
            {
                lens.FocalLength = step.FocalLength;
            }
        }
    }
}

class Lens
{
    public string Label { get; private set; }
    public int FocalLength { get; set; }

    public Lens(string label, int focalLength)
    {
        Label = label;
        FocalLength = focalLength;
    }
}
