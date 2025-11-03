using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using HtmlAgilityPack;
using DownloadJobsConsoleApp;
using System.Linq;
using System.Xml.Linq;

namespace DownloadJobsConsoleApp
{
    public static class RequestSearch
    {
        // API для вакансий (пример с HH.ru)
        private static readonly string BaseUrl = "https://api.hh.ru/vacancies";

        // Метод для получения ID всех вакансий по заданному поисковому запросу
        public static async Task<List<int>> GetVacancyIdsAsync()
        {
            Console.WriteLine("Введите полный URL для поиска вакансий с сайта HH.ru");
            Console.WriteLine("Пример: https://spb.hh.ru/search/vacancy?hhtmFrom=resume_search_result&hhtmFromLabel=vacancy_search_line&enable_snippets=false&industry=7&professional_role=157&search_field=name&search_field=company_name&search_field=description&text=");
            Console.WriteLine("Вы можете использовать конструктор запросов на сайте HH.ru и скопировать URL из адресной строки браузера");
            string userUrl = Console.ReadLine();

            // Преобразуем URL из браузерного формата в API формат
            string apiUrl = ConvertBrowserUrlToApiUrl(userUrl);

            if (string.IsNullOrEmpty(apiUrl))
            {
                Console.WriteLine("Не удалось преобразовать URL в формат API HH.ru");
                return new List<int>();
            }

            Console.WriteLine($"\nПреобразованный URL API: {apiUrl}");
            Console.WriteLine("\nНачинаем собирать вакансии...\n");

            var vacancyIds = new List<int>(); // Список для хранения ID вакансий

            try
            {
                using (var client = new HttpClient())
                {
                    // Устанавливаем заголовок для авторизации
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Profile.Token}");
                    client.DefaultRequestHeaders.Add("User-Agent", "RequirementsAnalysis/1.0");
                    client.DefaultRequestHeaders.Add("Host", "api.hh.ru");

                    int page = 0;
                    bool hasNextPage = true;

                    while (hasNextPage)
                    {
                        // Формируем URL с параметром страницы
                        string urlWithPage = apiUrl.Contains('?')
                            ? $"{apiUrl}&page={page}"
                            : $"{apiUrl}?page={page}";

                        // Отправляем запрос на получение вакансий по указанному запросу и странице
                        var response = await client.GetAsync(urlWithPage);
                        response.EnsureSuccessStatusCode();

                        // Получаем тело ответа от API
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var jobSearchResult = JsonConvert.DeserializeObject<JobSearchResult>(responseBody);

                        // Сохраняем ID вакансий в список
                        foreach (var vacancy in jobSearchResult.Items)
                        {
                            vacancyIds.Add(vacancy.Id);
                        }

                        Console.WriteLine($"Обработано {jobSearchResult.Items.Count} вакансий на странице {page + 1}");

                        // Проверяем, есть ли следующая страница
                        hasNextPage = jobSearchResult.Pages > page + 1;
                        page++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе вакансий: {ex.Message}");
            }

            Console.WriteLine($"\nВсего найдено вакансий: {vacancyIds.Count}");

            // Возвращаем список всех ID вакансий
            return vacancyIds;
        }

        // Метод для преобразования браузерного URL в API URL
        private static string ConvertBrowserUrlToApiUrl(string browserUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(browserUrl))
                    return null;

                // Если URL уже начинается с API, возвращаем как есть
                if (browserUrl.StartsWith("https://api.hh.ru/vacancies"))
                    return browserUrl;

                // Если это браузерный URL, преобразуем его
                if (browserUrl.Contains("hh.ru/search/vacancy"))
                {
                    // Извлекаем параметры запроса
                    var uri = new Uri(browserUrl);
                    var query = uri.Query;

                    // Базовый URL API
                    return $"https://api.hh.ru/vacancies{query}";
                }

                // Если это другой формат, который мы не ожидаем
                Console.WriteLine("Предупреждение: URL может быть в неподдерживаемом формате");
                return browserUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при преобразовании URL: {ex.Message}");
                return null;
            }
        }

        public static async Task LoadVacanciesAsync(List<int> jobIds)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Profile.Token}");
                client.DefaultRequestHeaders.Add("User-Agent", "YourAppName"); // Обязательно юзер-агент
                client.DefaultRequestHeaders.Host = "api.hh.ru"; // Или нужный хост, если требуется

                int processed = 0;
                int total = jobIds.Count;

                foreach (var jobId in jobIds)
                {
                    bool success = false;

                    while (!success)
                    {
                        try
                        {
                            var response = await client.GetAsync($"https://api.hh.ru/vacancies/{jobId}");
                            response.EnsureSuccessStatusCode();

                            var json = await response.Content.ReadAsStringAsync();
                            var vacancyRaw = JsonConvert.DeserializeObject<VacancyRaw>(json);

                            var vacancy = new VacancyRaw
                            {
                                Id = vacancyRaw.Id,
                                Name = vacancyRaw.Name,
                                Description = CleanHtml(vacancyRaw.Description),
                                BrandedDescription = CleanHtml(vacancyRaw.BrandedDescription),
                                KeySkills = vacancyRaw.KeySkills
                            };

                            ManagerCsv.WriteReport(vacancy);
                            processed++;
                            Console.WriteLine($"Обработано вакансий: {processed}/{total}");

                            // Рандомная задержка
                            Random random = new Random();
                            int delay = random.Next(3000, 7001);
                            await Task.Delay(delay);

                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при загрузке вакансии {jobId}: {ex.Message}");
                            string input;
                            do
                            {
                                Console.Write("Повторить попытку? (y/n): ");
                                input = Console.ReadLine()?.Trim().ToLower();
                            }
                            while (input != "y" && input != "n");

                            if (input == "n")
                            {
                                Console.WriteLine("Пропускаем вакансию.");
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Очистка от HTML
        public static string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText;
        }

    }

    // Классы для десериализации ответа от API
    public class JobSearchResult
    {
        [JsonProperty("items")]
        public List<Vacancy> Items { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }
    }

    public class Vacancy
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

}