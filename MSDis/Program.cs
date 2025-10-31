using System;
using System.Threading.Tasks;


namespace Function
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                var stats = new VacancyStatsUpdater();
                await stats.UpdateStats();
                Console.WriteLine($"OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
               
            }
        }
    }

}
