
namespace FinalTermOS.Models
{
    public class Student // 如果已在 Models 資料夾獨立創建，這裡可以移除或使用 using
    {
        public int Id { get; set; }
        public StudentState State { get; set; }
        // 可能需要其他屬性，例如當前等待的次數等

        public Student(int id)
        {
            Id = id;
            State = StudentState.Thinking;
        }
    }
}