using Microsoft.AspNetCore.Mvc;
using FinalTermOS.Services; // 引入你的 Service 命名空間
using FinalTermOS.Models; // 引入你的 Model 命名空間 (如果 GetCurrentStatus 返回的是 SimulationStatus)

public class FinalTermController : Controller
{
    // 宣告一個 Service 的 private 變數
    private readonly SleepingTASimulationService _sleepingTAService;
    private readonly DiningPhilosophersSimulationService _diningPhilosophersService;
    private readonly BankersAlgorithmService _bankersAlgorithmService;
    private readonly VirtualMemorySimulationService _virtualMemoryService;

    // 建構子，透過依賴注入初始化 Service
    public FinalTermController(
        SleepingTASimulationService sleepingTAService,
        DiningPhilosophersSimulationService diningPhilosophersService,
        BankersAlgorithmService bankersAlgorithmService,
        VirtualMemorySimulationService virtualMemoryService
    )
    {
        _sleepingTAService = sleepingTAService;
        _diningPhilosophersService = diningPhilosophersService;
        _bankersAlgorithmService = bankersAlgorithmService;
        _virtualMemoryService = virtualMemoryService;
    }

    // 顯示 Sleeping TA 頁面的 Action
    public IActionResult SleepingTA()
    {
        ViewData["Title"] = "睡覺的助教 (Sleeping TA)";
        return View();
    }

    // 處理啟動模擬 POST 請求的 Action
    [HttpPost]
    public IActionResult StartSleepingTASimulation(int numberOfStudents, int numberOfChairs, bool loopStudents)
    {
        if (!_sleepingTAService.IsSimulationRunning && numberOfStudents > 0 && numberOfChairs >= 0)
        {
            _sleepingTAService.StartSimulation(numberOfStudents, numberOfChairs, loopStudents); // Pass parameter
            return Json(new { success = true, message = "模擬已啟動。" });
        }
        else if (_sleepingTAService.IsSimulationRunning)
        {
            return Json(new { success = false, message = "模擬已在運行中。" });
        }
        else
        {
            return Json(new { success = false, message = "請輸入有效的參數。" });
        }
    }
    // stop action
    [HttpPost] // Use POST for actions that change state
    public IActionResult StopSleepingTASimulation()
    {
        if (_sleepingTAService.IsSimulationRunning)
        {
            _sleepingTAService.StopSimulation();
            return Json(new { success = true, message = "模擬停止請求已收到。" });
        }
        else
        {
            return Json(new { success = false, message = "模擬未在運行中。" });
        }
    }
    // 新增 Action 來獲取模擬的當前狀態
    [HttpGet]
    public IActionResult GetSleepingTASimulationStatus()
    {
        // It's good practice to check if the simulation is running before getting status
        if (_sleepingTAService.IsSimulationRunning)
        {
            SleepingTASimulationStatus status = _sleepingTAService.GetCurrentStatus();
            return Json(status);
        }
        else
        {
            // Return a status indicating simulation is not running
            return Json(new { taState = (int)TAState.Sleeping, students = new List<StudentStatusDto>(), waitingQueueCount = 0, availableChairs = 0, isRunning = false, message = "模擬未運行" });
        }

    }

    // DiningPhilosophers
    // Action to display Dining Philosophers page
    public IActionResult DiningPhilosophers()
    {
        ViewData["Title"] = "哲學家進餐問題 (Dining Philosophers)";
        return View();
    }

    // Action to start Dining Philosophers simulation
    [HttpPost]
    public IActionResult StartDiningPhilosophersSimulation(int numberOfPhilosophers)
    {
        // Check if service is already running (assuming a similar singleton pattern for this service too)
        if (_diningPhilosophersService.IsSimulationRunning)
        {
            return Json(new { success = false, message = "模擬已在運行中。" });
        }

        if (numberOfPhilosophers >= 2)
        {
            // TODO: Call your DiningPhilosophersSimulationService to start the simulation
            _diningPhilosophersService.StartSimulation(numberOfPhilosophers);
            return Json(new { success = true, message = "哲學家進餐模擬已啟動。" });
        }
        else
        {
            return Json(new { success = false, message = "請輸入有效的哲學家數量 (至少 2)。" });
        }
    }

    // Action to stop Dining Philosophers simulation
    [HttpPost]
    public IActionResult StopDiningPhilosophersSimulation()
    {
        if (_diningPhilosophersService.IsSimulationRunning)
        {
            _diningPhilosophersService.StopSimulation();
            return Json(new { success = true, message = "哲學家進餐模擬停止請求已收到。" });
        }
        else
        {
            return Json(new { success = false, message = "模擬未在運行中。" });
        }
        // return Json(new { success = true, message = "哲學家進餐模擬停止請求已收到。(功能待實現)" }); // Placeholder
    }

    // Action to get Dining Philosophers simulation status
    [HttpGet]
    public IActionResult GetDiningPhilosophersSimulationStatus()
    {
        // TODO: Get actual status from DiningPhilosophersSimulationService
        if (_diningPhilosophersService.IsSimulationRunning)
        {
            DiningPhilosophersSimulationStatus status = _diningPhilosophersService.GetCurrentStatus();
            return Json(status);
        }
        else
        {
            return Json(new { isRunning = false, message = "哲學家進餐模擬未運行", philosophers = new List<PhilosopherStatusDto>(), forks = new List<ForkStatusDto>() });
        }
        // Placeholder for now
        // return Json(new { isRunning = false, message = "哲學家進餐模擬未運行 (功能待實現)", philosophers = new List<object>(), forks = new List<object>() });
    }
    // === NEW: Pause/Resume Actions ===
    [HttpPost]
    public IActionResult PauseDiningPhilosophersSimulation()
    {
        if (_diningPhilosophersService.IsSimulationRunning && !_diningPhilosophersService.IsPaused)
        {
            _diningPhilosophersService.PauseSimulation();
            return Json(new { success = true, message = "模擬已暫停。" });
        }
        else if (!_diningPhilosophersService.IsSimulationRunning)
        {
            return Json(new { success = false, message = "模擬未在運行中。" });
        }
        else
        {
            return Json(new { success = false, message = "模擬已暫停。" });
        }
    }

    [HttpPost]
    public IActionResult ResumeDiningPhilosophersSimulation()
    {
        if (_diningPhilosophersService.IsSimulationRunning && _diningPhilosophersService.IsPaused)
        {
            _diningPhilosophersService.ResumeSimulation();
            return Json(new { success = true, message = "模擬已恢復。" });
        }
        else if (!_diningPhilosophersService.IsSimulationRunning)
        {
            return Json(new { success = false, message = "模擬未在運行中。" });
        }
        else
        {
            return Json(new { success = false, message = "模擬未暫停。" });
        }
    }
    // Banker's Algorithm
    // Action to display Banker's Algorithm page
    public IActionResult BankersAlgorithm()
    {
        ViewData["Title"] = "銀行家演算法 (Banker's Algorithm)";
        return View();
    }

    // Action to execute Banker's Algorithm (equivalent to 'Start' for this problem)
    [HttpPost]
    public IActionResult ExecuteBankersAlgorithm()
    {
        // Banker's Algorithm is a static calculation, so we just execute and return results.
        // No need for a running flag, but the service provides it for consistency if needed.
        BankersAlgorithmStatus result = _bankersAlgorithmService.ExecuteAlgorithm();
        return Json(result);
    }

    // Action to get Banker's Algorithm status (useful for initial display or re-fetching)
    [HttpGet]
    public IActionResult GetBankersAlgorithmStatus()
    {
        BankersAlgorithmStatus status = _bankersAlgorithmService.GetCurrentStatus();
        return Json(status);
    }
    // Page Replacement Problem
    public IActionResult PageReplacement()
    {
        ViewData["Title"] = "虛擬記憶體頁面置換演算法 (Page Replacement)";
        return View();
    }

    // Action to start Virtual Memory simulation (triggers all calculations)
    [HttpPost]
    public IActionResult StartPageReplacementSimulation(int referenceStringLength, int maxPageNumber, int minFrameCount, int maxFrameCount)
    {
        if (_virtualMemoryService.IsSimulationRunning) // Check if a calculation is already in progress
        {
            return Json(new { success = false, message = "模擬正在運行中。" });
        }

        // You might want to add validation for input parameters here
        if (referenceStringLength <= 0 || maxPageNumber < 0 || minFrameCount <= 0 || maxFrameCount < minFrameCount)
        {
            return Json(new { success = false, message = "請輸入有效的參數。" });
        }


        // Execute the simulation (this will run all algorithms and store results)
        _virtualMemoryService.StartSimulation(referenceStringLength, maxPageNumber, minFrameCount, maxFrameCount);

        return Json(new { success = true, message = "頁面置換演算法計算已啟動。" });
    }

    // Action to get the overall status and results
    [HttpGet]
    public IActionResult GetPageReplacementStatus()
    {
        VirtualMemorySimulationStatus status = _virtualMemoryService.GetCurrentStatus();
        return Json(status);
    }

    // Action to get detailed trace for a specific algorithm and frame count
    [HttpGet]
    public IActionResult GetPageReplacementDetailedTrace(string algorithmName, int frameCount)
    {
        List<string> trace = _virtualMemoryService.GetDetailedTrace(algorithmName, frameCount);
        return Json(new { trace = trace, algorithmName = algorithmName, frameCount = frameCount });
    }
}