using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DownloadJobsConsoleApp;

namespace DownloadJobsConsoleApp
{
    public static class VacancyStorage
    {
        public static List<VacancyRaw> Vacancies { get; set; } = new List<VacancyRaw>();
    }
}
