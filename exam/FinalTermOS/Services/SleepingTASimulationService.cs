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
        public SleepingTASimulationStatus GetCurrentStatus()
        {
            lock (_lock)
            {
                return new SleepingTASimulationStatus
                {
                    TAState = _currentTAState,
                    // Convert List<Student> to List<StudentStatusDto>
                    Students = _students.Select(s => new StudentStatusDto
                    {
                        Id = s.Id,
                        State = s.State
                    }).ToList(),
                    WaitingQueueCount = _waitingStudentsQueue.Count,
                    AvailableChairs = _waitingChairsSemaphore.CurrentCount
                };
            }
        }

        // Cancellation token source to signal threads to stop
        private CancellationTokenSource _cancellationTokenSource;

        // Property to check if simulation is currently running (optional but helpful)
        public bool IsSimulationRunning { get; private set; }

        private bool _loopStudentsAfterServed; // Flag from frontend

        // --- 模擬啟動方法 ---
        public void StartSimulation(int numberOfStudents, int numberOfChairs, bool loopStudents)
        {
             // Prevent starting if already running
            if (IsSimulationRunning)
            {
                Console.WriteLine("Simulation is already running.");
                return;
            }

            // === Initialization ===
            _cancellationTokenSource = new CancellationTokenSource();
            _waitingChairsSemaphore = new SemaphoreSlim(numberOfChairs, numberOfChairs);
            _taSemaphore = new SemaphoreSlim(0, 1);
            _currentTAState = TAState.Sleeping;
            _students = new List<Student>();
            _waitingStudentsQueue = new Queue<int>();
            IsSimulationRunning = true;
            _loopStudentsAfterServed = loopStudents; // Save the setting

            CancellationToken token = _cancellationTokenSource.Token;

            // Create and start student threads
            for (int i = 0; i < numberOfStudents; i++)
            {
                var student = new Student(i + 1);
                _students.Add(student);
                // Pass the cancellation token to the student logic
                Task.Run(() => StudentLogic(student, token), token); // Pass token and link task to token
            }

            // Create and start TA thread
            // Pass the cancellation token to the TA logic
            Task.Run(() => TALogic(token), token); // Pass token and link task to token

            Console.WriteLine("模擬初始化完成，執行緒已啟動。");
        }
                // --- 新增停止模擬方法 ---
        public void StopSimulation()
        {
            // If simulation is running and the token source exists
            if (IsSimulationRunning && _cancellationTokenSource != null)
            {
                Console.WriteLine("Stopping simulation...");
                _cancellationTokenSource.Cancel(); // Signal cancellation to all linked tokens
                IsSimulationRunning = false; // Clear running flag

                // Optional: Wait for tasks to complete (be careful with this in web requests)
                // Task.WhenAll(tasks).Wait(); // If you keep track of all task objects

                // Clean up resources if necessary (semaphores are managed by GC, but explicit Dispose is good practice)
                _cancellationTokenSource.Dispose();
                // _waitingChairsSemaphore.Dispose(); // Only if you know it's safe to dispose now
                // _taSemaphore.Dispose();          // Only if you know it's safe to dispose now

                Console.WriteLine("Simulation stopped.");
            }
        }

        // --- TA 的執行緒邏輯 ---
        private void TALogic(CancellationToken cancellationToken)
        {
            Console.WriteLine("TA thread started.");

            // Outer loop runs until cancellation is requested
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("TA: Waiting for student signal or checking queue...");
                // Wait for TA semaphore, but also check for cancellation
                // Use Wait(timeout, cancellationToken) or WaitAsync if possible
                // Wait() with just CancellationToken is not directly available,
                // need to use a timeout or WaitHandle
                // Simpler for this example: check token BEFORE waiting and inside inner loop
                 // Use a timeout to allow periodic token checks
                 _taSemaphore.Wait(Timeout.Infinite, cancellationToken); // Wait indefinitely, but cancelable

                // If cancellation requested while waiting, exit loop
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("TA: Cancellation requested while waiting, exiting.");
                    break;
                }

                Console.WriteLine("TA: Woken up, checking waiting queue...");

                // Inner loop to help students from the queue
                // Check cancellation token in the inner loop condition as well
                while (!cancellationToken.IsCancellationRequested) // Check token here too
                {
                    int studentIdToHelp = -1;
                    Student studentToHelp = null;

                    lock (_lock)
                    {
                        if (_waitingStudentsQueue.Count > 0)
                        {
                            studentIdToHelp = _waitingStudentsQueue.Dequeue();
                            studentToHelp = _students.FirstOrDefault(s => s.Id == studentIdToHelp);

                            if (studentToHelp != null)
                            {
                                _waitingChairsSemaphore.Release();
                                Console.WriteLine($"TA: 取出學生 {studentIdToHelp}。椅子釋放。");
                            }
                        }
                        else
                        {
                            // Queue is empty, break the inner loop
                            Console.WriteLine("TA: 等待隊列為空，完成服務。");
                             lock (_lock) { _currentTAState = TAState.Sleeping; } // Set state back to sleeping
                            break; // Exit inner loop
                        }
                    }

                    // If cancellation requested while processing queue, break inner loop
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("TA: Cancellation requested while processing queue, exiting inner loop.");
                        break; // Exit inner loop
                    }


                    // --- Help the student ---
                    if (studentToHelp != null)
                    {
                        Console.WriteLine($"TA: 正在幫助學生 {studentToHelp.Id}...");
                        lock (_lock)
                        {
                            _currentTAState = TAState.HelpingStudent;
                            studentToHelp.State = StudentState.GettingHelp;
                        }

                        // Simulate helping time - Pass token to Sleep if a cancelable sleep is needed
                        // Thread.Sleep() is not directly cancelable. Can use Task.Delay with token.
                        try
                        {
                             Task.Delay(new Random().Next(1000, 3000), cancellationToken).Wait(); // Use cancelable delay
                        }
                        catch (TaskCanceledException)
                        {
                             Console.WriteLine($"TA: Helping interrupted by cancellation.");
                             // Cancellation requested during helping, clean up and exit inner loop
                             lock(_lock) { studentToHelp.State = StudentState.Leaving; } // Set state to leaving
                             break; // Exit inner loop
                        }


                        lock (_lock)
                        {
                            Console.WriteLine($"TA: 完成幫助學生 {studentToHelp.Id}。");
                            studentToHelp.State = StudentState.Leaving;
                            studentToHelp.WaitingSignal.Set();
                            Console.WriteLine($"TA: 觸發學生 {studentToHelp.Id} 信號。");
                        }
                    }
                } // End of inner while loop

                 // If cancellation was requested by outer loop condition, this point is reached.
            } // End of outer while loop

             Console.WriteLine("TA thread exiting."); // TA thread terminates
        }
        // --- 學生 的執行緒邏輯 ---
        private void StudentLogic(Student student, CancellationToken cancellationToken)
        {
            Console.WriteLine($"學生 {student.Id} 執行緒啟動。");

            // === Control student looping ===
            // Use a flag to control if the student loops after being served once
            bool loopAfterServed = true; // This will be controlled by the switch

            // Outer loop runs until cancellation is requested OR student decides to stop looping
            // while (!cancellationToken.IsCancellationRequested && (loopAfterServed || student.ServiceCount == 0)) // More complex condition if tracking count
            // Simpler: loop while not cancelled, and exit if loopAfterServed is false AND student has been served at least once
            bool keepLooping = true;
            bool servedOnce = false;

            while (keepLooping && !cancellationToken.IsCancellationRequested) // Check cancellation token
            {
                // Student thinks
                Console.WriteLine($"學生 {student.Id}: 思考中...");
                lock (_lock) { student.State = StudentState.Thinking; }
                try
                {
                    Task.Delay(new Random().Next(2000, 5000), cancellationToken).Wait(); // Use cancelable delay
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"學生 {student.Id}: 思考中被取消。");
                    break; // Exit loop
                }


                // If cancellation requested while thinking, exit loop
                if (cancellationToken.IsCancellationRequested) break;

                // Student needs help
                Console.WriteLine($"學生 {student.Id}: 需要幫助，前往 TA 辦公室...");
                lock (_lock) { student.State = StudentState.SeekingHelp; }

                // Check if TA is sleeping
                bool isTASleeping;
                lock (_lock)
                {
                    isTASleeping = (_currentTAState == TAState.Sleeping);
                }

                if (isTASleeping)
                {
                    // If TA is sleeping, wake them up
                    Console.WriteLine($"學生 {student.Id}: TA 正在睡覺，叫醒 TA。");
                    // Release TA semaphore, but check if it's already signaled (count is 1)
                    // Only signal if TA is truly waiting (semaphore count is 0)
                     lock(_lock) // Need lock to check semaphore count safely
                     {
                         if(_taSemaphore.CurrentCount == 0)
                         {
                              try { _taSemaphore.Release(); } catch(SemaphoreFullException) { /* Already signaled */ }
                         }
                     }
                }

                // Try to acquire a chair (non-blocking attempt)
                Console.WriteLine($"學生 {student.Id}: 嘗試獲取椅子...");
                // Use Wait(0, cancellationToken) for cancelable non-blocking wait
                bool acquiredChair = _waitingChairsSemaphore.Wait(0, cancellationToken);

                 // If cancellation requested while trying to wait, exit loop
                if (cancellationToken.IsCancellationRequested) break;


                if (acquiredChair)
                {
                    // Successfully acquired a chair, sit and wait
                    Console.WriteLine($"學生 {student.Id}: 成功獲得椅子，坐在等待。");
                    lock (_lock)
                    {
                        student.State = StudentState.WaitingInChair;
                        _waitingStudentsQueue.Enqueue(student.Id); // Add student ID to waiting queue
                        Console.WriteLine($"學生 {student.Id} 加入等待隊列。當前等待人數: {_waitingStudentsQueue.Count}");
                    }

                    // --- Student waits here until signaled by TA ---
                    // Use Wait(cancellationToken) for cancelable wait
                    try
                    {
                         student.WaitingSignal.Wait(cancellationToken); // Wait until signaled or cancelled
                         servedOnce = true;
                         Console.WriteLine($"學生 {student.Id}: WaitingSignal 被觸發，被服務完成。");

                         // Reset the signal for potential future waits
                         student.WaitingSignal.Reset();

                         // Student's state was set to Leaving by TA logic.
                         // Now, simulate the student leaving and doing something else for a while.
                         Console.WriteLine($"學生 {student.Id}: 完成服務，離開辦公室。");
                         lock (_lock) { student.State = StudentState.Thinking; } // Set state back to thinking

                         // === Check the loop switch after being served ===
                         if (!loopAfterServed)
                         {
                             keepLooping = false; // Stop looping after this cycle
                             Console.WriteLine($"學生 {student.Id}: 完成一次服務，依設定不再循環。");
                         }
                         else
                         {
                             // If looping, add a delay before the next cycle
                              try { Task.Delay(new Random().Next(2000, 5000), cancellationToken).Wait(); } // Delay before next cycle
                              catch (TaskCanceledException) { Console.WriteLine($"學生 {student.Id}: 循環間隔中被取消。"); keepLooping = false; }
                         }

                        if (_loopStudentsAfterServed) // Check the service-level flag
                        {
                            try { Task.Delay(new Random().Next(2000, 5000), cancellationToken).Wait(); }
                            catch (TaskCanceledException) { break; } // Exit loop on cancellation
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine($"學生 {student.Id}: 等待被服務時被取消。");
                        // Cancellation requested while waiting for signal.
                        // Need to remove student from queue if they are still there.
                        lock(_lock)
                        {
                             // This is tricky: student might have just been dequeued by TA.
                             // A robust solution would involve coordination on removal.
                             // For simplicity now, we assume if Wait is cancelled, they didn't get served.
                             // Need to ensure chair is released if acquired but not served.
                             // The chair is released by TA when dequeuing. If cancelled while waiting,
                             // they are still in queue or just dequeued. This needs careful handling.
                             // Let's assume for now if cancelled while waiting for signal, they leave the queue.
                             // Removing from queue requires finding and removing, Queue<T> doesn't support easy removal.
                             // A List or a concurrent collection might be better for the queue if cancellation mid-wait is common.
                             // For this example, let's just exit and accept potential small inconsistencies on cancellation mid-wait.
                             student.State = StudentState.Leaving; // Set state to leaving due to cancellation
                        }
                        keepLooping = false; // Stop looping on cancellation
                        break;
                    }
                    finally
                    {
                         // Ensure the student's signal is reset if they exit this block for any reason (served or cancelled)
                         student.WaitingSignal.Reset();
                    }


                }
                else
                {
                    // No chairs available, come back later
                    Console.WriteLine($"學生 {student.Id}: 沒有可用的椅子，稍後回來。");
                    lock (_lock) { student.State = StudentState.Leaving; }

                    // Add a more substantial random delay before the student tries again
                    try
                    {
                         Task.Delay(new Random().Next(5000, 10000), cancellationToken).Wait();
                    }
                    catch (TaskCanceledException)
                    {
                         Console.WriteLine($"學生 {student.Id}:稍後回來等待中被取消。");
                         break; // Exit loop
                    }

                    lock (_lock) { student.State = StudentState.Thinking; } // Set state back to thinking after delay
                }

                // If cancellation requested at the end of a loop iteration
                if (cancellationToken.IsCancellationRequested) keepLooping = false;

            } // End of while loop

             Console.WriteLine($"學生 {student.Id} 執行緒退出。"); // Student thread terminates
             lock(_lock) // Optional: Update student state to truly finished
             {
                 // Maybe add a StudentState.Finished enum value
                 // student.State = StudentState.Finished;
             }
        }
    }
}