using SchedulingAPI.DTO.SuspenderAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class PolicyModifier
    {
        IDictionary<(int matchId, Bettype bettype), string> modifier;

        public PolicyModifier(IEnumerable<AssignmentStep> steps)
        {
            modifier = steps.ToDictionary(a => (a.Matchid, a.Bettype), a => a.SusId);
        }

        public bool HasModifier(int matchId, Bettype bt)
        {
            return modifier.ContainsKey((matchId, bt));
        }

        public string Modify(int matchId, Bettype bt)
        {
            return modifier[(matchId, bt)];
        }
    }
}