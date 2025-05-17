// Services/DiningPhilosophersSimulationService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq; // Needed for LINQ operations
using FinalTermOS.Models; // Needed for Philosopher, Chopstick, DTOs, and Enums
using System.Collections.Concurrent;

namespace FinalTermOS.Services
{
    public class DiningPhilosophersSimulationService
    {
        // --- Synchronization Objects ---
        private readonly object _lock = new object(); // General lock for shared data access
        private List<Chopstick> _chopsticks; // List of chopsticks
        private List<Philosopher> _philosophers; // List of philosophers

        // Cancellation token source to signal threads to stop
        private CancellationTokenSource _cancellationTokenSource;
        // Flag to indicate if simulation is running
        public bool IsSimulationRunning { get; private set; }

        // ADD logs to UI
        private ConcurrentQueue<string> _logMessages = new ConcurrentQueue<string>();
        private const int MaxLogMessages = 50; // Keep only the last 50 messages
        // Pause simulation
        // === NEW: Pause control ===
        private volatile bool _isPaused; // Flag to indicate pause state (volatile for thread visibility)
        // ManualResetEventSlim: true = signaled (threads don't wait, proceed), false = non-signaled (threads wait)
        private ManualResetEventSlim _pauseEvent;

        public bool IsPaused
        {
            get { return _isPaused; }
            private set { _isPaused = value; } // Optional: allow setting internally if needed
        }

        // --- Constructor ---
        public DiningPhilosophersSimulationService()
        {
            IsSimulationRunning = false; // Initially not running
            _isPaused = false; // Initially not paused
            _pauseEvent = new ManualResetEventSlim(true); // Initial state: signaled (not paused)
        }

        // --- Get Current Simulation Status for Frontend ---
        public DiningPhilosophersSimulationStatus GetCurrentStatus()
        {
            lock (_lock) // Ensure thread safety when accessing shared state
            {
                return new DiningPhilosophersSimulationStatus
                {
                    IsRunning = IsSimulationRunning,
                    Message = IsSimulationRunning ? "模擬正在進行..." : "模擬已停止。",
                    IsPaused = IsPaused,
                    Philosophers = _philosophers?.Select(p => new PhilosopherStatusDto
                    {
                        Id = p.Id,
                        State = p.State
                    }).ToList() ?? new List<PhilosopherStatusDto>(), // Handle null if not initialized
                    Forks = _chopsticks?.Select(c => new ForkStatusDto
                    {
                        Id = c.Id,
                        IsAvailable = c.IsAvailable
                    }).ToList() ?? new List<ForkStatusDto>(), // Handle null if not initialized
                    LogMessages = _logMessages.ToList() // Convert queue to list for serialization
                };
            }
        }
        // LOG
        private void AddLog(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            _logMessages.Enqueue(timestampedMessage);
            // Limit the number of messages in the queue
            while (_logMessages.Count > MaxLogMessages)
            {
                _logMessages.TryDequeue(out _); // Discard oldest message
            }
            Console.WriteLine(timestampedMessage); // Still print to console for server-side debugging
        }

        // --- Start Simulation ---
        public void StartSimulation(int numberOfPhilosophers)
        {
            if (IsSimulationRunning)
            {
                AddLog("Simulation is already running.");
                return;
            }
            _logMessages = new ConcurrentQueue<string>(); // Clear logs on start
            if (numberOfPhilosophers < 2)
            {
                AddLog("Number of philosophers must be at least 2.");
                return;
            }

            // Initialize simulation components
            _cancellationTokenSource = new CancellationTokenSource();
            _chopsticks = new List<Chopstick>();
            _philosophers = new List<Philosopher>();
            IsSimulationRunning = true;
            _isPaused = false; // Ensure not paused on start
            _pauseEvent.Set();

            CancellationToken token = _cancellationTokenSource.Token;

            // Create chopsticks (same number as philosophers)
            for (int i = 0; i < numberOfPhilosophers; i++)
            {
                _chopsticks.Add(new Chopstick(i)); // Chopsticks are identified by index 0 to N-1
            }

            // Create philosophers
            for (int i = 0; i < numberOfPhilosophers; i++)
            {
                var philosopher = new Philosopher(i);
                _philosophers.Add(philosopher);
                // Start each philosopher's logic in a separate task/thread
                Task.Run(() => PhilosopherLogic(philosopher, token), token);
            }

            AddLog($"哲學家進餐模擬初始化完成：{numberOfPhilosophers} 位哲學家，{numberOfPhilosophers} 雙筷子。");
        }

        // --- Stop Simulation ---
        public void StopSimulation()
        {
            if (IsSimulationRunning && _cancellationTokenSource != null)
            {
                AddLog("Stopping Dining Philosophers simulation...");
                _cancellationTokenSource.Cancel(); // Signal cancellation
                IsSimulationRunning = false; // Update flag
                _isPaused = false; // Reset pause state on stop
                _pauseEvent.Set(); 
                // Optional: Dispose of resources if needed
                _cancellationTokenSource.Dispose();
                AddLog("Dining Philosophers simulation stopped.");
            }
        }
        // === Pause Simulation Method ===
        public void PauseSimulation()
        {
            if (IsSimulationRunning && !_isPaused)
            {
                _isPaused = true;
                _pauseEvent.Reset(); // Set to non-signaled state (threads will wait)
                AddLog("模擬已暫停。");
            }
        }

        // === Resume Simulation Method ===
        public void ResumeSimulation()
        {
            if (IsSimulationRunning && _isPaused)
            {
                _isPaused = false;
                _pauseEvent.Set(); // Set to signaled state (threads will proceed)
                AddLog("模擬已恢復。");
            }
        }

        // --- Philosopher's Main Logic ---
        private void PhilosopherLogic(Philosopher philosopher, CancellationToken cancellationToken)
        {
            AddLog($"哲學家 {philosopher.Id} 執行緒啟動。");

            while (!cancellationToken.IsCancellationRequested) // Loop until cancelled
            {
                // Pause check
                try
                {
                    _pauseEvent.Wait(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation occurred while waiting on pauseEvent
                    Console.WriteLine($"哲學家 {philosopher.Id}: 暫停等待中被取消。");
                    break; // Exit loop
                }
                // 1. Thinking
                AddLog($"哲學家 {philosopher.Id}: 正在思考...");
                lock (_lock) { philosopher.State = PhilosopherState.Thinking; }
                try
                {
                    // Simulate thinking time (1-15 seconds)
                    Task.Delay(new Random().Next(1000, 5000), cancellationToken).Wait();
                }
                catch (TaskCanceledException) { AddLog($"哲學家 {philosopher.Id}: 思考中被取消。"); break; }

                if (cancellationToken.IsCancellationRequested) break;

                try { _pauseEvent.Wait(cancellationToken); }
                catch (OperationCanceledException) { Console.WriteLine($"哲學家 {philosopher.Id}: 暫停等待中被取消。"); break; }


                // 2. Hungry - Try to pick up forks
                AddLog($"哲學家 {philosopher.Id}: 感到飢餓，嘗試拿起筷子...");
                lock (_lock) { philosopher.State = PhilosopherState.Hungry; }

                // The philosopher needs left and right chopsticks
                int leftForkId = philosopher.Id;
                int rightForkId = (philosopher.Id + 1) % _philosophers.Count; // Modulo to wrap around for the last philosopher

                bool pickedUpBothForks = false;

                // Loop until both forks are picked up or cancelled
                while (!cancellationToken.IsCancellationRequested && !pickedUpBothForks)
                {
                    // Attempt to pick up left fork first
                    bool pickedUpLeft = false;
                    lock (_lock)
                    {
                        if (_chopsticks[leftForkId].IsAvailable)
                        {
                            _chopsticks[leftForkId].IsAvailable = false;
                            pickedUpLeft = true;
                            AddLog($"哲學家 {philosopher.Id}: 拿起左邊的筷子 {leftForkId}。");
                        }
                    }

                    if (pickedUpLeft)
                    {
                        // Now try to pick up the right fork
                        bool pickedUpRight = false;
                        lock (_lock)
                        {
                            if (_chopsticks[rightForkId].IsAvailable)
                            {
                                _chopsticks[rightForkId].IsAvailable = false;
                                pickedUpRight = true;
                                pickedUpBothForks = true; // Success!
                                AddLog($"哲學家 {philosopher.Id}: 拿起右邊的筷子 {rightForkId}。");
                            }
                        }

                        if (!pickedUpRight)
                        {
                            // Could not pick up right fork, put left fork back
                            lock (_lock)
                            {
                                _chopsticks[leftForkId].IsAvailable = true; // Put left fork back
                                AddLog($"哲學家 {philosopher.Id}: 無法拿起右邊筷子 {rightForkId}，放下左邊筷子 {leftForkId}。");
                            }
                            // Philosopher waits a bit before trying again
                            lock (_lock) { philosopher.State = PhilosopherState.WaitingForForks; }
                            try
                            {
                                // Wait and re-try
                                Task.Delay(new Random().Next(500, 1500), cancellationToken).Wait();
                            }
                            catch (TaskCanceledException) { break; } // Exit loop if cancelled
                            try { _pauseEvent.Wait(cancellationToken); }
                            catch (OperationCanceledException) { Console.WriteLine($"哲學家 {philosopher.Id}: 暫停等待中被取消。"); break; }

                        }
                    }
                    else
                    {
                        // Could not pick up left fork, wait and re-try
                        lock (_lock) { philosopher.State = PhilosopherState.WaitingForForks; }
                        try
                        {
                            // Wait and re-try
                            Task.Delay(new Random().Next(500, 1500), cancellationToken).Wait();
                        }
                        catch (TaskCanceledException) { break; } // Exit loop if cancelled
                        try { _pauseEvent.Wait(cancellationToken); }
                        catch (OperationCanceledException) { Console.WriteLine($"哲學家 {philosopher.Id}: 暫停等待中被取消。"); break; }

                    }

                    if (cancellationToken.IsCancellationRequested) break; // Check cancellation inside loop

                } // End while(!pickedUpBothForks) loop

                if (cancellationToken.IsCancellationRequested) break; // Check cancellation before eating

                try { _pauseEvent.Wait(cancellationToken); }
                catch (OperationCanceledException) { Console.WriteLine($"哲學家 {philosopher.Id}: 暫停等待中被取消。"); break; }


                // 3. Eating
                AddLog($"哲學家 {philosopher.Id}: 正在吃飯...");
                lock (_lock) { philosopher.State = PhilosopherState.Eating; }
                try
                {
                    // Simulate eating time (1-15 seconds)
                    Task.Delay(new Random().Next(1000, 5000), cancellationToken).Wait();
                }
                catch (TaskCanceledException) { AddLog($"哲學家 {philosopher.Id}: 吃飯中被取消。"); break; }

                if (cancellationToken.IsCancellationRequested) break;

                // 4. Put down forks
                AddLog($"哲學家 {philosopher.Id}: 放下筷子 {leftForkId} 和 {rightForkId}。");
                lock (_lock)
                {
                    _chopsticks[leftForkId].IsAvailable = true;
                    _chopsticks[rightForkId].IsAvailable = true;
                }

                // Loop continues for the next thinking phase
            } // End while(!cancellationToken.IsCancellationRequested) loop

            AddLog($"哲學家 {philosopher.Id} 執行緒退出。");
            lock (_lock) { philosopher.State = PhilosopherState.Thinking; } // Reset state to thinking on exit
        }
    }
}