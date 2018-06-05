using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if(message.Text == "hi" || message.Text == "hello")
            {
                await context.PostAsync("Hello from ScheduleBot! Did you want to add something new to your schedule?");
            }
            else if(message.Text.Contains("yes")|| message.Text.Contains("yeah") )
            {
                await context.PostAsync("Please type a date in MM/DD/YY format");
                context.Wait(MessageReceivedAsync);
            }
            else if(message.Text == "Check my schedule")
            {
                //  if(mySchedule.appointments.count == 0){
                    await context.PostAsync("Your schedule is empty");
                //  }
                //  else
                //  {
                    //  await context.PostAsync(mySchedule.appointments);
                //  }
                context.Wait(MessageReceivedAsync);
            }
            
            else if(message.Text.Contains("add"))
            {
                await context.PostAsync("Please type a date in MM/DD/YY format");
                context.Wait(MessageReceivedAsync);
            }
            else if (message.Text.Contains("tomorrow") || message.Text.Contains("Tomorrow"))
            {
                 PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Great! 06/05/18 it is.",
                    promptStyle: PromptStyle.Auto);
                    
                await context.PostAsync("Great. 06/05/18 it is.");
            }
            else if(message.Text.Contains("ya"))
            {
                await context.PostAsync("Please add description below");
                context.Wait(MessageReceivedAsync);
            }
            else if(message.Text.Contains("description") || message.Text.Contains("DESCRIPTION"))
            {
                await context.PostAsync("Description? Sure! Type one now.");
            }
            else if(message.Text.Contains("thank"))
            {
                await context.PostAsync("No problem! Have a nice day :-)");
            }
            else
            {
                await context.PostAsync("Great! 06/05/18 : Going fishing");
                context.Wait(MessageReceivedAsync);
            }

        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                await context.PostAsync("Sounds good, we will leave your appt at 06/05/18 with a status 'Pending'.");
            }
            else
            {
                await context.PostAsync("Please type a date in MM/DD/YY format");
            }
            //  mySchedule.Add("06/05/17 : Pending");
            context.Wait(MessageReceivedAsync);
        }

    }
    
    
    //  public class mySchedule{
    //      public List<string> appointments = new List<string>();
    
    //  }
    

    //RandomFact
    public class RandomFactDialog
	{
		 public static readonly IDialog<string> Dialog = Chain
            
            .PostToChain()
            .Select(m => m.Text)
            .Switch
            (
                Chain.Case
                (
                    new Regex("^tell me a fact"),
                    (context, text) =>

                Chain.Return("Grabbing a fact...")
                .PostToUser()
                .ContinueWith<string, string>(async (ctx, res) =>
                {
                    var response = await res;

                    var fact = await GetRandomFactAsync();

                    return Chain.Return("**FACT:** *" + fact + "*");

                })

                ),
                Chain.Default<string, IDialog<string>>(

                    (context, text) =>

                        Chain.Return("Say 'tell me a fact'")
                )
            )
            .Unwrap().PostToUser();
            
        public async static Task<string> GetRandomFactAsync()
        {
    
            return "this is a random fact";
        }
	}
   
   
   //  //FormFlow
    //  public enum AppOptions
    //  {
    //      None,
    //      [Terms(new string[] { "crap apps", "crappy" })]
    //      LotsOfCrummyApps,
    //      ReallyTrendyApps,
    //      FreeGames,
    //      ProfessionalApps,
    //      BankingApps
    //  };

    //  public enum DeviceType { Phone, Tablet, Desktop };
    //  public enum IntellgenceLevel { Low, Average, High };
    //  public enum AgeRange
    //  {
    //      [Describe("I'm just a teenager")]
    //      Teenager,

    //      [Describe("I'm young and trendy")]
    //      Millenial,

    //      [Describe("I've lived life and know things")]
    //      SeasonedAdult
    //  };

    //  [Serializable]
    //  public class DeviceOrder
    //  {
    //      [Prompt("What kind of device are you looking for? {||}")]
    //      public DeviceType? DeviceType;
        
    //      [Optional]
    //      [Prompt("What's your IQ?? {||}")]
    //      public IntellgenceLevel? IntellgenceLevel;

    //      [Prompt("How old are you? {||}")]
    //      public AgeRange? Age;

    //      [Prompt("What kind of apps do you want support for? {||}")]
    //      [Template(TemplateUsage.NotUnderstood, "What does \"{0}\" mean???", ChoiceStyle = ChoiceStyleOptions.Auto)]
    //      [Describe("Types of apps")]
    //      public List<AppOptions> AppOptions;
        
    //      public static IForm<DeviceOrder> BuildForm()
    //      {
    //          return new FormBuilder<DeviceOrder>()
    //                  .Message("Welcome to the personality-based device recommendation bot!")
    //                  .Confirm("Do you really want to buy this tablet?")                     
    //                  .OnCompletion(async (context, state) =>
    //                  {
                        
    //                      var reply = MessagesController.GetActivityName().CreateReply(GetDeviceRecommendationMessage(context, state));

    //                      await context.PostAsync(reply);

    //                  })
    //                  .Build();
    //      }
        
        //  private static string GetDeviceRecommendationMessage(IDialogContext context, DeviceOrder state)
        //  {
        //      string recommendationLabel = "For your personality and app preferences, we recommend a";

        //      if (state.Age < AgeRange.SeasonedAdult)
        //      {
        //          if (state.IntellgenceLevel == Forms.IntellgenceLevel.Low)
        //          {
        //              recommendationLabel += "n iOS";
        //          }
        //          else if (state.IntellgenceLevel == Forms.IntellgenceLevel.High)
        //          {
        //              recommendationLabel += " Windows";
        //          }
        //          else
        //          {
        //              recommendationLabel += "n Android";
        //          }
        //      }
        //      else
        //      {
        //          if (state.IntellgenceLevel == Forms.IntellgenceLevel.Low)
        //          {
        //              recommendationLabel += " Windows";
        //          }
        //          else if (state.IntellgenceLevel == Forms.IntellgenceLevel.High)
        //          {
        //              recommendationLabel += " Windows";
        //          }
        //          else
        //          {
        //              recommendationLabel += "n Android";
        //          }
        //      }

        //      recommendationLabel += " " + state.DeviceType + ".";

        //      return recommendationLabel;
        //  }


    //  }
    
}