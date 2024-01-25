using System.Collections.Generic;

namespace Nofun.PIP2
{
    public enum SpecialFunction
    {
        CreateTask = 0,
        DisposeTask = 1,
        Receive = 2,
        ReceiveAny = 3,
        Send = 4,
        SetStackSize = 5,
        TaskAlive = 6,
        ThisTask = 7
    }

    public static class SpecialFunctionUtils
    {
        private static Dictionary<string, SpecialFunction> _specialFunctionMap = new Dictionary<string, SpecialFunction>()
        {
            {"vCreateTask", SpecialFunction.CreateTask},
            {"vDisposeTask", SpecialFunction.DisposeTask},
            {"vReceive", SpecialFunction.Receive},
            {"vReceiveAny", SpecialFunction.ReceiveAny},
            {"vSend", SpecialFunction.Send},
            {"vSetStackSize", SpecialFunction.SetStackSize},
            {"vTaskAlive", SpecialFunction.TaskAlive},
            {"vThisTask", SpecialFunction.ThisTask}
        };

        public static bool IsSpecialFunction(string name)
        {
            return name != null && _specialFunctionMap.ContainsKey(name);
        }

        public static SpecialFunction GetSpecialFunction(string name)
        {
            return _specialFunctionMap[name];
        }
    }
}