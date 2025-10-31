using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class VacancyStat
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("vacancies")]
        public int Vacancies { get; set; }
    }
}
