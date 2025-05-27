// Services/VirtualMemorySimulationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinalTermOS.Models;
using System.Collections.Concurrent; // For log messages

namespace FinalTermOS.Services
{
    public class VirtualMemorySimulationService
    {
        // === Simulation Data ===
        private List<int> _pageReferenceString; // The generated page reference string
        private List<PageReplacementResult> _allResults; // Stores results for all algorithms/frame counts
        private Dictionary<string, List<string>> _detailedTraces; // Stores step-by-step logs for each run (Key: AlgoName_FrameCount)

        // === State for Frontend Display ===
        public bool IsSimulationRunning { get; private set; } // Indicates if the overall calculation is in progress
        private ConcurrentQueue<string> _highLevelLogs; // For general messages (e.g., "Starting FIFO for 3 frames")
        private const int MaxHighLevelLogs = 100;

        public VirtualMemorySimulationService()
        {
            _highLevelLogs = new ConcurrentQueue<string>();
            _allResults = new List<PageReplacementResult>();
            _detailedTraces = new Dictionary<string, List<string>>();
            IsSimulationRunning = false;
        }

        // Helper to add general log messages
        private void AddHighLevelLog(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            _highLevelLogs.Enqueue(timestampedMessage);
            while (_highLevelLogs.Count > MaxHighLevelLogs)
            {
                _highLevelLogs.TryDequeue(out _);
            }
            Console.WriteLine(timestampedMessage); // Still print to console for server-side debugging
        }

        // --- Get Current Simulation Status for Frontend ---
        public VirtualMemorySimulationStatus GetCurrentStatus()
        {
            // This method returns the overall summary and log, NOT step-by-step.
            return new VirtualMemorySimulationStatus
            {
                IsRunning = IsSimulationRunning,
                Message = IsSimulationRunning ? "正在執行演算法計算..." : "點擊 '執行模擬' 來查看結果。",
                PageReferenceString = _pageReferenceString ?? new List<int>(),
                Results = _allResults,
                LogMessages = _highLevelLogs.ToList()
            };
        }

        // --- Start Simulation: Generates string and executes all algorithms ---
        public void StartSimulation(int referenceStringLength, int maxPageNumber, int minFrameCount, int maxFrameCount)
        {
            if (IsSimulationRunning)
            {
                AddHighLevelLog("模擬正在運行中。");
                return;
            }

            IsSimulationRunning = true;
            _highLevelLogs.Clear(); // Clear logs from previous run
            _allResults.Clear(); // Clear previous results
            _detailedTraces.Clear(); // Clear previous traces

            // 1. Generate random page reference string
            AddHighLevelLog("生成頁面參考串...");
            _pageReferenceString = GeneratePageReferenceString(referenceStringLength, maxPageNumber);
            AddHighLevelLog($"生成的頁面參考串 ({_pageReferenceString.Count} 個): {string.Join(", ", _pageReferenceString)}");

            // 2. Execute all algorithms for different frame counts
            AddHighLevelLog("執行所有頁面置換演算法...");
            for (int frames = minFrameCount; frames <= maxFrameCount; frames++)
            {
                AddHighLevelLog($"--- 頁框數量: {frames} ---");

                // Run FIFO
                PageReplacementResult fifoResult = RunFIFO(_pageReferenceString, frames);
                _allResults.Add(fifoResult);
                _detailedTraces[$"FIFO_{frames}Frames"] = fifoResult.ExecutionLog;
                AddHighLevelLog($"FIFO ({frames} 頁框): 分頁錯誤 = {fifoResult.PageFaults}");

                // Run LRU
                PageReplacementResult lruResult = RunLRU(_pageReferenceString, frames);
                _allResults.Add(lruResult);
                _detailedTraces[$"LRU_{frames}Frames"] = lruResult.ExecutionLog;
                AddHighLevelLog($"LRU ({frames} 頁框): 分頁錯誤 = {lruResult.PageFaults}");

                // Run Optimal
                PageReplacementResult optimalResult = RunOptimal(_pageReferenceString, frames);
                _allResults.Add(optimalResult);
                _detailedTraces[$"Optimal_{frames}Frames"] = optimalResult.ExecutionLog;
                AddHighLevelLog($"Optimal ({frames} 頁框): 分頁錯誤 = {optimalResult.PageFaults}");
            }

            IsSimulationRunning = false; // Calculation complete
            AddHighLevelLog("所有演算法執行完成。");
        }

        // --- Get Detailed Trace for Specific Algorithm and Frame Count ---
        public List<string> GetDetailedTrace(string algorithmName, int frameCount)
        {
            string key = $"{algorithmName}_{frameCount}Frames";
            if (_detailedTraces.TryGetValue(key, out var trace))
            {
                return trace;
            }
            return new List<string> { $"未找到 {algorithmName} (頁框: {frameCount}) 的詳細日誌。" };
        }

        // --- Helper: Generate Page Reference String ---
        private List<int> GeneratePageReferenceString(int length, int maxPageNumber)
        {
            Random rand = new Random();
            List<int> referenceString = new List<int>();
            for (int i = 0; i < length; i++)
            {
                referenceString.Add(rand.Next(0, maxPageNumber + 1)); // Pages from 0 to maxPageNumber
            }
            return referenceString;
        }

        // --- FIFO Algorithm Implementation ---
        private PageReplacementResult RunFIFO(List<int> referenceString, int frameCount)
        {
            PageReplacementResult result = new PageReplacementResult
            {
                AlgorithmName = "FIFO",
                PageFrameCount = frameCount,
                ExecutionLog = new List<string>()
            };

            Queue<int> frames = new Queue<int>(frameCount); // Represents frames with FIFO order
            HashSet<int> currentPages = new HashSet<int>(); // For quick lookup if page is in memory

            int pageFaults = 0;
            result.ExecutionLog.Add($"--- FIFO Algorithm (Frames: {frameCount}) ---");
            result.ExecutionLog.Add($"Reference String: {string.Join(", ", referenceString)}");

            foreach (int pageNumber in referenceString)
            {
                string logLine = $"Page {pageNumber}: ";
                if (currentPages.Contains(pageNumber))
                {
                    logLine += "Hit! (Page already in memory) ";
                }
                else
                {
                    pageFaults++;
                    logLine += "PAGE FAULT! ";

                    if (frames.Count < frameCount)
                    {
                        // Frames available, add directly
                        frames.Enqueue(pageNumber);
                        currentPages.Add(pageNumber);
                        logLine += "Frame available, added to memory. ";
                    }
                    else
                    {
                        // No frames available, replace oldest (FIFO)
                        int oldestPage = frames.Dequeue();
                        currentPages.Remove(oldestPage);
                        
                        frames.Enqueue(pageNumber);
                        currentPages.Add(pageNumber);
                        logLine += $"Page {oldestPage} replaced by Page {pageNumber}. ";
                    }
                }
                logLine += $"Current Frames: [{string.Join(", ", frames)}]";
                result.ExecutionLog.Add(logLine);
            }
            result.PageFaults = pageFaults;
            return result;
        }

        // --- LRU Algorithm Implementation ---
        private PageReplacementResult RunLRU(List<int> referenceString, int frameCount)
        {
            PageReplacementResult result = new PageReplacementResult
            {
                AlgorithmName = "LRU",
                PageFrameCount = frameCount,
                ExecutionLog = new List<string>()
            };

            // Use LinkedList for efficient removal from middle/end for LRU (or List with reordering)
            LinkedList<int> frames = new LinkedList<int>();
            HashSet<int> currentPages = new HashSet<int>(); // For quick lookup if page is in memory

            int pageFaults = 0;
            result.ExecutionLog.Add($"--- LRU Algorithm (Frames: {frameCount}) ---");
            result.ExecutionLog.Add($"Reference String: {string.Join(", ", referenceString)}");

            foreach (int pageNumber in referenceString)
            {
                string logLine = $"Page {pageNumber}: ";
                if (currentPages.Contains(pageNumber))
                {
                    logLine += "Hit! (Page already in memory) ";
                    // Move accessed page to the most recently used end
                    frames.Remove(pageNumber);
                    frames.AddLast(pageNumber);
                }
                else
                {
                    pageFaults++;
                    logLine += "PAGE FAULT! ";

                    if (frames.Count < frameCount)
                    {
                        // Frames available, add directly
                        frames.AddLast(pageNumber);
                        currentPages.Add(pageNumber);
                        logLine += "Frame available, added to memory. ";
                    }
                    else
                    {
                        // No frames available, replace LRU (front of the list)
                        int lruPage = frames.First.Value;
                        frames.RemoveFirst();
                        currentPages.Remove(lruPage);

                        frames.AddLast(pageNumber);
                        currentPages.Add(pageNumber);
                        logLine += $"Page {lruPage} replaced by Page {pageNumber}. ";
                    }
                }
                logLine += $"Current Frames: [{string.Join(", ", frames)}]";
                result.ExecutionLog.Add(logLine);
            }
            result.PageFaults = pageFaults;
            return result;
        }

        // --- Optimal Algorithm Implementation ---
        private PageReplacementResult RunOptimal(List<int> referenceString, int frameCount)
        {
            PageReplacementResult result = new PageReplacementResult
            {
                AlgorithmName = "Optimal",
                PageFrameCount = frameCount,
                ExecutionLog = new List<string>()
            } ;

            List<int> frames = new List<int>(); // Represents frames
            HashSet<int> currentPages = new HashSet<int>(); // For quick lookup

            int pageFaults = 0;
            result.ExecutionLog.Add($"--- Optimal Algorithm (Frames: {frameCount}) ---");
            result.ExecutionLog.Add($"Reference String: {string.Join(", ", referenceString)}");

            for (int i = 0; i < referenceString.Count; i++)
            {
                int pageNumber = referenceString[i];
                string logLine = $"Page {pageNumber}: ";

                if (currentPages.Contains(pageNumber))
                {
                    logLine += "Hit! (Page already in memory) ";
                }
                else
                {
                    pageFaults++;
                    logLine += "PAGE FAULT! ";

                    if (frames.Count < frameCount)
                    {
                        // Frames available, add directly
                        frames.Add(pageNumber);
                        currentPages.Add(pageNumber);
                        logLine += "Frame available, added to memory. ";
                    }
                    else
                    {
                        // Find page to replace using Optimal strategy
                        // Look ahead in the reference string to find the page that will be used furthest in the future
                        int pageToReplace = -1;
                        int furthestUse = -1;

                        foreach (int framePage in frames)
                        {
                            // Find next occurrence of this page in the remaining reference string
                            int nextUseIndex = -1;
                            for (int j = i + 1; j < referenceString.Count; j++)
                            {
                                if (referenceString[j] == framePage)
                                {
                                    nextUseIndex = j;
                                    break;
                                }
                            }

                            if (nextUseIndex == -1)
                            {
                                // This page will not be used again, so it's the best to replace
                                pageToReplace = framePage;
                                break;
                            }
                            else if (nextUseIndex > furthestUse)
                            {
                                // This page is used further in the future
                                furthestUse = nextUseIndex;
                                pageToReplace = framePage;
                            }
                        }

                        // Replace the page
                        currentPages.Remove(pageToReplace);
                        frames.Remove(pageToReplace);

                        frames.Add(pageNumber);
                        currentPages.Add(pageNumber);
                        logLine += $"Page {pageToReplace} replaced by Page {pageNumber}. ";
                    }
                }
                // Sort frames for consistent display
                frames.Sort();
                logLine += $"Current Frames: [{string.Join(", ", frames)}]";
                result.ExecutionLog.Add(logLine);
            }
            result.PageFaults = pageFaults;
            return result;
        }

        // --- No direct Stop/Pause methods for the calculation itself ---
        // As the calculation is intended to be fast and one-time.
        // The frontend will handle displaying results after calculation.
    }
}