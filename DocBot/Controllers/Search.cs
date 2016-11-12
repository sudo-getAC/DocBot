using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace DocBot.Controllers
{
    public class RootObject
    {
        public string id { get; set; }
        public string label { get; set; }
    }

    public class Search
    {
        public static async Task<RootObject[]> GetSearchQuery(string phrase)
        {
            RootObject[] result = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.infermedica.com/v2/search?");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("app_id", "52447695");
            client.DefaultRequestHeaders.Add("app_key", "fb166ebd9340942d20c121e2cf4eeb56");


            var path = "https://api.infermedica.com/v2/search?phrase=" + Uri.EscapeDataString(phrase);
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                 result = await response.Content.ReadAsAsync<RootObject[]>();
            }

            
            return result;

        }
    }
}