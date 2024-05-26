using System.Resources;
using System.Security.Authentication;
using Game;

namespace RL;
public record QLearn(double alpha, double gamma, double epsilon, double[] w, int max_game_len)
{
    static int LoseReward = -10;
    double Reward(State state, State next) {
        int [] rewards = [0, 1, 4, 8];
        int diffPieces = state.Board.Sum(x => x ? 1 : 0) - next.Board.Sum(x => x ? 1 : 0);
        int placed = (diffPieces != 0) ? 1 : 0;
        return placed * (0.1 + rewards[(3 + diffPieces) / Globals.NUM_COLS]);
    }

    double Eval(State? state) => 
        state == null ? LoseReward : state.FeatureVector().Zip(w, (a, b) => a * b).Sum();

    void Update(State state, Game.Action action, State? next)
    {
        double gt = (next == null) ? 0 : 
            Reward(state, next) + NextActionValues(next).Max();
        double scalar = (gt - Eval(state)) * alpha;

        double[] xs = state.FeatureVector();
        for (var i = 0; i < 32; ++i)
            w[i] += scalar * xs[i];
    }

    IEnumerable<double> NextActionValues(State cur) =>
        from Game.Action action in Enum.GetValues(typeof(Game.Action))
        select Eval(cur.DoAction(action)!);

    Game.Action GetAction(State cur) {
        double[] values = NextActionValues(cur).ToArray();
        double max = values.Max();

        if (values[0] == max) return (Game.Action) 0;
        if (values[1] == max) return (Game.Action) 1;
        return (Game.Action) 2;
    }

    Game.Action GetActionEpsilon(State cur) {
        // with epsilon chance, do uniform random
        // otherwise do best
        Random rand = new Random();
        return (rand.NextDouble() < epsilon) 
        ? (Game.Action) rand.Next(0, 2) : GetAction(cur);
    }

    void TrainIteration() {
        var state = new State();
        for (var t = 0; t < max_game_len; ++t) 
        {
            Game.Action action = GetActionEpsilon(state);
            State? next = state.DoAction(action);
            Update(state, action, next);

            if (next == null)
                break;
            state = next;
        }
    }

    public void Train(int num_iterations)
    {
        for (var i = 0; i < num_iterations; ++i)
            TrainIteration();
    }

    public void PrintEpisode() {
        var state = new State();
        for (var t = 0; t < max_game_len; ++t) 
        {
            Game.Action action = GetAction(state);
            State? next = state.DoAction(action);
            Console.WriteLine($"{state}\n");

            if (next == null)
                break;
            state = next;
        }
    }

    public String Serialize() => $"[{String.Join(", ", w)}]";

}