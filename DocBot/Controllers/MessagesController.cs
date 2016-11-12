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
                var message = await argument;
                var optionsList = new List<string>();
                string query = string.Empty;
                if (message.Text == "hi")
                {
                    await context.PostAsync($"How you doinn...");
                    context.Wait(MessageReceivedAsync);
                }
                else if (count == 1)
                {
                    ExtractLuis replyFromLuis = new ExtractLuis();
                    //Luis details
                    LuisModelAttribute docBot = new LuisModelAttribute("0ada6925-a4b5-4eae-91bb-0157a7f6efdf", "8e313738104945008db930cb54f355a7");
                    LuisService docService = new LuisService(docBot);
                    LuisResult LuisResponse = await docService.QueryAsync(message.Text);
                    query = replyFromLuis.getDisease(LuisResponse, message.Text);
                    RootObject[] t = await Search.GetSearchQuery(query);
                    optionsList.Clear();
                    foreach(RootObject x in t)
                    {
                        optionsList.Add(x.label.ToString());
                    }
                    PromptOptions<string> options = new PromptOptions<string>($"What kind of {query} ?", options: optionsList);
                    PromptDialog.Choice<string>(context, AfterResetAsync, options);
                    count++;
                }
                else if (message.Text == "reset")
                {
                    count = 1;
                }
                else
                {
                    await context.PostAsync($"{this.count++}: You said {message.Text}");
                    context.Wait(MessageReceivedAsync);
                }
            }
            public async Task AfterResetAsync(IDialogContext context, IAwaitable<string> argument)
            {
                var confirm = await argument;
                
                context.Wait(MessageReceivedAsync);
            }
        }


    }
}