using System;

namespace SchedulingAPI.DTO.SuspenderAssignment
{
    public class Suspender
    {
        public int Lvl { get; set; }

        public bool SpeakChinese { get; set; }

        public DateTime InTime { get; set; }

        public DateTime OutTime { get; set; }

        public bool InLineWithTV { get; set; }

        public string SusId { get; set; }
    }
}