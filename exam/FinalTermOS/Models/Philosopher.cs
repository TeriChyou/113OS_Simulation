// Models/Philosopher.cs
using System.Threading; // Needed for ManualResetEventSlim
using FinalTermOS.Models; // Needed for PhilosopherState

namespace FinalTermOS.Models
{
    public class Philosopher
    {
        public int Id { get; set; }
        public PhilosopherState State { get; set; }

        // Each philosopher needs a signal to wake them up when conditions are met (e.g., forks available)
        public ManualResetEventSlim CanEatSignal { get; }

        public Philosopher(int id)
        {
            Id = id;
            State = PhilosopherState.Thinking;
            CanEatSignal = new ManualResetEventSlim(false); // Initially not signaled
        }
    }
}