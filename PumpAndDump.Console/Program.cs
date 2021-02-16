using PumpAndDump.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace PumpAndDump
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IList<IProcess> processes = new List<IProcess>
            {
                new BittrexProcess(),
                new BinanceProcess()
            };
            foreach (var process in processes)
            {
                await process.LoadProcess(o => Console.WriteLine(o));
                await process.StartProcess();
            }
            while (true)
            {
                var data = Console.ReadLine();
                var process = processes[int.Parse(data)];
                var list = process.CompareMarketDic.Select(o => o.Value).ToList();
                Console.WriteLine($"*******************{process.GetType().Name}**************************************");
                foreach (var marketInformation in list)
                {
                    Console.WriteLine(
                        $"LOOK:\n" +
                        $"Symbol: {marketInformation.Symbol}\n" +
                        $"Current price: {marketInformation.CurrentValue}\n" +
                        $"Checking price: {marketInformation.CheckingValue}\n" +
                        $"Difference price: {marketInformation.Difference}\n" +
                        $"Difference percentage: {marketInformation.DifferencePercentage}\n" +
                        $"Checking date: {marketInformation.CheckingTime:MM/dd/yyyy hh:mm:ss tt}\n");
                }
                Console.WriteLine("*********************************************************");
            }
        }
    }
}
