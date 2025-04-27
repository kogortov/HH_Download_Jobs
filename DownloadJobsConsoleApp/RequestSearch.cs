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
        // Поисковый запрос (название вакансии)
        public static string Text { get; set; }

        // Поисковый запрос (регион)
        public static string Region { get; set; }


        // Поисковый запрос (отрасль компании)
        public static string Industry { get; set; }

       
        // API для вакансий (пример с HH.ru)
        private static readonly string BaseUrl = "https://api.hh.ru/vacancies";

        // Метод для получения ID всех вакансий по заданному поисковому запросу
        public static async Task<List<int>> GetVacancyIdsAsync()
        {
            Console.WriteLine("Введите название вакансии*.");
            Text = Console.ReadLine();

            // Проверяем, что строка не пуста, если пуста — просим ввести повторно
            while (string.IsNullOrEmpty(Text))
            {
                Console.WriteLine("Название вакансии не может быть пустым. Пожалуйста, введите название вакансии.");
                Text = Console.ReadLine();
            }

            Console.WriteLine("Введите id региона.");
            Region = Console.ReadLine();

            Console.WriteLine("Введите id отрасли компании.");
            Industry = Console.ReadLine();

            var vacancyIds = new List<int>(); // Список для хранения ID вакансий
            string url = $"{BaseUrl}?text={Text}"; // Формируем URL для поиска вакансий

            // Добавляем параметр для региона, если он не пустой
            if (!string.IsNullOrEmpty(Region))
            {
                url += $"&area={Region}";
            }

            // Добавляем параметр для отрасли, если он не пустой
            if (!string.IsNullOrEmpty(Industry))
            {
                url += $"&industry={Industry}";
            }

            Console.WriteLine("\nНачинаем собирать вакансии...\n");
            

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
                        // Отправляем запрос на получение вакансий по указанному запросу и странице
                        var response = await client.GetAsync($"{url}&page={page}");
                        response.EnsureSuccessStatusCode();

                        // Получаем тело ответа от API
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var jobSearchResult = JsonConvert.DeserializeObject<JobSearchResult>(responseBody);

                        // Сохраняем ID вакансий в список
                        foreach (var vacancy in jobSearchResult.Items)
                        {
                            vacancyIds.Add(vacancy.Id);
                        }

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

            // Возвращаем список всех ID вакансий
            return vacancyIds;
        }


        public static async Task LoadVacanciesAsync(List<int> jobIds)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Profile.Token}");
                client.DefaultRequestHeaders.Add("User-Agent", "YourAppName"); // Обязательно юзер-агент
                client.DefaultRequestHeaders.Host = "api.hh.ru"; // Или нужный хост, если требуется

                foreach (var jobId in jobIds)
                {
                    try
                    {
                        var response = await client.GetAsync($"https://api.hh.ru/vacancies/{jobId}");
                        response.EnsureSuccessStatusCode();

                        var json = await response.Content.ReadAsStringAsync();

                        // Десериализуем в промежуточный класс
                        var vacancyRaw = JsonConvert.DeserializeObject<VacancyRaw>(json);

                        

                        // Преобразуем в наш объект Vacancy
                        var vacancy = new VacancyRaw
                        {
                            Id = vacancyRaw.Id,
                            Name = vacancyRaw.Name,
                            Description = CleanHtml(vacancyRaw.Description),
                            BrandedDescription = CleanHtml(vacancyRaw.BrandedDescription),
                            KeySkills = vacancyRaw.KeySkills
                        };

                        // Добавляем в CSV файл
                        ManagerCsv.WriteReport(vacancy);


                        // Генерируем рандом
                        Random random = new Random();
                        int delay = random.Next(1000, 3001);
                        await Task.Delay(delay);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при загрузке вакансии {jobId}: {ex.Message}");
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
