using System;
using System.Threading.Tasks;

namespace Function
{
    internal class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                var stats = new StatsProvider();
                var res = await stats.GetStats();
                Console.WriteLine(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
