using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace DownloadJobsConsoleApp
{
    public static class Profile
    {
        public static readonly string ClientId = "I7E5200QERLFMTLGRCQEA7EF7L5KM78FQ37MHOE7AU474MJ4QNEU3E8NLCTLDG6I";
        public static readonly string ClientSecret = "UEBCQH40P9TQ277NKULBKT7V7PJKLITVLSO34NIRH0A18HD8IB6EDHKRMADAKKMF";
        public static string AuthorizationCode {  get; set; }
        public static string Token { get; set; }

        // Создать профиль
        public static async Task CreateProfile()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // Получаем код
            Console.WriteLine("Алгоритм действий:");
            Console.WriteLine("1.   Скопируйте ссылку ниже и вставьте её в браузер↓");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"https://hh.ru/oauth/authorize?response_type=code&client_id={ClientId}");
            Console.ResetColor();

            Console.WriteLine("2.1. Сделайте авторизацию в HH по данной ссылке. После авторизации Вас переадресует на страницу «Не удается получить доступ к сайту», не пугайтесь!");
            Console.WriteLine("2.2. Если Вы ранее уже были авторизованы на портале HH в браузере, то Вам необходимо нажать на кнопку «Продолжить». Далее Вас переадресует на страницу «Не удается получить доступ к сайту», не пугайтесь!");
            
            Console.WriteLine("3.   Скопируете из адресной строки браузера полученною ссылку после авторизации и вставьте ниже ↓\n");

            string authorizedLink = Console.ReadLine();
            AuthorizationCode = ExtractCodeFromUrl(authorizedLink);

            Console.WriteLine("\nКод авторизации извлечён!");

            // Получаем Токен
            Token = await ExchangeCodeForTokenAsync();
            Console.WriteLine("Токен успешно получен!\n");


        }

        // Получаем код из ссылки
        private static string ExtractCodeFromUrl(string url)
        {

            // Проверка на пустую строку
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL не может быть пустым или содержать только пробелы.");
            }

            // Регулярное выражение для извлечения параметра 'code'
            var regex = new Regex(@"[?&]code=([^&]+)");
            var match = regex.Match(url);

            // Если код не найден, выбрасываем исключение
            if (!match.Success)
            {
                throw new ArgumentException("Параметр 'code' не найден в URL.");
            }

            // Возвращаем значение после 'code='
            return match.Groups[1].Value;

        }

        // Получаем токе
        public static async Task<string> ExchangeCodeForTokenAsync()
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", AuthorizationCode },
            { "client_id", ClientId },
            { "client_secret", ClientSecret }
        };

                var content = new FormUrlEncodedContent(values);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.hh.ru/token")
                {
                    Content = content
                };
                request.Headers.Add("Accept", "application/json"); // Опционально

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка при обмене кода на токен: {response.StatusCode}\n{errorBody}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);

                // Например, можно сразу вернуть access_token
                return json["access_token"].ToString();
            }
        }
    }

}

