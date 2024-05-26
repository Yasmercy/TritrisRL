using Game;
using RL;

void CheckActions() {
    Game.Action[] actions = {
    Game.Action.Left,
    Game.Action.Left,
    Game.Action.Drop,
    Game.Action.Right,
    Game.Action.Drop,
    Game.Action.Left,
    Game.Action.Drop,
    Game.Action.Right,
    Game.Action.Drop,
};

    var start = new State();
    Console.WriteLine(start.ToString());
    Console.WriteLine();

    foreach (Game.Action action in actions)
    {
        start = start.DoAction(action) ?? start;
        Console.WriteLine(start.ToString());
        Console.WriteLine();
    }

}

void CheckRL() {
    QLearn rl = new QLearn(0.001, 0.9, 0.1, Enumerable.Repeat<double>(0, Globals.SIZE + 8).ToArray(), 20);

    rl.Train(1_000_000);

    for (var i  = 0; i < 3; ++i) {
        Console.WriteLine("Starting Episode");
        rl.PrintEpisode();
    }    
    Console.WriteLine(rl.Serialize());
}

CheckRL();