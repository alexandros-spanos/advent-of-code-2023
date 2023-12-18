var recordsLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\records.txt"));
var sum = 0;
var index = 0;

foreach (var recordLine in recordsLines)
{
    var record = new Record();
    record.Initialize(recordLine);
    var ways = record.CountWays();
    sum += ways;
    Console.WriteLine($"{++index}) {recordLine} -> {ways}");
}

Console.WriteLine(sum);

class Record
{
    public List<RecordBlock> Blocks { get; private set; }
    public List<int> GroupSizes { get; private set; }
    public List<Scenario> Scenarios { get; private set; }

    public void Initialize(string line)
    {
        ParseLine(line);
        CleanUp();
        Scenarios = [];
    }

    private void ParseLine(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.TrimEntries);

        Blocks = parts[0]
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => new RecordBlock(p)).ToList();

        GroupSizes = parts[1].Split(',')
            .Select(int.Parse).ToList();
    }

    private void CleanUp()
    {
        var initialGroupSize = GroupSizes[0];

        while (Blocks[0].Sequence.Length < initialGroupSize)
        {
            Blocks.RemoveAt(0);
        }
    }

    public int CountWays()
    {
        var result = 0;
        FindPossibleScenarios(CreateScenario(), 0, 0);

        foreach (var scenario in Scenarios)
        {
            var scenarioInvalid = false;
            var scenarioCombinations = new List<int>(Blocks.Count);

            for (var blockIndex = 0; blockIndex < Blocks.Count; blockIndex++)
            {
                var groupSizes = scenario.GetGroupsInBlock(blockIndex).ConvertAll(g => GroupSizes[g]);
                var block = Blocks[blockIndex];

                if (groupSizes.Count > 0)
                {
                    var blockCombinations = block.CountCombinations(groupSizes);

                    if (blockCombinations > 0)
                    {
                        scenarioCombinations.Add(blockCombinations);
                    }
                    else
                    {
                        scenarioInvalid = true;
                        break;
                    }
                }
                else if (!block.IsQuestionable())
                {
                    scenarioInvalid = true;
                    break;
                }
            }

            if (scenarioInvalid)
            {
                continue;
            }

            result += scenarioCombinations.Aggregate((a, b) => a * b);
        }

        return result;
    }

    private void FindPossibleScenarios(Scenario scenario, int fillStartGroupIndex, int fillStartBlockIndex)
    {
        if (fillStartGroupIndex == GroupSizes.Count)
        {
            if (IsValid(scenario))
            {
                Scenarios.Add(scenario);
            }
            return;
        }

        FillScenario(scenario, fillStartBlockIndex, fillStartGroupIndex);

        if (IsValid(scenario))
        {
            if (fillStartBlockIndex < Blocks.Count - 1)
            {
                var groupIds = scenario.GetGroupsInBlock(fillStartBlockIndex);

                for (var groupIndex = 0; groupIndex < groupIds.Count; groupIndex++)
                {
                    var groupId = groupIds[groupIndex];
                    var subScenario = CreateScenario(scenario, groupId);

                    FindPossibleScenarios(subScenario, groupId + 1, fillStartBlockIndex + 1);
                }

                Scenario lastSubScenario;
                if (fillStartGroupIndex == 0)
                {
                    lastSubScenario = CreateScenario();
                }
                else
                {
                    lastSubScenario = CreateScenario(scenario, fillStartGroupIndex - 1);
                }
                FindPossibleScenarios(lastSubScenario, fillStartGroupIndex, fillStartBlockIndex + 1);
            }
            else
            {
                Scenarios.Add(scenario);
            }
        }
    }

    private void FillScenario(Scenario scenario, int blockIndex = 0, int groupIndex = 0)
    {
        for (; blockIndex < Blocks.Count; blockIndex++)
        {
            var block = Blocks[blockIndex];

            for (; groupIndex < GroupSizes.Count; groupIndex++)
            {
                var groupSize = GroupSizes[groupIndex];

                if (block.Sequence.Length - SizeTaken(blockIndex, scenario) >= groupSize)
                {
                    scenario.GroupsToBlocksMap.Add(blockIndex);
                }
                else break;
            }
        }
    }

    private int SizeTaken(int blockIndex, Scenario scenario)
    {
        var groupSizes = scenario
            .GetGroupsInBlock(blockIndex)
            .Select(g => GroupSizes[g])
            .ToList();

        if (groupSizes.Count > 0)
        {
            return groupSizes.Sum() + groupSizes.Count;
        }
        return 0;
    }

    private bool IsValid(Scenario scenario)
    {
        return scenario.GroupsToBlocksMap.Count == GroupSizes.Count;
    }

    private Scenario CreateScenario(Scenario scenario, int groupIndex)
    {
        groupIndex++;
        var tempArray = new int[groupIndex];
        scenario.GroupsToBlocksMap.CopyTo(0, tempArray, 0, groupIndex);

        var nextScenario = CreateScenario();
        nextScenario.GroupsToBlocksMap = tempArray.ToList();
        return nextScenario;
    }

    private Scenario CreateScenario()
    {
        return new Scenario(GroupSizes.Count);
    }
}

class Scenario
{
    public List<int> GroupsToBlocksMap { get; set; }

    public Scenario(int length)
    {
        GroupsToBlocksMap = new List<int>(length);
    }

    public List<int> GetGroupsInBlock(int blockIndex)
    {
        var groupIds = new List<int>();

        for (var groupIndex = 0; groupIndex < GroupsToBlocksMap.Count; groupIndex++)
        {
            var blockId = GroupsToBlocksMap[groupIndex];

            if (blockId == blockIndex)
            {
                groupIds.Add(groupIndex);
            }
            else if (groupIds.Count > 0)
            {
                break;
            }
        }

        return groupIds;
    }
}

class RecordBlock
{
    public char[] Sequence { get; set; }

    public RecordBlock(string line)
    {
        Sequence = line.ToCharArray();
    }

    public int CountCombinations(List<int> groupSizes, char[] sequence = null)
    {
        if (sequence == null)
        {
            sequence = Sequence;
        }
        var combinations = 0;

        if (groupSizes.Count > 0)
        {
            var maxMargin = sequence.Length - (groupSizes.Sum() + groupSizes.Count - 1);

            for (var margin = 0; margin <= maxMargin; margin++)
            {
                var groupEnd = margin + groupSizes[0];
                var isValid = IsQuestionable(sequence, 0, margin);

                if (groupEnd + 1 <= sequence.Length)
                {
                    isValid &= IsQuestionable(sequence, groupEnd, ++groupEnd);
                }

                if (isValid)
                {
                    if (groupSizes.Count == 1)
                    {
                        if (IsQuestionable(sequence, groupEnd, sequence.Length))
                        {
                            combinations++;
                        }
                    }
                    else
                    {
                        var subSequence = sequence.Skip(groupEnd).ToArray();
                        combinations += CountCombinations(groupSizes.Skip(1).ToList(), subSequence);
                    }
                }
            }
        }
        return combinations;
    }

    public bool IsQuestionable()
    {
        return IsQuestionable(Sequence, 0, Sequence.Length);
    }

    private static bool IsQuestionable(char[] sequence, int startIndex, int endIndex)
    {
        for (; startIndex < endIndex; startIndex++)
        {
            if (sequence[startIndex] != '?') return false;
        }
        return true;
    }
}
