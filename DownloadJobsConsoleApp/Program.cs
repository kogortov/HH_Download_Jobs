using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadJobsConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // ASCII-арт
            string asciiArt = @"

                                                                                       ▄▄                          ▄▄                            ▄▄               
▀████▀  ▀████▀▀████▀  ▀████▀▀       ▀███▀▀▀██▄                                       ▀███                        ▀███            ▀████▀         ▄██               
  ██      ██    ██      ██            ██    ▀██▄                                       ██                          ██              ██            ██               
  ██      ██    ██      ██            ██     ▀██ ▄██▀██▄▀██▀    ▄█    ▀██▀████████▄    ██   ▄██▀██▄ ▄█▀██▄    ▄█▀▀███              ██   ▄██▀██▄  ██▄████▄  ▄██▀███
  ██████████    ██████████            ██      ████▀   ▀██ ██   ▄███   ▄█   ██    ██    ██  ██▀   ▀███   ██  ▄██    ██              ██  ██▀   ▀██ ██    ▀██ ██   ▀▀
  ██      ██    ██      ██            ██     ▄████     ██  ██ ▄█  ██ ▄█    ██    ██    ██  ██     ██▄█████  ███    ██              ██  ██     ██ ██     ██ ▀█████▄
  ██      ██    ██      ██            ██    ▄██▀██▄   ▄██   ███    ███     ██    ██    ██  ██▄   ▄███   ██  ▀██    ██         ███  ██  ██▄   ▄██ ██▄   ▄██ █▄   ██
▄████▄  ▄████▄▄████▄  ▄████▄▄       ▄████████▀   ▀█████▀     █      █    ▄████  ████▄▄████▄ ▀█████▀▀████▀██▄ ▀████▀███▄        █████    ▀█████▀  █▀█████▀  ██████▀
                                                                                                                                                                  
                                                                                                                                                                  
                                                                                                                                                                                                                        
        ";

            // Название компании и автор    
            string author = "Kogortov";
            string releaseDate = "24.05.2025";
            string gitHub = "https://github.com/kogortov";

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(asciiArt);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Автор: { author}");
            Console.WriteLine($"Дата релиза: {releaseDate}");
            Console.WriteLine($"{gitHub}\n");
            Console.ResetColor();

            await Profile.CreateProfile();

            List<int> jobIds = await RequestSearch.GetVacancyIdsAsync();


            await RequestSearch.LoadVacanciesAsync(jobIds);

            Console.WriteLine($"\nГотово! CSV файл лежит в пути: C:\\...\\Documents\\HH Download Jobs ");

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();

        }
    }
}
