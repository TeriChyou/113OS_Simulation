// Services/BankersAlgorithmService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; // For StringBuilder for safe sequences
using FinalTermOS.Models; // Needed for ResourceVector, ProcessData, DTOs

namespace FinalTermOS.Services
{
    public class BankersAlgorithmService
    {
        // === Simulation Data (from problem description) ===
        private ResourceVector _available;
        private List<ProcessData> _processes;

        // === State for Frontend Display ===
        public bool IsSimulationRunning { get; private set; } // Although static, helps with UI control
        private List<string> _logMessages; // To store algorithm steps and results

        public BankersAlgorithmService()
        {
            IsSimulationRunning = false; // Initially not running
            _logMessages = new List<string>();
            InitializeData(); // Initialize data based on problem description
        }

        // Helper to add log messages and print to console
        private void AddLog(string message)
        {
            _logMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.WriteLine(message); // Still print to console for server-side debugging
        }

        // --- Initialize data based on problem description ---
        private void InitializeData()
        {
            // Initial Available Resources: A=3, B=3, C=2, D=1
            _available = new ResourceVector(3, 3, 2, 1);

            // Initialize Processes with Max and Allocation
            _processes = new List<ProcessData>
            {
                // P1: Max(4,2,1,2), Alloc(2,0,0,1)
                new ProcessData(1, new ResourceVector(4, 2, 1, 2), new ResourceVector(2, 0, 0, 1)),
                // P2: Max(5,2,5,2), Alloc(3,1,2,1)
                new ProcessData(2, new ResourceVector(5, 2, 5, 2), new ResourceVector(3, 1, 2, 1)),
                // P3: Max(2,3,1,6), Alloc(2,1,0,3)
                new ProcessData(3, new ResourceVector(2, 3, 1, 6), new ResourceVector(2, 1, 0, 3)),
                // P4: Max(1,4,2,4), Alloc(1,3,1,2)
                new ProcessData(4, new ResourceVector(1, 4, 2, 4), new ResourceVector(1, 3, 1, 2)),
                // P5: Max(3,6,6,5), Alloc(1,4,3,2)
                new ProcessData(5, new ResourceVector(3, 6, 6, 5), new ResourceVector(1, 4, 3, 2))
            };

            AddLog("銀行家演算法資料初始化完成。");
        }

        // --- Get Current Simulation Status for Frontend ---
        public BankersAlgorithmStatus GetCurrentStatus()
        {
            _logMessages.Clear(); // Clear logs for each status request (as it re-calculates)
            IsSimulationRunning = false; // Banker's is a static calculation, not a running simulation

            var status = new BankersAlgorithmStatus
            {
                IsRunning = IsSimulationRunning,
                Message = "點擊 '執行演算法' 來查看結果。",
                Available = _available.ToString(),
                Processes = _processes.Select(p => new ProcessStatusDto
                {
                    Id = p.Id,
                    Max = p.Max.ToString(),
                    Allocation = p.Allocation.ToString(),
                    Need = p.Need.ToString(),
                    IsFinished = false // Will be updated by safety algorithm
                }).ToList(),
                LogMessages = new List<string>() // Provide a fresh list for logs
            };

            // Run the algorithm and populate results only when explicitly requested,
            // or when StartSimulation is called (which is 'Execute Algorithm' for this problem)
            return status;
        }

        // --- Execute Banker's Algorithm (This is equivalent to 'Start Simulation' for this problem) ---
        public BankersAlgorithmStatus ExecuteAlgorithm()
        {
            _logMessages.Clear(); // Clear logs from previous run
            IsSimulationRunning = true; // Indicate that calculation is active

            AddLog("--- 執行銀行家演算法 ---");

            // 1. 列出 Allocation, Max, Available, Need
            AddLog("\n1. 當前系統狀態矩陣內容：");
            AddLog($"  Available: {_available.ToString()}");
            AddLog("  進程 | Max        | Allocation | Need       |");
            AddLog("-------------------------------------------------");
            foreach (var p in _processes)
            {
                AddLog($"  P{p.Id}   | {p.Max.ToString()} | {p.Allocation.ToString()} | {p.Need.ToString()} |");
            }

            // 2. 找出安全序列
            AddLog("\n2. 執行安全演算法，尋找安全序列：");
            var safeSequences = FindSafeSequences(_processes.ToList(), new ResourceVector(_available.ToArray()), new List<int>());

            if (safeSequences.Any())
            {
                AddLog($"  找到 {safeSequences.Count} 組安全序列：");
                foreach (var seq in safeSequences)
                {
                    AddLog($"    {string.Join(" -> ", seq.Select(id => $"P{id}"))}");
                }
                AddLog("  系統處於安全狀態。");
            }
            else
            {
                AddLog("  未找到安全序列。系統處於不安全狀態。");
            }

            // 3. 處理 P1 的請求
            AddLog("\n3. 處理 P1 的資源請求 (1, 1, 0, 0)：");
            ResourceVector requestP1 = new ResourceVector(1, 1, 0, 0);
            ProcessData p1 = _processes.FirstOrDefault(p => p.Id == 1);
            string requestP1Result = "";
            if (p1 != null)
            {
                requestP1Result = CheckRequest(p1, requestP1);
            }
            else
            {
                requestP1Result = "P1 進程不存在。";
            }
            AddLog($"  P1 請求結果: {requestP1Result}");

            // 4. 處理 P4 的請求
            AddLog("\n4. 處理 P4 的資源請求 (0, 0, 2, 0)：");
            ResourceVector requestP4 = new ResourceVector(0, 0, 2, 0);
            ProcessData p4 = _processes.FirstOrDefault(p => p.Id == 4);
            string requestP4Result = "";
            if (p4 != null)
            {
                requestP4Result = CheckRequest(p4, requestP4);
            }
            else
            {
                requestP4Result = "P4 進程不存在。";
            }
            AddLog($"  P4 請求結果: {requestP4Result}");


            IsSimulationRunning = false; // Calculation is complete

            // Prepare the final status object to send to frontend
            var finalStatus = new BankersAlgorithmStatus
            {
                IsRunning = IsSimulationRunning,
                Message = "銀行家演算法執行完成。請查看結果。",
                Available = _available.ToString(),
                Processes = _processes.Select(p => new ProcessStatusDto
                {
                    Id = p.Id,
                    Max = p.Max.ToString(),
                    Allocation = p.Allocation.ToString(),
                    Need = p.Need.ToString(),
                    // IsFinished will be updated based on safety algorithm findings if needed for display
                }).ToList(),
                SafeSequences = safeSequences.Select(s => string.Join(" -> ", s.Select(id => $"P{id}"))).ToList(),
                RequestP1Result = requestP1Result,
                RequestP4Result = requestP4Result,
                LogMessages = _logMessages.ToList()
            };

            return finalStatus;
        }

        // --- 安全演算法 (Safety Algorithm) ---
        // Recursively finds all safe sequences
        private List<List<int>> FindSafeSequences(List<ProcessData> currentProcesses, ResourceVector currentAvailable, List<int> currentSequence)
        {
            List<List<int>> foundSafeSequences = new List<List<int>>();

            // Base case: If all processes are in the sequence, it's a safe sequence
            if (currentProcesses.Count == 0)
            {
                foundSafeSequences.Add(new List<int>(currentSequence));
                return foundSafeSequences;
            }

            // Iterate through remaining processes
            foreach (var p in currentProcesses)
            {
                // Check if currentAvailable >= p.Need
                if (p.Need.LessThanOrEqual(currentAvailable))
                {
                    // This process can execute. Assume it does.
                    List<ProcessData> remainingProcesses = currentProcesses.Where(x => x.Id != p.Id).ToList();
                    ResourceVector newAvailable = currentAvailable + p.Allocation; // Resources are released

                    List<int> newSequence = new List<int>(currentSequence);
                    newSequence.Add(p.Id);

                    // Recursively find safe sequences with remaining processes
                    var subSequences = FindSafeSequences(remainingProcesses, newAvailable, newSequence);
                    foundSafeSequences.AddRange(subSequences);
                }
            }
            return foundSafeSequences;
        }

        // --- 請求演算法 (Request Algorithm) ---
        // Checks if a request can be granted safely
        private string CheckRequest(ProcessData process, ResourceVector request)
        {
            // 1. If Request > Need, error (process asked for more than its max demand)
            if (!request.LessThanOrEqual(process.Need))
            {
                return $"拒絕: P{process.Id} 請求資源 {request.ToString()} 超過其所需 (Need: {process.Need.ToString()})。";
            }

            // 2. If Request > Available, process must wait (not enough resources)
            if (!request.LessThanOrEqual(_available))
            {
                return $"拒絕: P{process.Id} 請求資源 {request.ToString()} 數量超過系統可用 ({_available.ToString()})，進程必須等待。";
            }

            // 3. Assume resources are granted, and check if system is still safe
            // Create temporary state
            ResourceVector tempAvailable = _available - request;
            ResourceVector tempAllocation = process.Allocation + request;
            ResourceVector tempNeed = process.Need - request;

            // Create a temporary list of processes with updated state for the requesting process
            List<ProcessData> tempProcesses = _processes.Select(p =>
            {
                if (p.Id == process.Id)
                {
                    return new ProcessData(p.Id, p.Max, tempAllocation); // Update allocation for requesting process
                }
                return p;
            }).ToList();

            // Run safety algorithm on this temporary state
            var safeSequences = FindSafeSequences(tempProcesses, tempAvailable, new List<int>());

            if (safeSequences.Any())
            {
                // Request can be granted, and the system remains in a safe state.
                return $"接受: P{process.Id} 請求可立即獲得，系統保持安全狀態。安全序列範例：{string.Join(" -> ", safeSequences.First().Select(id => $"P{id}"))}";
            }
            else
            {
                // Request cannot be granted safely, process must wait.
                return $"拒絕: P{process.Id} 請求將使系統進入不安全狀態，進程必須等待。";
            }
        }
    }
}