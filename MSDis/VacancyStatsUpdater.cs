using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class VacancyStatsUpdater
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

        JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd",
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task UpdateStats()
        {
            var stats = await GetStatsFromHH();
            var fileStream = await GetFileFromObjectStorage();
            var updatedFile = await UpdateFile(fileStream, stats);
            await UploadFile(updatedFile);
        }


        public async Task<int> GetStatsFromHH()
        {
            try
            {
                const string url = "https://api.hh.ru/vacancies?text=C%23%20Developer&schedule=remote&per_page=100";
                using (var _httpClient = new HttpClient())
                {
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                        "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string data = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(data);

                    if (!json.ContainsKey("found"))
                        throw new InvalidDataException("Поле 'found' не найдено");

                    return json["found"]!.Value<int>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения статистики с HH: {ex.Message}", ex);
            }
        }


        public async Task<Stream> GetFileFromObjectStorage()
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
                    var response = await client.GetObjectAsync(request);
                    return response.ResponseStream;
                }
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
            {
                Console.WriteLine("Файл не существует, создание нового");
                return new MemoryStream();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения файла из хранилища {ex.Message}", ex);
            }
        }

        public async Task<string> UpdateFile(Stream fileStream, int stats)
        {
            try
            {
                List<VacancyStat> vacancyStats = new List<VacancyStat>();

                if (fileStream != null && fileStream.CanRead)
                {
                    using var reader = new StreamReader(fileStream);
                    string content = await reader.ReadToEndAsync();
                    vacancyStats = JsonConvert.DeserializeObject<List<VacancyStat>>(content, _jsonSettings)
                                 ?? vacancyStats;
                }

                var today = DateTime.Today;
                var existingItem = vacancyStats.FirstOrDefault(x => x.Date == today);

                if (existingItem != null)
                {
                    existingItem.Vacancies = stats;
                }
                else
                {
                    vacancyStats.Add(new VacancyStat
                    {
                        Date = today,
                        Vacancies = stats
                    });
                }

                return JsonConvert.SerializeObject(vacancyStats, _jsonSettings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось обновить файл: {ex.Message}", ex);
            }
        }


        public async Task UploadFile(string fileContent)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = FileName,
                    ContentBody = fileContent,
                    ContentType = "application/json",
                    StorageClass = S3StorageClass.Standard
                };
                using (IAmazonS3 client = new AmazonS3Client(AccessKey, SecretKey, config))
                {
                    var ans = await client.PutObjectAsync(putRequest);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при созранении файла в хранилище: {ex.Message}", ex);
            }
        }
    }
}
