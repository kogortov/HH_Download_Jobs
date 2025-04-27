using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DownloadJobsConsoleApp
{

    public class VacancyRaw
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("branded_description")]
        public string BrandedDescription { get; set; }

        [JsonProperty("key_skills")]
        public List<Skill> KeySkills { get; set; }
    }

    public class Skill
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

}
