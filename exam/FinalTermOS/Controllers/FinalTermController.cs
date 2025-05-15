using Microsoft.AspNetCore.Mvc;
using FinalTermOS.Services; // 引入你的 Service 命名空間
using FinalTermOS.Models; // 引入你的 Model 命名空間 (如果 GetCurrentStatus 返回的是 SimulationStatus)

public class FinalTermController : Controller
{
    // 宣告一個 Service 的 private 變數
    private readonly SleepingTASimulationService _sleepingTAService;

    // 建構子，透過依賴注入初始化 Service
    public FinalTermController(SleepingTASimulationService sleepingTAService)
    {
        _sleepingTAService = sleepingTAService;
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
}