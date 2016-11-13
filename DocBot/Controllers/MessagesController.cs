using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using DocBot.Controllers;
using System.Diagnostics;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Threading;
using RestSharp;

namespace DocBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;


                /*
                // return our reply to the user
                Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");

                if (activity.Text == "hi")
                {
                    reply = activity.CreateReply($"Hi. How may I Help you");
                }
                else
                {

                    ExtractLuis replyFromLuis = new ExtractLuis();
                    //Luis details
                    LuisModelAttribute docBot = new LuisModelAttribute("0ada6925-a4b5-4eae-91bb-0157a7f6efdf", "8e313738104945008db930cb54f355a7");
                    LuisService docService = new LuisService(docBot);
                    LuisResult LuisResponse = await docService.QueryAsync(activity.Text);
                    Debug.WriteLine(LuisResponse.Intents[0].Intent);
                    string query = replyFromLuis.getDisease(LuisResponse,activity.Text);
                    RootObject[] t = await Search.GetSearchQuery(query);
                    
                }
                */
                await Conversation.SendAsync(activity, () => new EchoDialog());
                //await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        [Serializable]
        public class EchoDialog : IDialog<object>
        {
            protected int count = 1;
            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceivedAsync);
            }
            public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
            {
                //Luis details
                LuisModelAttribute docBot = new LuisModelAttribute("0ada6925-a4b5-4eae-91bb-0157a7f6efdf", "8e313738104945008db930cb54f355a7");
                LuisService docService = new LuisService(docBot);
                RootObject[] optionID=null;

                var message = await argument;
                var optionsList = new List<string>();
                string query = string.Empty;
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;
                LuisResult LuisResponse = await docService.QueryAsync(message.Text,token);
                

                if (LuisResponse.Intents[0].Intent=="hiHello")
                {
                    await context.PostAsync($"Hi. How are you today");
                    context.Wait(MessageReceivedAsync);
                }
                else if (LuisResponse.Intents[0].Intent=="haveDisease")
                {
                    ExtractLuis replyFromLuis = new ExtractLuis();
                    
                    query = replyFromLuis.getDisease(LuisResponse, message.Text);
                    optionID = await Search.GetSearchQuery(query);
                    
                    optionsList.Clear();
                    foreach(RootObject x in optionID)
                    {
                        optionsList.Add(x.label.ToString());
                    }
                    PromptOptions<string> options = new PromptOptions<string>($"What kind of {query} ?", options: optionsList);
                    PromptDialog.Choice<string>(context, AfterResetAsync, options);
                    count++;
                }
                else if (LuisResponse.Intents[0].Intent == "dontKnow")
                {

                }
                else if (message.Text == "reset")
                {
                    count = 1;
                }
                else
                {
                    optionsList.Clear();

                    context.Wait(MessageReceivedAsync);
                }
            }
            public async Task AfterResetAsync(IDialogContext context, IAwaitable<string> argument)
            {
                var confirm = await argument;
                var optionsList = new List<Choice>();
                var desc = new List<string>();
                RootObject[] optionID = await Search.GetSearchQuery(confirm);
                IRestResponse<RootObjectD> newOption = await Diagnosis.PostDiagnosisQuery(optionID[0].id.ToString(),"present");
                //await context.PostAsync($" {newOption.Data.question.text}");

                foreach(Item i in newOption.Data.question.items)
                {
                    string itemID = i.id;
                    foreach(Choice c in i.choices)
                    {
                        optionsList.Add(c);
                        desc.Add(c.label);
                    }
                }
                PromptOptions<Choice> options = new PromptOptions<Choice>($"{newOption.Data.question.text} ?", options: optionsList, descriptions: desc);
                PromptDialog.Choice<Choice>(context, AfterResetQuestions, options);
                
                //context.Wait(MessageReceivedAsync);
            }
            public async Task AfterResetQuestions(IDialogContext context, IAwaitable<Choice> argument)
            {
                var confirm = await argument;

                //string[] id=confirm.Split('-');
                var test = confirm.id;
                var optionsList = new List<string>();
                //IRestResponse<RootObjectD> newOption = await Diagnosis.PostDiagnosisQuery(id[1],(id[0]=="Yes")?"present":"absent");
                //await context.PostAsync($" {newOption.Data.question.text}");
                //foreach (Item i in newOption.Data.question.items)
                //{
                //    string itemID = i.id;
                //    foreach (Choice c in i.choices)
                //    {
                //        optionsList.Add(c.label + "-" + itemID);
                //    }
                //}
                //if (newOption.Data.conditions[0].probability > 0.7)
                //{
                //    await context.PostAsync($"You have {newOption.Data.conditions[0].name}");
                //    context.Wait(MessageReceivedAsync);
                //}
                //else {
                //    PromptOptions<string> options = new PromptOptions<string>($"{newOption.Data.question.text} ?", options: optionsList);
                //    PromptDialog.Choice<string>(context, AfterResetQuestions, options);
                //}
                ////context.Wait(MessageReceivedAsync);
            }
        }


    }
}