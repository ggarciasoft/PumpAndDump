using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PumpAndDump.Shared
{
    public interface IProcess
    {
        ConcurrentDictionary<string, CompareMarketInformation> CompareMarketDic { get; set; }

        Task<bool> StartProcess();
        Task EndProcess();
        Task LoadProcess(Action<string> showLogAction);
    }
}
