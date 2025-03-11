import matplotlib.pyplot as plt
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

# 定義 Scheduler 類別，負責 SJF 排程
class Scheduler:
    def __init__(self, processes):
        self.processes = sorted(processes, key=lambda p: (p.arrival_time, p.burst_time))
        self.time_counter = 0
        self.schedule = []
        self.ready_queue = []
        self.interrupted_processes = {"D", "G"}  # D 和 G 會被中斷
        self.suspended_processes = set()  # 記錄已被中斷的行程

    def run_sjf(self):
        plt.ion()
        fig, ax = plt.subplots()
        fig.canvas.manager.set_window_title("SJF 執行過程")

        while self.processes or self.ready_queue:
            # 將到達的行程加入就緒隊列
            while self.processes and self.processes[0].arrival_time <= self.time_counter:
                self.ready_queue.append(self.processes.pop(0))
                self.ready_queue.sort(key=lambda p: p.burst_time)
            
            if self.ready_queue:
                process = self.ready_queue.pop(0)
                self.update_gantt_chart(ax, self.schedule, process)
                self.display_pcb(process)
                plt.pause(1)
                process.state = "Running"
                process.program_counter = random.randint(1000, 5000)
                process.waiting_time = self.time_counter - process.arrival_time
                process.turnaround_time = process.waiting_time + process.burst_time
                
                # 如果是 D 或 G，則執行一部分後中斷
                if process.pid in self.interrupted_processes and process.pid not in self.suspended_processes:
                    interrupt_time = 2  # 模擬 2 單位時間的執行後中斷
                    self.display_pcb(process)
                    self.update_gantt_chart(ax, self.schedule, process)
                    plt.pause(1)
                    print(f"Process {process.pid} is INTERRUPTED (Printing a file)... Moving to Suspended state.")
                    process.state = "Suspended"
                    self.schedule.append((process.pid, self.time_counter, self.time_counter + interrupt_time, 'red'))
                    self.display_pcb(process)
                    self.update_gantt_chart(ax, self.schedule, process)
                    plt.pause(2)
                    process.remaining_time -= interrupt_time
                    self.time_counter += interrupt_time
                    process.state = "Ready"
                    self.suspended_processes.add(process.pid)
                    self.ready_queue.insert(0, process)  # 立即回到 ready queue 頂端
                else:
                    process.completion_time = self.time_counter + process.remaining_time
                    self.schedule.append((process.pid, self.time_counter, process.completion_time, 'blue'))
                    self.display_pcb(process)
                    self.update_gantt_chart(ax, self.schedule, process)
                    plt.pause(1)
                    self.time_counter += process.remaining_time
                    process.state = "Terminated"
                    self.update_gantt_chart(ax, self.schedule, process)
                    self.display_pcb(process)
            else:
                self.time_counter += 1
        
        plt.ioff()
        plt.show()

    def update_gantt_chart(self, ax, schedule, current_process):
        ax.clear()
        for pid, start, end, color in schedule:
            ax.barh(pid, end - start, left=start, color=color)
            ax.text((start + end) / 2, pid, pid, ha='center', va='center', color='white', fontsize=12, fontweight='bold')
        
        ax.set_xlabel("Time")
        ax.set_ylabel("Processes")
        ax.set_title(f"Gantt Chart - SJF Execution (Current: {current_process.pid} : {current_process.state})")
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
    Process("D", 7, 8),  # 會被中斷
    Process("E", 8, 15),
    Process("F", 12, 25),
    Process("G", 15, 12)  # 會被中斷
]

# 執行 SJF 排程
scheduler = Scheduler(processes)
scheduler.run_sjf()
