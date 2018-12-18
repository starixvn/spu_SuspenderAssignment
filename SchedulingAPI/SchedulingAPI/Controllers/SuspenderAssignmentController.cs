using Animus.ANN;
using Animus.ANN.Core;
using Animus.ANN.Optimizer.Reinforcement;
using Animus.ANN.Types;
using Animus.ANN.Types.Layers;
using Animus.Tensor;
using SchedulingAPI.DTO.SuspenderAssignment;
using SchedulingAPI.Models.SuspenderAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SchedulingAPI.Controllers
{
    public class SuspenderAssignmentController : ApiController
    {
        static private IDictionary<string, AssignmentModel> savedEnvModels = new Dictionary<string, AssignmentModel>();

        static private IDictionary<string, Timer> resourceRelease = new Dictionary<string, Timer>();

        [HttpPost]
        public string AssignSuspender(IEnumerable<Match> matches, IEnumerable<Suspender> suspenders)
        {
            var id = Guid.NewGuid().ToString();

            var env = new Models.SuspenderAssignment.Environment(matches, suspenders);
            var ctrller = new Controller(matches, suspenders);
            var agent = CreateAgent(matches, suspenders);

            var policyTsk = ctrller.Train(env, agent);

            savedEnvModels.Add(id, new AssignmentModel { Env = env, Model = policyTsk });

            policyTsk.Start();

            return id;
        }

        [HttpPost]
        public object CheckStatus(string id)
        {
            if (savedEnvModels.ContainsKey(id))
            {
                switch (savedEnvModels[id].Model.Status)
                {
                    case TaskStatus.WaitingToRun:
                    case TaskStatus.Running:
                    case TaskStatus.WaitingForChildrenToComplete:
                        return "running";

                    case TaskStatus.RanToCompletion:
                        var env = savedEnvModels[id].Env;
                        var model = savedEnvModels[id].Model.Result;

                        var duetime = (DateTime.Now.AddDays(1) - DateTime.Now).TotalMilliseconds;

                        var timer = new Timer(_ =>
                        {
                            savedEnvModels.Remove(id);
                            resourceRelease.Remove(id);
                        }, null, (long)duetime, Timeout.Infinite);

                        resourceRelease.Add(id, timer);

                        return Solve(env.CreateInitialState(), env, model);

                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        return "failed";

                    default:
                        return savedEnvModels[id].Model.Status.ToString();
                }
            }

            throw new InvalidOperationException($"invalid id {id}");
        }

        [HttpPost]
        public AssignmentResult ChangeAssignment(string id, IEnumerable<AssignmentStep> steps)
        {
            if (savedEnvModels.ContainsKey(id))
            {
                var envModel = savedEnvModels[id];

                var modifier = new PolicyModifier(steps);

                return new AssignmentResult
                {
                    Id = id,
                    Steps = Solve(envModel.Env.CreateInitialState(), envModel.Env, envModel.Model.Result, modifier)
                };
            }

            throw new InvalidOperationException($"No model with id {id}");
        }

        private static INN<Assignments, Vector> CreateAgent(IEnumerable<Match> matches, IEnumerable<Suspender> suspenders)
        {
            var count = new DataCount
            {
                MatchCount = matches.Count(),
                SuspenderCount = suspenders.Count(),
                BettypeCount = Enum.GetValues(typeof(Bettype)).Length
            };

            var transformer = CreateTransformer(matches, suspenders, count);

            return new FeedForwardNet<Assignments>(
                    inputLayer: LayerFactory.MakeInputLayer(transformer, count.Total),
                    hiddenLayers: new[] { 128, 64 }.Select(LayerFactory.MakeTanhLayer),
                    outputLayer: LayerFactory.MakeLinearLayer(2 + suspenders.Count())
                );
        }

        private static Func<Assignments, Vector> CreateTransformer(IEnumerable<Match> matches, IEnumerable<Suspender> suspenders, DataCount count)
        {
            var matchIdx = matches.Select((m, i) => Tuple.Create(m.MatchId, i)).ToDictionary(t => t.Item1, t => t.Item2);
            var susIdx = suspenders.Select((s, i) => Tuple.Create(s.SusId, i)).ToDictionary(t => t.Item1, t => t.Item2);

            return state => Transform(state, matchIdx, susIdx, count);
        }

        private static Vector Transform(Assignments state, IDictionary<int, int> matchIdx, IDictionary<int, int> susIdx, DataCount count)
        {
            var arr = Enumerable.Range(0, count.Total).Select(_ => 0.0).ToArray();

            var idx = 0;

            foreach (var kv in state.AssignedSuspenderMatch)
            {
                var sus = susIdx[kv.Key];

                foreach (var m in kv.Value)
                {
                    var match = matchIdx[m];

                    arr[idx + sus * count.MatchCount + match] = 1;
                }
            }

            idx += count.SuspenderCount * count.MatchCount;

            foreach (var kv in state.AssignedSuspenderBettype)
            {
                var sus = susIdx[kv.Key];

                foreach (var bt in kv.Value)
                {
                    arr[idx + sus * count.BettypeCount + (int)bt] = 1;
                }
            }

            idx += count.SuspenderCount * count.BettypeCount;

            arr[idx + matchIdx[state.CurrentMatch.MatchId]] = 1;
            arr[idx + count.MatchCount + (int)state.CurrentBettype] = 1;

            return new Vector(arr);
        }

        private static IEnumerable<AssignmentStep> Solve(Assignments state, Models.SuspenderAssignment.Environment env, IModel<Assignments, IEnumerable<ActionScore<int>>> model, PolicyModifier modifier = null)
        {
            while (!env.IsTerminalState(state))
            {
                var actions = model.Compute(state);

                var pickedAction = 0;

                if (modifier != null && modifier.HasModifier(state.CurrentMatch.MatchId, state.CurrentBettype))
                {
                    pickedAction = modifier.Modify(state.CurrentMatch.MatchId, state.CurrentBettype);
                }
                else
                {
                    var score = double.NegativeInfinity;

                    foreach (var action in actions)
                    {
                        if (action.Score > score)
                        {
                            pickedAction = action.Action;
                            score = action.Score;
                        }
                    }
                }

                yield return new AssignmentStep
                {
                    Matchid = state.CurrentMatch.MatchId,
                    Bettype = state.CurrentBettype,
                    SusId = pickedAction
                };

                state = env.NextState(state, pickedAction);
            }
        }
    }
}