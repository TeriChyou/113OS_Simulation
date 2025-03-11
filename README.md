# 2025 02 26

Initialized the project

## Plan

### Core Framework & UI

- UI => 1024 * 768 => Desktop, TaskBar, Windows(Including Apps, Terminal, Resource Monitor)

- Window Manager

- Multi-tasking UI => Allow multiple apps open at once.

- Simulated Kernel Layer:
  - Event-driven model => Handling Inputs, Processes, Resource Allocation
  - Simulate system calls => **API for processes to request OS services.**

### OS Core Components

- Process Manager:
  - New -> Ready -> Running -> Terminated
  - Simulated CPU scheduling algorithms.
  - Support multi-processes, allow users to create/terminal processes.
- Memory Management:
  - Visualize Memory Allocation (Chart View. Etc.)
  - Simulate paging & fragmentation (Allocate/Deallocate memory dynamically.)
  - Implement swapping (**Page replacement algorithms => multiprogramming.**)
- File System Simulation:
  - Simulate a hierarchical file system. (Folders, files, metadata)
  - Implement a simple virtual disk (Emulated using JSON/SQLite)
  - Allow file operations (**Create, read, write, delete**)
- I/O Device Management
  - Simulate I/O devices (keyboard, mouse, disk, printer, network)
  - Implement an I/O request queue (FIFO)
  - Show device status & active I/O requests.
- IPC (Inter-Process Communication)
  - Support basic message passing between processes. (Queues, Shared preferences...)
  - Simulate pipes, signals, **semaphores**. (Maybe mode semaphore?)
- Error Handling & System Logs
  - Detect common errors => memory overuse, inf. loops, deadlocks
  - Implement system logs => Note down mainor errors from major errors.

### Resource Monitroing & User Interaction

- Resource Monitor
  - Display CPU, Memory, Internet, Disk usage, running processes.
  - Module may use => **psutil**
- Interactive Terminal (Shell)
  - Implement a basic CLI.
  - Support commands => ls, cd, ps, kill, mkdir, rm, echo.
  - Allow process control => kill PID, nice PID, fg, bg.
- Task Manager (**GUI**)
  - List of running processes.
  - Allow users to force close processes.
  - Display resource usage per process.
- User Programs(**Fake Application => But functionale**)
  - Simple programs: Text Editor, Calculator, Notepad, Paint, maybe IDE (lol)
  - Simulated OS-level apps: System monitor, File Explorer, Terminal.

### The VM's info

- Hardware
  - Memory
    - RAM_CAP = 4 # GB
- Cache Memory
  - CACHE_L1 = 128  # KB
  - CACHE_L2 = 512  # KB
  - CACHE_L3 = 4  # MB
- System Bus Sepeed
  - SYSTEM_BUS_SPEED = 100  # MB/s
- CPU
  - CPU_CORES = 4
  - CPU_THREADS = 8
  - CPU_CLOCK = 1 # GHz
- SSD
  - SSD = 32 # GB
  - SSD_READ_SPEED = 500  # MB/s
  - SSD_WRITE_SPEED = 250  # MB/s
  - DISK_LATENCY = 5  # ms
- Internet
  - NETWORK_BANDWIDTH = 100  # Mbps
  - NETWORK_LATENCY = 20  # ms

## Processes today

Made a GUI and taskmanager, but is's not a good start.
So deleted it and made some basic hardware stuffs, then try to simulate CPU, RAM, Writing, Reading, Downloading stuffs.
Still a lot of researches and concepts to learn.

# 2025 03 05

Made => exam => midterm => 3 files for 3 questions.

# 2025 03 11

Modified exam_1 & 2, and then make the suspend and pause able to be viewed on the gannt chart.

