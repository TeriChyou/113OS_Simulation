import matplotlib.pyplot as plt
from collections import deque
import random

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
        
        # Ensure processes are sorted by arrival time before execution
        self.processes.sort(key=lambda p: p.arrival_time)

        for process in self.processes:
            # If CPU is idle, jump to the next process arrival time
            if self.time_counter < process.arrival_time:
                self.time_counter = process.arrival_time

            # Calculate process metrics
            process.waiting_time = self.time_counter - process.arrival_time
            process.turnaround_time = process.waiting_time + process.burst_time
            process.completion_time = self.time_counter + process.burst_time

            # Log scheduling data
            self.schedule.append((process.pid, self.time_counter, process.completion_time))

            # Advance time counter
            self.time_counter += process.burst_time

        self.display_avg_waiting_time("FCFS")
        self.display_gantt_chart("FCFS")
        
    def run_rr(self, quantum=3):
        self.reset()
        
        # Ensure processes are sorted by arrival time initially
        self.processes.sort(key=lambda p: p.arrival_time)
        
        queue = deque()
        index = 0  # To track the next arriving process
        n = len(self.processes)
        completed = 0

        while completed < n:
            # Add newly arrived processes to queue
            while index < n and self.processes[index].arrival_time <= self.time_counter:
                queue.append(self.processes[index])
                index += 1

            if queue:
                process = queue.popleft()
                execution_time = min(quantum, process.remaining_time)

                # Schedule process execution
                self.schedule.append((process.pid, self.time_counter, self.time_counter + execution_time))
                self.time_counter += execution_time
                process.remaining_time -= execution_time

                # Add newly arrived processes after execution
                while index < n and self.processes[index].arrival_time <= self.time_counter:
                    queue.append(self.processes[index])
                    index += 1

                # If process is not finished, put it back in the queue
                if process.remaining_time > 0:
                    queue.append(process)
                else:
                    process.completion_time = self.time_counter
                    process.turnaround_time = process.completion_time - process.arrival_time
                    process.waiting_time = process.turnaround_time - process.burst_time
                    completed += 1
            else:
                # If no process is ready, jump to the next process's arrival time
                self.time_counter = self.processes[index].arrival_time

        self.display_avg_waiting_time(f"RR (q={quantum})")
        self.display_gantt_chart(f"RR (q={quantum})")
        
    def run_sjf(self):
        self.reset()

        # Ensure processes are sorted by arrival time first
        self.processes.sort(key=lambda p: (p.arrival_time, p.burst_time))
        remaining_processes = self.processes[:]
        self.ready_queue = []
        total_waiting_time = 0
        total_turnaround_time = 0
        n = len(self.processes)

        while remaining_processes or self.ready_queue:
            # Move all processes that have arrived into the ready queue
            while remaining_processes and remaining_processes[0].arrival_time <= self.time_counter:
                self.ready_queue.append(remaining_processes.pop(0))

            # Sort the ready queue by burst time (Shortest Job First)
            self.ready_queue.sort(key=lambda p: p.burst_time)

            if self.ready_queue:
                # Pick the shortest job available
                process = self.ready_queue.pop(0)
                process.waiting_time = self.time_counter - process.arrival_time
                process.turnaround_time = process.waiting_time + process.burst_time
                process.completion_time = self.time_counter + process.burst_time

                # Accumulate waiting time and turnaround time for averaging
                total_waiting_time += process.waiting_time
                total_turnaround_time += process.turnaround_time

                # Schedule process execution
                self.schedule.append((process.pid, self.time_counter, process.completion_time))
                self.time_counter += process.burst_time
            else:
                # If no process is available, jump to the next arrival time
                self.time_counter = remaining_processes[0].arrival_time

        # Calculate and display average waiting time
        avg_waiting_time = total_waiting_time / n
        
        print(f"SJF - Average Waiting Time: {avg_waiting_time:.2f}")
        self.display_gantt_chart("SJF")

    def run_srt(self):
        self.reset()
        completed = 0
        remaining_processes = self.processes[:]

        while completed < len(self.processes):
            # 將已到達的行程加入 ready_queue
            while remaining_processes and remaining_processes[0].arrival_time <= self.time_counter:
                self.ready_queue.append(remaining_processes.pop(0))
            
            # 選擇剩餘時間最短的行程執行
            if self.ready_queue:
                self.ready_queue.sort(key=lambda p: p.remaining_time)
                process = self.ready_queue[0]  # 選擇當前剩餘時間最短的行程
                process.remaining_time -= 1
                self.schedule.append((process.pid, self.time_counter, self.time_counter + 1))
                self.time_counter += 1
                
                # 若行程執行完畢
                if process.remaining_time == 0:
                    process.completion_time = self.time_counter
                    process.turnaround_time = process.completion_time - process.arrival_time
                    process.waiting_time = process.turnaround_time - process.burst_time
                    self.ready_queue.pop(0)  # 從 ready_queue 移除已完成的行程
                    completed += 1
            else:
                self.time_counter += 1  # 若無行程可執行，時間前進

        self.display_avg_waiting_time("SRTF")
        self.display_gantt_chart("SRTF")
        
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
        self.processes.sort(key=lambda p: p.pid)  # Ensure ordering before computing
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
scheduler.run_sjf()
scheduler.run_srt()

leave = input("Press Enter to leave...")