var maps = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\maps.txt"));

var map = new Map();
map.ParseLines(maps);
map.ProcessNodes();
var count = map.CountSteps();

Console.WriteLine(count);

class Map
{
    public string Directions { get; private set; }
    public List<Node> Nodes { get; private set; }
    public Node Start { get; private set; }
    public Node End { get; private set; }

    public void ParseLines(string[] lines)
    {
        Directions = lines[0];
        Nodes = [];

        foreach (var line in lines.Skip(2))
        {
            var elements = line.Split('=', StringSplitOptions.TrimEntries);
            var directions = elements[1]
                .TrimStart('(').TrimEnd(')')
                .Split(',', StringSplitOptions.TrimEntries);

            Nodes.Add(new Node(elements[0], directions[0], directions[1]));
        }
    }

    public void ProcessNodes()
    {
        Start = Nodes.Find(n => n.Code == "AAA");
        ProcessNode(Start);
    }

    private void ProcessNode(Node node)
    {
        if (node.LeftNode != null) return;
        node.LeftNode = Nodes.Find(n => n.Code == node.LeftCode);
        node.RightNode = Nodes.Find(n => n.Code == node.RightCode);
        ProcessNode(node.LeftNode);
        ProcessNode(node.RightNode);
    }

    public int CountSteps()
    {
        var stepCount = 0;
        var current = Start;

        End = Nodes.Find(n => n.Code == "ZZZ");

        while (current != End)
        {
            for (var index = 0; index < Directions.Length; index++)
            {
                switch (Directions[index])
                {
                    case 'L':
                        current = current.LeftNode; break;
                    case 'R':
                        current = current.RightNode; break;
                    default:
                        throw new Exception("cannot parse directions");
                }

                stepCount++;
            }
        }

        return stepCount;
    }
}

class Node
{
    public Node LeftNode { get; set; }
    public Node RightNode { get; set; }
    public string Code { get; private set; }
    public string LeftCode { get; private set; }
    public string RightCode { get; private set; }

    public Node(string code, string leftCode, string rightCode)
    {
        Code = code;
        LeftCode = leftCode;
        RightCode = rightCode;
    }
}
