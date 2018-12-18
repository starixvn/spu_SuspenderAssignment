using Animus.ANN.Optimizer.Reinforcement;
using SchedulingAPI.DTO.SuspenderAssignment;
using System.Collections.Generic;
using System.Linq;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class Environment : IEnvironment<Assignments, int>
    {
        private int matchDuration = 120;

        private readonly IDictionary<int, Match> matches;

        private readonly IDictionary<int, Suspender> suspenders;

        public Environment(IEnumerable<Match> matches, IEnumerable<Suspender> suspenders)
        {
            this.matches = matches.ToDictionary(m => m.MatchId);
            this.suspenders = suspenders.ToDictionary(s => s.SusId);
        }

        public Assignments CreateInitialState()
        {
            var matchQueue = new Queue<int>(matches.Values.OrderByDescending(m => m.MaxBet).Select(m => m.MatchId));

            var firstMatch = matchQueue.Dequeue();

            var bettypes = new Queue<Bettype>(matches[firstMatch].PickedBettypes.Where(bt => bt != Bettype.Main));

            return new Assignments
            {
                MatchQueue = matchQueue,
                CurrentMatch = matches[firstMatch],
                CurrentBettype = Bettype.Main,
                AssignedSuspenderMatch = new Dictionary<int, HashSet<int>>(),
                BettypeQueue = bettypes,
                AssignedSuspenderBettype = new Dictionary<int, HashSet<Bettype>>()
            };
        }

        public IEnumerable<int> GetActions(Assignments state)
        {
            yield return -1;

            foreach (var sus in suspenders.Values)
            {
                if (IsBigMatch(state.CurrentMatch.MatchId) && BigMatchFilter(sus, state))
                {
                    yield return sus.SusId;
                }
                else if (NormalMatchFilter(sus, state))
                {
                    yield return sus.SusId;
                }
            }
        }

        public bool IsTerminalState(Assignments state)
        {
            return state.CurrentMatch == null;
        }

        public Assignments NextState(Assignments state, int action)
        {
            var newState = state.Clone();

            if (newState.AssignedSuspenderMatch.ContainsKey(action))
            {
                newState.AssignedSuspenderMatch[action].Add(state.CurrentMatch.MatchId);
            }
            else if (action != -1)
            {
                newState.AssignedSuspenderMatch.Add(action, new HashSet<int> { state.CurrentMatch.MatchId });
            }

            if (newState.AssignedSuspenderBettype.ContainsKey(action))
            {
                newState.AssignedSuspenderBettype[action].Add(state.CurrentBettype);
            }
            else if (action != -1)
            {
                newState.AssignedSuspenderBettype.Add(action, new HashSet<Bettype> { state.CurrentBettype });
            }

            if (newState.BettypeQueue.Count == 0)
            {
                if (newState.MatchQueue.Count == 0)
                {
                    newState = null;
                }
                else
                {
                    var next_match = matches[newState.MatchQueue.Dequeue()];
                    var bettypes = new Queue<Bettype>(next_match.PickedBettypes.Where(bt => bt != Bettype.Main));

                    newState.CurrentMatch = next_match;
                    newState.BettypeQueue = bettypes;
                    newState.CurrentBettype = Bettype.Main;
                    newState.AssignedSuspenderBettype = new Dictionary<int, HashSet<Bettype>>();
                }
            }
            else
            {
                newState.CurrentBettype = newState.BettypeQueue.Dequeue();
            }

            return newState;
        }

        private bool IsBigMatch(int match)
        {
            return matches[match].MaxBet >= 180;
        }

        private bool InWorkHour(Suspender suspender, Assignments state)
        {
            var ko = state.CurrentMatch.KOTime;

            var end = ko.AddMinutes(matchDuration);

            return suspender.InTime <= ko && end <= suspender.OutTime;
        }

        private bool IsBusy(Suspender suspender, Assignments state)
        {
            var lowerBound = state.CurrentMatch.KOTime.AddMinutes(-matchDuration);
            var upperBound = state.CurrentMatch.KOTime.AddMinutes(matchDuration);

            return state.AssignedSuspenderMatch.ContainsKey(suspender.SusId) &&
                state.AssignedSuspenderMatch[suspender.SusId].Where(match => match != state.CurrentMatch.MatchId)
                .Any(match =>
                {
                    var ko = matches[match].KOTime;

                    return lowerBound < ko && ko < upperBound;
                });
        }

        private bool BigMatchFilter(Suspender suspender, Assignments state)
        {
            return InWorkHour(suspender, state)
                && !IsBusy(suspender, state)
                && !state.AssignedSuspenderBettype.ContainsKey(suspender.SusId)
                && (state.CurrentBettype == Bettype.Main ? suspender.Lvl <= 1 : suspender.Lvl <= 2)
                && (state.CurrentMatch.RequireChinese ? suspender.SpeakChinese : true)
                && (state.CurrentMatch.HasTV ? suspender.InLineWithTV : true);
        }

        private bool NormalMatchFilter(Suspender suspender, Assignments state)
        {
            return InWorkHour(suspender, state)
                && !IsBusy(suspender, state)
                && (!state.AssignedSuspenderBettype.ContainsKey(suspender.SusId)
                        || (suspender.Lvl == 0 && state.AssignedSuspenderBettype[suspender.SusId].Count <= 1))
                && (state.CurrentMatch.RequireChinese ? suspender.SpeakChinese : true)
                && (state.CurrentMatch.HasTV ? suspender.InLineWithTV && suspender.Lvl <= 3 : true)
                && (state.CurrentMatch.HasReporter || state.CurrentMatch.Provider == Provider.KOSPT || state.CurrentMatch.Provider == Provider.RB2 ?
                        suspender.Lvl <= 4 : true)
                && (state.CurrentBettype == Bettype.Corner ? suspender.Lvl <= 2 : true);
        }
    }
}