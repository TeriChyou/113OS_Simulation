// Models/SleepingTASimulationStatus.cs
using System.Collections.Generic;
using FinalTermOS.Models;

namespace FinalTermOS.Models
{
    // Status object sent to the frontend
    public class SleepingTASimulationStatus
    {
        public TAState TAState { get; set; }
        // Use the DTO for students
        public List<StudentStatusDto> Students { get; set; }
        public int WaitingQueueCount { get; set; }
        public int AvailableChairs { get; set; }
    }
}