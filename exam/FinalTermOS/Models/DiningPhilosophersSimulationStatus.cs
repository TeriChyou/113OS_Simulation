// Models/DiningPhilosophersSimulationStatus.cs
using System.Collections.Generic;
using FinalTermOS.Models; // Needed for PhilosopherState

namespace FinalTermOS.Models
{
    // DTO for a single philosopher's status to be sent to frontend
    public class PhilosopherStatusDto
    {
        public int Id { get; set; }
        public PhilosopherState State { get; set; } // Send the enum value directly
        // You can add more info here if needed for display, e.g., current forks held
    }

    // DTO for a single fork's status to be sent to frontend
    public class ForkStatusDto
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
        // You can add which philosopher holds it if occupied for more detailed display
    }

    // Main DTO for the entire simulation status
    public class DiningPhilosophersSimulationStatus
    {
        public bool IsRunning { get; set; } // To tell frontend if simulation is active
        public string Message { get; set; } // General status message
        public bool IsPaused { get; set; }

        public List<PhilosopherStatusDto> Philosophers { get; set; }
        public List<ForkStatusDto> Forks { get; set; }
        public List<string> LogMessages { get; set; }

        public DiningPhilosophersSimulationStatus()
        {
            Philosophers = new List<PhilosopherStatusDto>();
            Forks = new List<ForkStatusDto>();
            LogMessages = new List<string>();
        }
    }
}