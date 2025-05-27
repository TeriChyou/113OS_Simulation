// Models/ProcessData.cs
namespace FinalTermOS.Models
{
    public class ProcessData
    {
        public int Id { get; set; } // Process ID (e.g., 1 for P1, 2 for P2)
        public ResourceVector Max { get; set; } // Maximum demand
        public ResourceVector Allocation { get; set; } // Currently allocated resources

        // Need is calculated: Max - Allocation
        public ResourceVector Need => Max - Allocation;

        public ProcessData(int id, ResourceVector max, ResourceVector allocation)
        {
            Id = id;
            Max = max;
            Allocation = allocation;
        }
    }
}