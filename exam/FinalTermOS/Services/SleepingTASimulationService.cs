using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq; // 需要使用 Linq 來處理列表
using FinalTermOS.Models; // 引入狀態枚舉

namespace FinalTermOS.Services
{
    public class SleepingTASimulationService
    {
        // --- 同步物件 ---
        private SemaphoreSlim _waitingChairsSemaphore; // 控制走廊椅子的號誌
        private SemaphoreSlim _taSemaphore;          // 用於學生叫醒 TA 的號誌
        private readonly object _lock = new object();  // 用於保護共用資料 (如等待隊列)

        // --- 模擬狀態資料 ---
        private TAState _currentTAState;
        private List<Student> _students; // 學生列表，包含每個學生的狀態和 ID
        private Queue<int> _waitingStudentsQueue; // 在椅子上等待的學生 ID 隊列

        // 定義一個類別來承載模擬狀態，用於傳遞給 View
        public SleepingTASimulationStatus GetCurrentStatus() // 修改返回類型
        {
            lock (_lock)
            {
                return new SleepingTASimulationStatus // 使用新的類別名稱
                {
                    TAState = _currentTAState,
                    Students = _students.ToList(),
                    WaitingQueueCount = _waitingStudentsQueue.Count,
                    AvailableChairs = _waitingChairsSemaphore.CurrentCount
                };
            }
        }

        // --- 模擬啟動方法 ---
        public void StartSimulation(int numberOfStudents, int numberOfChairs)
        {
            // --- 初始化 ---
            _waitingChairsSemaphore = new SemaphoreSlim(numberOfChairs, numberOfChairs);
            _taSemaphore = new SemaphoreSlim(0, 1); // TA 號誌，初始為 0 (睡覺)，最大為 1
            _currentTAState = TAState.Sleeping;
            _students = new List<Student>();
            _waitingStudentsQueue = new Queue<int>();

            // 創建學生執行緒
            for (int i = 0; i < numberOfStudents; i++)
            {
                var student = new Student(i + 1); // 給學生一個 ID (從 1 開始)
                _students.Add(student);
                // 使用 Task.Run 在背景執行學生的邏輯
                Task.Run(() => StudentLogic(student));
            }

            // 創建 TA 執行緒
            Task.Run(() => TALogic());

            Console.WriteLine("模擬初始化完成，執行緒已啟動。");
            // 注意：這裡不會阻塞，模擬在背景運行
        }

        // --- TA 的執行緒邏輯 ---
        private void TALogic()
        {
            Console.WriteLine("TA 執行緒啟動。TA 正在睡覺...");

            while (true) // TA 持續運行
            {
                // TA 等待學生叫醒 (如果沒有等待的學生且 TA 睡著)
                Console.WriteLine("TA: 等待學生叫醒或檢查隊列...");
                // 等待 _taSemaphore 被 Release (學生到來時會 Release)
                _taSemaphore.Wait(); // 如果計數為 0，TA 在這裡阻塞並睡覺

                // TA 被叫醒，檢查是否有學生在隊列中
                Console.WriteLine("TA: 被叫醒，檢查等待隊列。");
                lock (_lock)
                {
                     _currentTAState = TAState.CheckingForStudents; // 更新狀態
                }


                int studentIdToHelp = -1;

                lock (_lock)
                {
                    if (_waitingStudentsQueue.Count > 0)
                    {
                        studentIdToHelp = _waitingStudentsQueue.Dequeue(); // 從隊列取出一位學生
                         // 這位學生離開椅子，釋放一個椅子號誌
                        _waitingChairsSemaphore.Release();
                         Console.WriteLine($"TA: 從隊列中取出學生 {studentIdToHelp}。一個椅子被釋放。");
                    }
                    else
                    {
                        // 如果隊列為空，TA 回去睡覺
                         Console.WriteLine("TA: 隊列為空，回去睡覺。");
                         _currentTAState = TAState.Sleeping; // 更新狀態
                         continue; // 回到 while 循環的開始，TA 再次等待被叫醒
                    }
                }

                // --- 幫助學生 ---
                Console.WriteLine($"TA: 正在幫助學生 {studentIdToHelp}...");
                 lock (_lock)
                {
                    _currentTAState = TAState.HelpingStudent; // 更新狀態
                     var student = _students.FirstOrDefault(s => s.Id == studentIdToHelp);
                     if (student != null) student.State = StudentState.GettingHelp; // 更新學生狀態
                }

                // 模擬幫助學生所需的時間
                Thread.Sleep(new Random().Next(1000, 3000)); // 隨機幫助 1-3 秒

                 lock (_lock)
                {
                     Console.WriteLine($"TA: 完成幫助學生 {studentIdToHelp}。");
                     var student = _students.FirstOrDefault(s => s.Id == studentIdToHelp);
                     if (student != null) student.State = StudentState.Leaving; // 更新學生狀態為離開
                }

                // TA 幫助完學生後，再次檢查隊列 (回到循環開始)
            }
        }

        // --- 學生 的執行緒邏輯 ---
        private void StudentLogic(Student student)
        {
            Console.WriteLine($"學生 {student.Id} 執行緒啟動。");

            while (true) // 學生持續運行
            {
                // 學生思考一段時間
                Console.WriteLine($"學生 {student.Id}: 正在思考...");
                 lock (_lock) { student.State = StudentState.Thinking; }
                Thread.Sleep(new Random().Next(2000, 5000)); // 隨機思考 2-5 秒

                // 學生決定尋求幫助
                Console.WriteLine($"學生 {student.Id}: 需要幫助，前往 TA 辦公室...");
                 lock (_lock) { student.State = StudentState.SeekingHelp; }

                // 檢查 TA 是否在睡覺
                bool isTASleeping;
                lock (_lock)
                {
                    isTASleeping = (_currentTAState == TAState.Sleeping);
                }


                if (isTASleeping)
                {
                    // 如果 TA 在睡覺，叫醒 TA
                    Console.WriteLine($"學生 {student.Id}: TA 正在睡覺，叫醒 TA。");
                    _taSemaphore.Release(); // 釋放 TA 號誌
                }

                // 嘗試獲取一個椅子
                Console.WriteLine($"學生 {student.Id}: 嘗試獲取走廊的椅子...");
                bool acquiredChair = _waitingChairsSemaphore.Wait(0); // Wait(0) 表示非阻塞嘗試獲取

                if (acquiredChair)
                {
                    // 成功獲取椅子，坐在椅子上等待
                    Console.WriteLine($"學生 {student.Id}: 成功獲得一個椅子，坐在上面等待。");
                    lock (_lock)
                    {
                        student.State = StudentState.WaitingInChair;
                        _waitingStudentsQueue.Enqueue(student.Id); // 加入等待隊列
                         Console.WriteLine($"學生 {student.Id} 加入等待隊列。當前等待人數: {_waitingStudentsQueue.Count}");
                    }

                    // 學生在這裡等待 TA 從隊列中取出他 (這部分由 TA 邏輯處理)
                    // 學生的狀態會被 TA 邏輯從 WaitingInChair 改為 GettingHelp，然後到 Leaving
                     // 這裡的學生執行緒會一直等到其狀態被 TA 改變
                    // 然而，我們這裡沒有明確的阻塞機制讓學生等待被服務
                    // 一種簡單的方式是讓學生在這裡自旋檢查狀態，或者使用事件通知（更複雜）
                    // 對於簡單模擬，讓學生執行緒繼續，並依賴狀態更新來表示它的位置，
                    // 或者在 TALogic 幫助完學生後，發送一個信號或使用一個特定的學生等待用的 SemaphoreSlim 來通知學生。

                    // 為了簡化，我們目前依賴狀態更新來表示學生在哪個階段。
                    // 真實應用中，學生在這裡會阻塞等待被服務。

                }
                else
                {
                    // 沒有可用的椅子，稍後回來
                    Console.WriteLine($"學生 {student.Id}: 沒有可用的椅子，稍後回來。");
                    lock (_lock) { student.State = StudentState.Leaving; } // 暫時設為離開
                    Thread.Sleep(new Random().Next(1000, 3000)); // 隨機時間後重試
                }

                 // 如果學生被幫助完成並離開，可以結束這個執行緒，或者讓它再次進入思考狀態
                 // 為了持續模擬，讓學生再次進入思考狀態
            }
        }
    }
}