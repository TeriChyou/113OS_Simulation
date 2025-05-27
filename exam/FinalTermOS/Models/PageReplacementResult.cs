// Models/PageReplacementResult.cs
using System.Collections.Generic;

namespace FinalTermOS.Models
{
    public class PageReplacementResult
    {
        public string AlgorithmName { get; set; } // e.g., "FIFO", "LRU", "Optimal"
        public int PageFrameCount { get; set; } // Number of page frames used for this test
        public int PageFaults { get; set; } // Total page faults recorded
        public List<string> ExecutionLog { get; set; } = new List<string>(); // Detailed steps of this algorithm run
    }
}