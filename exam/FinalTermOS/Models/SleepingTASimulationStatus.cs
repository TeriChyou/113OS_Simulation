// Models/SleepingTASimulationStatus.cs
using System.Collections.Generic;
using FinalTermOS.Models; // 引入 Student 類別和狀態枚舉的命名空間

namespace FinalTermOS.Models
{
    // 將 SimulationStatus 定義為一個公共類別
    public class SleepingTASimulationStatus // 可以改個更明確的名稱
    {
        public TAState TAState { get; set; }
        public List<Student> Students { get; set; } // 需要 Student 類別也能被存取
        public int WaitingQueueCount { get; set; }
        public int AvailableChairs { get; set; }
    }
}