using Animus.ANN.Optimizer.Reinforcement;
using SchedulingAPI.DTO.SuspenderAssignment;
using System.Collections.Generic;
using System.Linq;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class Controller : A3C<Assignments, int>
    {
        private int maxGame = 500;

        private IDictionary<int, int> susIndexer;

        private double maxBet;

        private IDictionary<int, Match> matchDict;

        private IDictionary<int, Suspender> susDict;

        public Controller(IEnumerable<Match> matches, IEnumerable<Suspender> suspenders)
        {
            matchDict = matches.ToDictionary(m => m.MatchId);
            susDict = suspenders.ToDictionary(s => s.SusId);
            maxBet = matches.Max(m => m.MaxBet);
            susIndexer = suspenders.Select((s, i) => new KeyValuePair<int, int>(s.SusId, i)).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public override double Discount => 0.99;

        public override int Threads => 1;

        public override IAsyncOptimizer Optimizer => new RmsConfig();

        public override int ActionIndex(int action) => action == -1 ? 0 : susIndexer[action] + 1;

        public override double Reward(Assignments state, int action, Assignments nextState)
        {
            var score = -state.CurrentMatch.MaxBet / maxBet;
            var minors = state.CurrentMatch.PickedBettypes.Count - 1;

            if (action == -1)
            {
                if (state.CurrentBettype == Bettype.Main)
                {
                    return 0.5 * score;
                }
                else
                {
                    return 0.5 * score / minors;
                }
            }
            else
            {
                var lvl = susDict[action].Lvl;

                if (state.CurrentBettype == Bettype.Main)
                {
                    return 0.5 * score * lvl / 5;
                }
                else
                {
                    return 0.5 * score * lvl / 5 / minors;
                }
            }
        }

        public override bool StopAndUpdate(int game, int turn) => turn % 100 == 0;

        public override bool Terminate(int thread, int game) => game == maxGame;
    }
}