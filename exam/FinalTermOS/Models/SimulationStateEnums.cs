// Models/SimulationStateEnums.cs
namespace FinalTermOS.Models
{
    public enum TAState
    {
        Sleeping,
        HelpingStudent,
        CheckingForStudents // TA 幫助完學生後檢查走廊
    }

    public enum StudentState
    {
        Thinking,       // 學生在做自己的事
        SeekingHelp,    // 學生決定尋求 TA 幫助，正前往辦公室
        WaitingInChair, // 學生坐在走廊的椅子上等待
        GettingHelp,    // 學生正在被 TA 幫助
        Leaving,         // 學生離開辦公室（無論是還是椅子滿了
        Finished // 學生得到幫助
    }
}