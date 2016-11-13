using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBot.Controllers
{
    public class ExtractLuis
    {
        public string getDisease(LuisResult LuisResponse,string normalResponse)
        {
            if (LuisResponse.Intents[0].Intent == "haveDisease")
                return LuisResponse.Entities[0].Entity;
            return null;
        }
    }
}