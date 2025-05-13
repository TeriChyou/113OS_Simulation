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
    public IActionResult StartSleepingTASimulation(int numberOfStudents, int numberOfChairs)
    {
        if (numberOfStudents > 0 && numberOfChairs >= 0)
        {
            // 呼叫 Service 來啟動模擬
            _sleepingTAService.StartSimulation(numberOfStudents, numberOfChairs);
            // 返回成功訊息
            return Json(new { success = true, message = "模擬已啟動。" });
        }
        else
        {
            return Json(new { success = false, message = "請輸入有效的參數。" });
        }
    }

    // 新增 Action 來獲取模擬的當前狀態
    [HttpGet]
    public IActionResult GetSleepingTASimulationStatus()
    {
        // 從 Service 獲取當前狀態
        SleepingTASimulationStatus status = _sleepingTAService.GetCurrentStatus();
        // 將狀態作為 JSON 回傳給前端
        return Json(status);
    }
}