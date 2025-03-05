import matplotlib.pyplot as plt
from collections import deque
import random
import time

# 定義 Process 類別，代表一個行程
class Process:
    def __init__(self, pid, arrival_time, burst_time):
        self.pid = pid  # 行程 ID
        self.arrival_time = arrival_time  # 到達時間
        self.burst_time = burst_time  # 執行時間
        self.remaining_time = burst_time  # 剩餘時間（供 SRT 使用）
        self.waiting_time = 0  # 等待時間
        self.turnaround_time = 0  # 周轉時間
        self.completion_time = None  # 完成時間
        self.state = "Ready"  # 行程狀態
        self.program_counter = 0  # 程式計數器
        self.registers = {f'R{i}': random.randint(0, 100) for i in range(4)}  # 模擬暫存器
        self.memory_limit = random.randint(100, 500)  # 記憶體限制
        self.open_files = [f'file_{pid}_{i}.txt' for i in range(random.randint(1, 3))]  # 已開啟檔案表

# 定義 Scheduler 類別，負責行程排程
class Scheduler:
    def __init__(self, processes):
        self.processes = sorted(processes, key=lambda p: p.arrival_time)  # 依到達時間排序

    def run_fcfs(self):
        time_counter = 0
        schedule = []
        plt.ion()  # 開啟互動模式，允許即時更新圖表
        fig, ax = plt.subplots()
        fig.canvas.manager.set_window_title("FCFS執行過程")
        
        for process in self.processes:
            if time_counter < process.arrival_time:
                time_counter = process.arrival_time  # CPU 閒置時跳到該行程的到達時間
            
            process.state = "Running"
            process.program_counter = random.randint(1000, 5000)  # 模擬程式計數器變化
            process.waiting_time = time_counter - process.arrival_time
            process.turnaround_time = process.waiting_time + process.burst_time
            process.completion_time = time_counter + process.burst_time
            schedule.append((process.pid, time_counter, process.completion_time))
            
            self.display_pcb(process)
            self.update_gantt_chart(ax, schedule)
            plt.pause(1)  # 暫停 1 秒來模擬執行過程
            
            time_counter += process.burst_time  # 更新當前時間
            process.state = "Terminated"
            self.display_pcb(process)
        
        plt.ioff()
        plt.show()

    def update_gantt_chart(self, ax, schedule):
        ax.clear()
        for pid, start, end in schedule:
            ax.barh(pid, end - start, left=start, color='blue')
            ax.text((start + end) / 2, pid, pid, ha='center', va='center', color='white', fontsize=12, fontweight='bold')
        ax.set_xlabel("Time")
        ax.set_ylabel("Processes")
        ax.set_title("Gantt Chart - Execution in Progress")
        plt.draw()

    def display_pcb(self, process):
        print(f"\nPCB for Process {process.pid}: ")
        print(f"  State: {process.state}")
        print(f"  Process ID: {process.pid}")
        print(f"  Program Counter: {process.program_counter}")
        print(f"  Registers: {process.registers}")
        print(f"  Memory Limit: {process.memory_limit}")
        print(f"  Open Files: {process.open_files}")

# 測試行程資料
processes = [
    Process("A", 0, 5),
    Process("B", 2, 7),
    Process("C", 5, 10),
    Process("D", 7, 8),
    Process("E", 8, 15),
    Process("F", 12, 25),
    Process("G", 15, 12)
]

# 執行 FCFS 排程
scheduler = Scheduler(processes)
scheduler.run_fcfs()