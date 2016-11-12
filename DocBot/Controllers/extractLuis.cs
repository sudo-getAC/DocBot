using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBot.Controllers
{
    public class ExtractLuis
    {
        public string getDisease(LuisResult LuisResponse)
        {
            if (LuisResponse.Intents[0].Intent == "haveDisease" && LuisResponse.Intents[0].Intent!="None")
                return LuisResponse.Entities[0].Entity;
            return null;
        }
    }
}