using Game;

namespace RL;
public record QLearn(double alpha, double gamma, double epsilon, double[] w, int max_game_len)
{
    public QLearn(double alpha, double gamma, double epsilon, int max_game_len) : 
        this(alpha, gamma, epsilon, new double[Globals.SIZE + 6], max_game_len) {}

    #region StaticMembers
    static readonly Game.Action[][] actions = [
        [Game.Action.Drop],
        [Game.Action.Right, Game.Action.Drop],
        [Game.Action.Left, Game.Action.Drop],
        [Game.Action.Left, Game.Action.Left, Game.Action.Drop],
    ];
    static readonly int LoseReward = -10;
    static double Reward(State state, State next) {
        int [] rewards = [0, 1, 4, 16];
        int diffPieces = state.Board.Sum(x => x ? 1 : 0) - next.Board.Sum(x => x ? 1 : 0);
        return rewards[(3 + diffPieces) / Globals.NUM_COLS];
    }
    #endregion

    #region Helpers
    double Eval(State state) => 
        state.IsTerminal() ? LoseReward : state.FeatureVector().Zip(w, (a, b) => a * b).Sum();

    void Update(State state, State? next)
    {
        double gt = (next == null) ? 0 : 
            Reward(state, next) + NextActionValues(next).Max();
        double scalar = (gt - Eval(state)) * alpha;

        double[] xs = state.FeatureVector();
        for (var i = 0; i < xs.Length; ++i)
            w[i] += scalar * xs[i];
    }

    IEnumerable<double> NextActionValues(State cur) =>
        from Game.Action[] action in actions
        select Eval(cur.DoAction(action));

    Game.Action[] GetAction(State cur) {
        double[] values = NextActionValues(cur).ToArray();
        double max = values.Max();

        if (values[0] == max) return actions[0];
        if (values[1] == max) return actions[1];
        if (values[2] == max) return actions[2];
        return actions[3];
    }

    Game.Action[] GetActionEpsilon(State cur) {
        // with epsilon chance, do uniform random
        // otherwise do best
        Random rand = new Random();
        return (rand.NextDouble() < epsilon) 
        ? actions[rand.Next(0, 4)] : GetAction(cur);
    }
    #endregion

    #region Training
    void TrainIteration() {
        var state = new State();
        for (var t = 0; t < max_game_len; ++t) 
        {
            Game.Action[] action = GetActionEpsilon(state);
            State next = state.DoAction(action);
            Update(state, next);

            if (next.IsTerminal())
                break;
            state = next;
        }
    }

    public void Train(int num_iterations)
    {
        for (var i = 0; i < 100; ++i)
        {
            Console.WriteLine($"{i}% done");
            for (var j = 0; j < (i + 1) * num_iterations / 100; ++j)
                TrainIteration();
        }
        Console.WriteLine("100% done");
    }

    public void PrintEpisode() {
        var state = new State();
        for (var t = 0; t < max_game_len; ++t) 
        {
            Game.Action[] action = GetAction(state);
            State next = state.DoAction(action);
            Console.WriteLine($"{state}\n");

            if (next.IsTerminal()) {
                Console.WriteLine(state.DoAction(action));
                break;
            }
            state = next;
        }
    }
    #endregion

    public String Serialize() => $"[{String.Join(", ", w)}]";

}