using Animus.ANN;
using Animus.ANN.Optimizer.Reinforcement;
using SchedulingAPI.DTO.SuspenderAssignment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class AssignmentModel
    {
        public Environment Env { get; set; }

        public Task<IModel<Assignments, IEnumerable<ActionScore<int>>>> Model { get; set; }
    }
}