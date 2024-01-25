using System.Collections.Generic;
using Nofun.PIP2;
using Nofun.Util;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private StackAllocator stackAllocator;
        private SimpleObjectManager<TaskInfo> taskInfos = new();
        private List<TaskInfo> taskQueue = new();
        private long taskStackSize = -1;
        private TaskInfo currentTask = null;

        public StackAllocator StackAllocator => stackAllocator;

        private void InitializeTasks()
        {
            this.stackAllocator = new StackAllocator(system.TaskStackSectionStart, system.Executable.Header.stackSize,
                system.TaskStackSectionSize);

            CreateMainTask();
        }

        /// <summary>
        /// Save the context of the current task from the processor.
        /// </summary>
        private void SaveCurrentTask()
        {
            if (currentTask == null)
            {
                return;
            }

            for (var i = 1; i < Register.TotalReg; i++)
            {
                currentTask.taskContext[i] = system.Processor.Reg[(uint)(i << 2)];
            }
        }

        private void ScheduleTask(TaskInfo info)
        {
            taskQueue.Add(info);
        }

        private void RunNextTask()
        {
            if (taskQueue.Count == 0)
            {
                vTerminateVMGP();
                return;
            }

            SaveCurrentTask();

            TaskInfo nextTask = taskQueue[0];
            taskQueue.RemoveAt(0);

            currentTask = nextTask;

            for (var i = 1; i < Register.TotalReg; i++)
            {
                system.Processor.Reg[(uint)(i << 2)] = nextTask.taskContext[i];
            }
        }

        private void CreateMainTask()
        {
            TaskInfo mainTask = new TaskInfo();
            mainTask.entryPoint = 0;
            mainTask.stackAddress = 0;

            int handle = taskInfos.Add(mainTask);
            mainTask.taskID = handle;

            currentTask = mainTask;
        }

        [ModuleCall]
        private int vCreateTask(uint addr, uint p0, uint p1, uint p2)
        {
            TaskInfo info = new TaskInfo();

            info.entryPoint = addr;
            info.stackAddress = stackAllocator.Allocate(taskStackSize);

            info.taskContext[Register.P0 >> 2] = p0;
            info.taskContext[Register.P1 >> 2] = p1;
            info.taskContext[Register.P2 >> 2] = p2;
            info.taskContext[Register.SP >> 2] = info.stackAddress;
            info.taskContext[Register.RA >> 2] = system.TaskTerminateSubroutineAddress;
            info.taskContext[Register.PC >> 2] = addr;

            int taskHandle = taskInfos.Add(info);
            info.taskID = taskHandle;

            ScheduleTask(info);
            return taskHandle;
        }

        [ModuleCall]
        private void vDisposeTask(int handle)
        {
            TaskInfo info = taskInfos.Get(handle);
            if (info == null)
            {
                throw new System.Exception("Task not found.");
            }

            if (info.stackAddress != 0)
            {
                stackAllocator.Free(info.stackAddress);
            }

            taskInfos.Remove(handle);
            taskQueue.Remove(info);
        }

        [ModuleCall]
        private void vSend(int handle, int data)
        {
            TaskInfo info = taskInfos.Get(handle);
            if (info == null)
            {
                throw new System.Exception("Task not found.");
            }

            info.receivedData = data;
        }

        [ModuleCall]
        private int vReceiveAny(int handle)
        {
            TaskInfo info = taskInfos.Get(handle);
            if (info == null)
            {
                throw new System.Exception("Task not found.");
            }

            return info.receivedData;
        }

        [ModuleCall]
        private int vReceive()
        {
            if (currentTask == null)
            {
                throw new System.Exception("No task is currently running!");
            }

            return currentTask.receivedData;
        }

        [ModuleCall]
        private int vTaskAlive(int handle)
        {
            return taskInfos.Get(handle) != null ? 1 : 0;
        }

        [ModuleCall]
        private int vThisTask()
        {
            return currentTask.taskID;
        }

        [ModuleCall]
        private uint vSetStackSize(uint size)
        {
            taskStackSize = MemoryUtil.AlignUp(size, 4);
            return (uint)taskStackSize;
        }

        public void YieldTask()
        {
            ScheduleTask(currentTask);
            RunNextTask();
        }

        public void TerminateTask()
        {
            taskInfos.Remove(currentTask.taskID);
            taskQueue.Remove(currentTask);

            if (currentTask.stackAddress != 0)
            {
                stackAllocator.Free(currentTask.stackAddress);
            }

            currentTask = null;
            RunNextTask();
        }
    }
}