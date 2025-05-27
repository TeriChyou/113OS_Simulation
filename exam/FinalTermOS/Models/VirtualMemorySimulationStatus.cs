// Models/VirtualMemorySimulationStatus.cs
using System.Collections.Generic;

namespace FinalTermOS.Models
{
    public class VirtualMemorySimulationStatus
    {
        public List<int> PageReferenceString { get; set; } // The random page reference string used
        public List<PageReplacementResult> Results { get; set; } // Results for each algorithm and frame count

        public bool IsRunning { get; set; } // To indicate if calculation is in progress
        public string Message { get; set; } // General status message
        public List<string> LogMessages { get; set; } // General algorithm execution log (high-level)

        public VirtualMemorySimulationStatus()
        {
            PageReferenceString = new List<int>();
            Results = new List<PageReplacementResult>();
            LogMessages = new List<string>();
        }
    }
}