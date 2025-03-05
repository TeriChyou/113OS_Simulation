class Process:
    def __init__(self, pid, program_counter, register, memory, open_files, arrival_time, burst_time, waiting_time, turnaround_time):
        self.status = "new" # new, ready, running, waiting, terminated
        self.program_counter = program_counter # 系統中的程序計數器
        self.pid = pid # 進程ID
        self.register = register # 進程的寄存器
        self.memory = memory # 進程的記憶體
        self.open_files = open_files # 進程的打開文件
        self.arrival_time = arrival_time # 進程的到達時間
        self.burst_time = burst_time # 進程的執行時間
        self.waiting_time = waiting_time # 進程的等待時間
        self.turnaround_time = turnaround_time # 進程的周轉時間