import matplotlib.pyplot as plt
from collections import deque
import random
import time

# 定義 Process 類別
class Process:
    def __init__(self, pid, arrival_time, burst_time):
        self.pid = pid
        self.arrival_time = arrival_time
        self.burst_time = burst_time
        self.remaining_time = burst_time
        self.waiting_time = 0
        self.turnaround_time = 0
        self.completion_time = None
        self.state = "Ready"
        self.program_counter = 0
        self.registers = {f'R{i}': random.randint(0, 100) for i in range(4)}
        self.memory_limit = random.randint(100, 500)
        self.open_files = [f'file_{pid}_{i}.txt' for i in range(random.randint(1, 3))]

# 定義 Scheduler 類別，負責不同的排程模式
class Scheduler:
    def __init__(self, processes):
        self.processes = sorted(processes, key=lambda p: p.arrival_time)
        self.time_counter = 0
        self.schedule = []
        self.ready_queue = []

    def run_fcfs(self):
        self.reset()
        for process in self.processes:
            if self.time_counter < process.arrival_time:
                self.time_counter = process.arrival_time
            process.waiting_time = self.time_counter - process.arrival_time
            process.turnaround_time = process.waiting_time + process.burst_time
            process.completion_time = self.time_counter + process.burst_time
            self.schedule.append((process.pid, self.time_counter, process.completion_time))
            self.time_counter += process.burst_time
        self.display_gantt_chart("FCFS")
        self.display_avg_waiting_time("FCFS")

    def run_rr(self, quantum=3):
        self.reset()
        queue = deque(self.processes)
        while queue:
            process = queue.popleft()
            if self.time_counter < process.arrival_time:
                self.time_counter = process.arrival_time
            execution_time = min(quantum, process.remaining_time)
            self.schedule.append((process.pid, self.time_counter, self.time_counter + execution_time))
            self.time_counter += execution_time
            process.remaining_time -= execution_time
            if process.remaining_time > 0:
                queue.append(process)
            else:
                process.completion_time = self.time_counter
                process.turnaround_time = process.completion_time - process.arrival_time
                process.waiting_time = process.turnaround_time - process.burst_time
        self.display_gantt_chart("RR (q=3)")
        self.display_avg_waiting_time("RR (q=3)")

    def run_spn(self):
        self.reset()
        queue = sorted(self.processes, key=lambda p: (p.arrival_time, p.burst_time))
        while queue:
            process = queue.pop(0)
            if self.time_counter < process.arrival_time:
                self.time_counter = process.arrival_time
            process.waiting_time = self.time_counter - process.arrival_time
            process.turnaround_time = process.waiting_time + process.burst_time
            process.completion_time = self.time_counter + process.burst_time
            self.schedule.append((process.pid, self.time_counter, process.completion_time))
            self.time_counter += process.burst_time
        self.display_gantt_chart("SPN")
        self.display_avg_waiting_time("SPN")

    def run_srt(self):
        self.reset()
        queue = []
        remaining_processes = self.processes[:]
        while queue or remaining_processes:
            while remaining_processes and remaining_processes[0].arrival_time <= self.time_counter:
                queue.append(remaining_processes.pop(0))
                queue.sort(key=lambda p: p.remaining_time)
            if queue:
                process = queue.pop(0)
                execution_time = 1
                self.schedule.append((process.pid, self.time_counter, self.time_counter + execution_time))
                self.time_counter += execution_time
                process.remaining_time -= execution_time
                if process.remaining_time > 0:
                    queue.append(process)
                else:
                    process.completion_time = self.time_counter
                    process.turnaround_time = process.completion_time - process.arrival_time
                    process.waiting_time = process.turnaround_time - process.burst_time
            else:
                self.time_counter += 1
        self.display_gantt_chart("SRT")
        self.display_avg_waiting_time("SRT")

    def reset(self):
        self.time_counter = 0
        self.schedule = []
        for process in self.processes:
            process.remaining_time = process.burst_time
            process.waiting_time = 0
            process.turnaround_time = 0
            process.completion_time = None

    def display_gantt_chart(self, title):
        fig, ax = plt.subplots()
        fig.canvas.manager.set_window_title(title)
        for pid, start, end in self.schedule:
            ax.barh(pid, end - start, left=start)
            ax.text((start + end) / 2, pid, pid, ha='center', va='center', color='white', fontsize=12, fontweight='bold')
        plt.xlabel("Time")
        plt.ylabel("Processes")
        plt.title(f"Gantt Chart - {title}")
        plt.show()

    def display_avg_waiting_time(self, title):
        avg_waiting_time = sum(p.waiting_time for p in self.processes) / len(self.processes)
        print(f"{title} - Average Waiting Time: {avg_waiting_time:.2f}")

# 測試行程資料
processes = [
    Process("A", 0, 4),
    Process("B", 2, 9),
    Process("C", 5, 6),
    Process("D", 10, 12),
    Process("E", 15, 20)
]

# 執行不同的排程方式
scheduler = Scheduler(processes)
scheduler.run_fcfs()
scheduler.run_rr(quantum=3)
scheduler.run_spn()
scheduler.run_srt()
