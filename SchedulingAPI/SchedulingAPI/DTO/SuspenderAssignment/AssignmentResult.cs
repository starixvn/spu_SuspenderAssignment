using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchedulingAPI.DTO.SuspenderAssignment
{
    public class AssignmentResult
    {
        public string Id { get; set; }

        public IEnumerable<AssignmentStep> Steps { get; set; }
    }
}