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
    Console.WriteLine(start);
    Console.WriteLine(start.DoAction(actions));

    // Console.WriteLine(start.ToString());
    // Console.WriteLine();

    // foreach (Game.Action action in actions)
    // {
    //     start = start.DoAction(action);
    //     Console.WriteLine(start.ToString());
    //     Console.WriteLine();
    // }
}

void CheckRL() {
    QLearn rl = new QLearn(0.001, 0.9, 0.1, 25);


    rl.Train(100_000);

    for (var i  = 0; i < 3; ++i) {
        Console.WriteLine("Starting Episode");
        rl.PrintEpisode();
    }    
    Console.WriteLine(rl.Serialize());
}

// CheckActions();
CheckRL();