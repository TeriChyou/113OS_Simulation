import fcfsPCB
import interruptSJF
import schedulingMethod

def main():
    while True:
        x = input("請輸入要執行的程式:\n1: 以 FCFS 呈現甘特圖及 PCB\n2: 以 SJF 呈現甘特圖及 PCB 並產生中斷\n3: 以四種排程排序呈現甘特圖及平均等待時間\n選擇 (1/2/3): ")
        
        if x == "1":
            fcfsPCB.main()
        elif x == "2":
            interruptSJF.main()
        elif x == "3":
            schedulingMethod.main()
        else:
            print("輸入錯誤，請重新輸入")
            continue
        
        # 詢問使用者是否繼續
        exit_choice = input("是否要結束模擬呈現? (Y/N): ").strip().lower()
        if exit_choice == "y":
            print("結束模擬...\n")
            break
        elif exit_choice == "n":
            print("返回選單...\n")
        else:
            print("無效輸入，預設返回選單...\n")

if __name__ == "__main__":
    main()
