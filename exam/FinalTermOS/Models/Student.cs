
namespace FinalTermOS.Models
{
    public class Student
{
    public int Id { get; set; }
    public StudentState State { get; set; }
    // Add a ManualResetEventSlim for the student to wait on
    public ManualResetEventSlim WaitingSignal { get; }

    public Student(int id)
    {
        Id = id;
        State = StudentState.Thinking;
        // Initialize the event in a non-signaled state (等待狀態)
        WaitingSignal = new ManualResetEventSlim(false);
    }
}
}