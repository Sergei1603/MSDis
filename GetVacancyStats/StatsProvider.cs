using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class StatsProvider
    {
        private const string BucketName = "object-storage";
        private const string FileName = "vacancies_stats.json";

        private static string AccessKey => Environment.GetEnvironmentVariable("ACCESS_KEY")
                                 ?? throw new InvalidOperationException("ACCESS_KEY not set");

        private static string SecretKey => Environment.GetEnvironmentVariable("SECRET_KEY")
                                         ?? throw new InvalidOperationException("SECRET_KEY not set");

        AmazonS3Config config = new AmazonS3Config
        {
            ServiceURL = "https://storage.yandexcloud.net",
            ForcePathStyle = true
        };

        public async Task<string> GetStats()
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = BucketName,
                    Key = FileName
                };
                IAmazonS3 client = new AmazonS3Client(AccessKey, SecretKey, config);
                {
                    var response = client.GetObjectAsync(request).Result;
                    using var reader = new StreamReader(response.ResponseStream);
                    string content = await reader.ReadToEndAsync();
                    return content;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения файла из хранилища {ex.Message}", ex);
            }
        }
    }
}
