// Models/BankersAlgorithmStatus.cs
using System.Collections.Generic;

namespace FinalTermOS.Models
{
    // DTO for a single process's relevant data to be sent to frontend
    public class ProcessStatusDto
    {
        public int Id { get; set; }
        public string Max { get; set; } // String representation of ResourceVector
        public string Allocation { get; set; } // String representation of ResourceVector
        public string Need { get; set; } // String representation of ResourceVector
        public bool IsFinished { get; set; } // To indicate if process has finished in a sequence
    }

    // Main DTO for the entire Banker's Algorithm simulation status
    public class BankersAlgorithmStatus
    {
        public bool IsRunning { get; set; } // To indicate if simulation is active (though Banker's is static)
        public string Message { get; set; } // General status message

        public List<ProcessStatusDto> Processes { get; set; }
        public string Available { get; set; } // String representation of Available resources

        public List<string> SafeSequences { get; set; } // List of found safe sequences (e.g., "P1 -> P3 -> P2 -> ...")
        public string RequestP1Result { get; set; } // Result of P1's request
        public string RequestP4Result { get; set; } // Result of P4's request

        public List<string> LogMessages { get; set; } // For algorithm steps/output

        public BankersAlgorithmStatus()
        {
            Processes = new List<ProcessStatusDto>();
            SafeSequences = new List<string>();
            LogMessages = new List<string>();
        }
    }
}