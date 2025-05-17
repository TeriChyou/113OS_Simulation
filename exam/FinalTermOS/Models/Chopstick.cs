// Models/Chopstick.cs
namespace FinalTermOS.Models
{
    public class Chopstick
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }

        public Chopstick(int id)
        {
            Id = id;
            IsAvailable = true; // Initially available
        }
    }
}