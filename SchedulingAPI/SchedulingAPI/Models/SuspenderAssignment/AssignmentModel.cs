using Animus.ANN;
using Animus.ANN.Optimizer.Reinforcement;
using SchedulingAPI.DTO.SuspenderAssignment;
using System.Collections.Generic;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class AssignmentModel
    {
        public Environment Env { get; set; }

        public IModel<Assignments, IEnumerable<ActionScore<int>>> Model { get; set; }
    }
}