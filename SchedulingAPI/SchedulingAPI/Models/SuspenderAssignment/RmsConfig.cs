using Animus.ANN.Optimizer.Reinforcement;
using System;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class RmsConfig : Rms
    {
        private int maxGame = 5000;

        protected override double LearningRate(int game, int turn)
        {
            return game < maxGame / 2 ? 1e-5 : 1e-5 / (game - maxGame / 2 + 1);
        }
    }
}