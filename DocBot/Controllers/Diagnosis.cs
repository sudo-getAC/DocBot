using DocBot.Controllers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace DocBot.Controllers
{
    [Serializable]
    public class Choice
    {
        public string id { get; set; }
        public string label { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<Choice> choices { get; set; }
    }

    public class Extras
    {
    }

    public class Question
    {
        public string type { get; set; }
        public string text { get; set; }
        public List<Item> items { get; set; }
        public Extras extras { get; set; }
    }

    public class Condition
    {
        public string id { get; set; }
        public string name { get; set; }
        public double probability { get; set; }
    }

    public class RootObjectD
    {
        public Question question { get; set; }
        public List<Condition> conditions { get; set; }
    }

    public class EvidenceObject
    {
        public string id { get; set; }
        public string choice_id { get; set; }
    }

    public class QueryObject
    {
        public string sex { get; set; }
        public int age { get; set; }
        public EvidenceObject[] evidence { get; set; }
    }



    public class Diagnosis
    {
        public static async Task<IRestResponse<RootObjectD>> PostDiagnosisQuery(string s_id,string choice)
        {
            var client = new RestClient("https://api.infermedica.com/v2/diagnosis");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d08af1e0-683e-be45-cc9e-b00fde2b8776");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("app_key", "fb166ebd9340942d20c121e2cf4eeb56");
            request.AddHeader("app_id", "52447695");
            EvidenceObject[] evi = { new EvidenceObject { id = s_id, choice_id = choice } };
            QueryObject content = new QueryObject { sex = "male", age = 20, evidence = evi };

            string data = JsonConvert.SerializeObject(content);
            request.AddParameter("application/json; charset=utf-8", data, ParameterType.RequestBody);
            IRestResponse<RootObjectD> response = client.Execute<RootObjectD>(request);
            return response;
        }
    }
}