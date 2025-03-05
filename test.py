import time
import threading
import random

class Process:
    def __init__(self, pid, interactive=False):
        self.pid = pid  # Process ID
        self.state = "Ready"
        self.arrival_time = time.time()
        self.start_time = None
        self.completion_time = None
        self.burst_time = random.randint(2, 5) if not interactive else None  # Fixed burst time for non-interactive
        self.interactive = interactive  # True for long-running interactive processes

    def run(self):
        self.state = "Running"
        self.start_time = time.time()
        if self.interactive:
            print(f"Process {self.pid} (Interactive) started and running indefinitely... Press Ctrl+C to stop.")
            while True:
                time.sleep(1)  # Simulating waiting for user input or events
        else:
            print(f"Process {self.pid} started. Running for {self.burst_time} seconds...")
            time.sleep(self.burst_time)  # Simulate process execution time
            self.completion_time = time.time()
            self.state = "Terminated"
            print(f"Process {self.pid} completed in {self.completion_time - self.start_time:.2f} seconds.")

# Simulating two types of processes
short_process = Process(1)  # Normal finite burst process
interactive_process = Process(2, interactive=True)  # Infinite process like Word

# Running short process in a thread
threading.Thread(target=short_process.run).start()

# Running interactive process (Simulates Word-like application)
threading.Thread(target=interactive_process.run).start()
