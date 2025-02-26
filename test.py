import time
import threading
import random
from consts import *

class CPU:
    def __init__(self, cores, threads, clock_speed):
        self.cores = cores
        self.threads = threads
        self.clock_speed = clock_speed  # GHz
        self.utilization = [0] * cores  # Track CPU usage per core

    def execute_task(self, process_name, cycles_required):
        """Simulate execution of a process based on CPU speed."""
        cycles_per_second = self.clock_speed * 1_000_000_000
        execution_time = cycles_required / cycles_per_second
        core = random.randint(0, self.cores - 1)  # Assign a random core
        self.utilization[core] += 10  # Simulating CPU load
        
        print(f"Executing {process_name} on Core {core} ({cycles_required} cycles). ETA: {execution_time:.4f}s")
        time.sleep(execution_time)
        self.utilization[core] -= 10  # Free up CPU after execution

    def get_usage(self):
        return self.utilization

# Initialize CPU
cpu = CPU(CPU_CORES, CPU_THREADS, CPU_CLOCK)
cpu.execute_task("Task_A", 2_000_000_000)  # Task requiring 2 billion cycles

class RAM:
    def __init__(self, size_gb):
        self.total_memory = size_gb * 1024  # Convert GB to MB
        self.used_memory = 0
        self.memory_map = {}  # {pid: memory_allocated}

    def allocate(self, pid, size_mb):
        if self.used_memory + size_mb > self.total_memory:
            print(f"Memory Allocation Failed: Not enough RAM for PID {pid}")
            return False
        self.memory_map[pid] = size_mb
        self.used_memory += size_mb
        print(f"Allocated {size_mb}MB to Process {pid}.")
        return True

    def deallocate(self, pid):
        if pid in self.memory_map:
            size_mb = self.memory_map.pop(pid)
            self.used_memory -= size_mb
            print(f"Deallocated {size_mb}MB from Process {pid}.")
            return True
        return False

    def get_usage(self):
        return self.used_memory, self.total_memory

# Initialize RAM
ram = RAM(RAM_CAP)
ram.allocate(1, 512)  # Allocate 512MB to process 1
ram.allocate(2, 1024) # Allocate 1GB to process 2

class Disk:
    def __init__(self, size_gb, read_speed, write_speed, latency):
        self.total_storage = size_gb * 1024  # Convert GB to MB
        self.used_storage = 0
        self.read_speed = read_speed
        self.write_speed = write_speed
        self.latency = latency
        self.files = {}  # Simulated file system

    def write(self, filename, size_mb):
        if self.used_storage + size_mb > self.total_storage:
            print(f"Disk Full: Cannot write {filename}")
            return False
        self.files[filename] = size_mb
        self.used_storage += size_mb
        write_time = size_mb / self.write_speed + (self.latency / 1000)
        print(f"Writing {filename}... Time: {write_time:.2f}s")
        time.sleep(write_time)
        return True

    def read(self, filename):
        if filename not in self.files:
            print("File Not Found!")
            return False
        size_mb = self.files[filename]
        read_time = size_mb / self.read_speed + (self.latency / 1000)
        print(f"Reading {filename}... Time: {read_time:.2f}s")
        time.sleep(read_time)
        return True

    def delete(self, filename):
        if filename in self.files:
            self.used_storage -= self.files[filename]
            del self.files[filename]
            print(f"Deleted {filename}.")
            return True
        return False

# Initialize SSD
disk = Disk(SSD, SSD_READ_SPEED, SSD_WRITE_SPEED, DISK_LATENCY)
disk.write("game.iso", 1024)  # Write 1GB file
disk.read("game.iso")  # Read file

class Network:
    def __init__(self, bandwidth, latency):
        self.bandwidth = bandwidth  # Mbps
        self.latency = latency  # ms

    def download(self, file_size_mb):
        download_time = (file_size_mb * 8) / self.bandwidth  # Convert MB to Mb
        download_time += self.latency / 1000  # Convert ms to seconds
        print(f"Downloading {file_size_mb}MB... ETA: {download_time:.2f}s")
        time.sleep(download_time)

network = Network(NETWORK_BANDWIDTH, NETWORK_LATENCY)
network.download(500)  # Simulate downloading a 500MB file
