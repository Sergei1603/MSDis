using Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class Request
    {
        public string httpMethod { get; set; }
        public string body { get; set; }
    }

    public class Response
    {
        public Response(int statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public int StatusCode { get; set; }
        public string Body { get; set; }
    }


    public class Handler
    {

        public async Task<Response> FunctionHandler(Request request)
        {
            try
            {
                var stats = new StatsProvider();
                var res = await stats.GetStats();
                return new Response(200, res);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return new Response(404, "Not Found");
            }
        }
    }
}
