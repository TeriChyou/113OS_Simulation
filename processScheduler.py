import matplotlib.pyplot as plt
from collections import deque

# 定義 Process 類別，代表一個行程
class Process:
    def __init__(self, pid, arrival_time, burst_time):
        self.pid = pid  # 行程 ID
        self.arrival_time = arrival_time  # 到達時間
        self.burst_time = burst_time  # 執行時間
        self.remaining_time = burst_time  # 剩餘時間（供 SRT 使用）
        self.waiting_time = 0  # 等待時間
        self.turnaround_time = 0  # 周轉時間
        self.completion_time = 0  # 完成時間

# 定義 Scheduler 類別，負責行程排程
class Scheduler:
    def __init__(self, processes):
        self.processes = sorted(processes, key=lambda p: p.arrival_time)  # 依到達時間排序

    def run_fcfs(self):
        time = 0
        schedule = []
        for process in self.processes:
            if time < process.arrival_time:
                time = process.arrival_time  # CPU 閒置時跳到該行程的到達時間
            process.waiting_time = time - process.arrival_time
            process.turnaround_time = process.waiting_time + process.burst_time
            process.completion_time = time + process.burst_time
            schedule.append((process.pid, time, process.completion_time))
            time += process.burst_time  # 更新當前時間
        self.draw_gantt_chart(schedule)

    def draw_gantt_chart(self, schedule):
        fig, ax = plt.subplots()
        for pid, start, end in schedule:
            ax.barh(pid, end - start, left=start)
            ax.text((start + end) / 2, pid, pid, ha='center', va='center', color='white', fontsize=12, fontweight='bold')
        plt.xlabel("Time")
        plt.ylabel("Processes")
        plt.title("Gantt Chart for FCFS Scheduling")
        plt.show()

# 測試行程資料
processes = [
    Process("A", 0, 5),
    Process("B", 2, 7),
    Process("C", 4, 10),
    Process("D", 6, 3),
    Process("E", 8, 12)
]

# 執行 FCFS 排程
scheduler = Scheduler(processes)
scheduler.run_fcfs()
