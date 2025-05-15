// Models/StudentStatusDto.cs
using FinalTermOS.Models; // 如果 StudentState 在 Models 裡

namespace FinalTermOS.Models
{
    // Data Transfer Object for sending student status to frontend
    public class StudentStatusDto
    {
        public int Id { get; set; }
        public StudentState State { get; set; }
        // Do NOT include the WaitingSignal here
    }
}