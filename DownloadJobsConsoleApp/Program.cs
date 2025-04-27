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
  
   _____      _             __      __                              
  / ____|    | |            \ \    / /                              
 | |  __  ___| |_ ___ _ __   \ \  / /_ _  ___ __ _ _ __   ___ _   _ 
 | | |_ |/ _ \ __/ _ \ '__|   \ \/ / _` |/ __/ _` | '_ \ / __| | | |
 | |__| |  __/ ||  __/ |       \  / (_| | (_| (_| | | | | (__| |_| |
  \_____|\___|\__\___|_|        \/ \__,_|\___\__,_|_| |_|\___|\__, |
                                                               __/ |
                                                              |___/ 

                                                                     
        ";

            // Название компании и автор
            string author = "Автор: Kogortov";
            string gitHub = "https://github.com/kogortov";
            Console.WriteLine(asciiArt);
            Console.WriteLine(author);
            Console.WriteLine($"{gitHub}\n");

            await Profile.CreateProfile();

            List<int> jobIds = await RequestSearch.GetVacancyIdsAsync();


            await RequestSearch.LoadVacanciesAsync(jobIds);

        }
    }
}
