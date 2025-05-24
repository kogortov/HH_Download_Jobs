using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DownloadJobsConsoleApp
{
    public static class ManagerCsv
    {
        static string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HH Download Jobs");


        // Генерация имени файла с текущей датой и временем (с миллисекундами)
        static string fileName = $"Vacancy_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.csv";
        static string filePath = Path.Combine(directoryPath, fileName);

        // Метод для записи одиночной вакансии в CSV
        public static void WriteReport(VacancyRaw vacancy)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Проверяем, существует ли файл. Если нет, создаем его с заголовками
            bool fileExists = File.Exists(filePath);

            using (var writer = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8)) // append: true для добавления данных в файл
            {
                // Если файл не существует, записываем заголовки
                if (!fileExists)
                {
                    writer.WriteLine("Id;Название;Описание;Доп. описание;Ключевые навыки");
                }

                // Обработка значений с возможными разделителями
                int id = vacancy.Id;
                string name = ProtectCsvValue(vacancy.Name);
                string description = ProtectCsvValue(vacancy.Description);
                string brandedDescription = ProtectCsvValue(vacancy.BrandedDescription);
                string keySkills = string.Join(", ", vacancy.KeySkills.Select(skill => ProtectCsvValue(skill.Name)));

                // Запись строки в файл
                writer.WriteLine($"{vacancy.Id};{name};{description};{brandedDescription};{keySkills}");
            }

            Console.WriteLine($"Вакансия добавлена в CSV файл: {filePath}");
        }

        // Метод для обработки значений с разделителями
        private static string ProtectCsvValue(string value)
        {
            // Заменяем несколько пробелов подряд на один
            value = Regex.Replace(value, @"\s+", " ").Trim();

            // Проверяем на наличие специальных символов, включая точку с запятой
            if (value.Contains(",") || value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
            {
                // Экранируем двойные кавычки и оборачиваем в кавычки
                value = "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}

