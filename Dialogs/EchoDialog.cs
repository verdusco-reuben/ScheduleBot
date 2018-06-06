using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;
using System.Web;
using Microsoft.Bot.Connector;


namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [LuisModel("7d8ea658-f01a-49f2-a239-2d7ef805dde9", "1cf447f840ee414e87c7b93bb6d5cc63", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class EchoDialog : LuisDialog<object>
    {
        // Store notes in a dictionary that uses the title as a key
        private readonly Dictionary<string, Note> noteByTitle = new Dictionary<string, Note>();

        // Default note title
        public const string DefaultNoteTitle = "default";
        // Name of note title entity
        public const string Entity_Note_Title = "Note.Title";
        
        // Provide a welcome and ask for a name
        private bool userWelcomed;
        
        //
        private string user_name;
        
        // Search for a single note 
        public bool FindOneNote(LuisResult result, out Note note)
        {
            note = null;

            string titleToFind;

            EntityRecommendation title;
            if (result.TryFindEntity(Entity_Note_Title, out title))
            {
                titleToFind = title.Entity;
            }
            else
            {
                titleToFind = DefaultNoteTitle;
            }

            return this.noteByTitle.TryGetValue(titleToFind, out note); // TryGetValue returns false if no match is found.
        }

        /// <summary>
        /// This method overload takes a string and finds the note with that title.
        /// </summary>
        /// <param name="noteTitle">A string containing the title of the note to search for.</param>
        /// <param name="note">This parameter returns any note that is found in the list of notes that has a matching title.</param>
        /// <returns>true if a note was found, otherwise false</returns>
        public bool FindOneNote(string noteTitle, out Note note)
        {
            bool foundNote = this.noteByTitle.TryGetValue(noteTitle, out note); // TryGetValue returns false if no match is found.
            return foundNote;
        }


        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            if(userWelcomed != true)
            {
                await context.PostAsync(message);
                user_name = MessageReceived
                context.Wait(MessageReceived);
            }
            else
            {
                string message = $"Hello"+user_name+"! I'm ScheduleBot. I can understand requests to create, read, and delete events for you! \n\n Detected intent: " + string.Join(", ", result.Intents.Select(i => i.Intent));
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }
        
        
        
        /// <summary>
        /// Handle the Note.Delete intent. If a title isn't detected in the LUIS result, prompt the user for a title.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        /// <returns></returns>
        
        [LuisIntent("Note.Delete")]
        public async Task DeleteNote(IDialogContext context, LuisResult result)
        {
            Note note;
            if (FindOneNote(result, out note))
            {
                this.noteByTitle.Remove(note.Title);
                await context.PostAsync($"Note {note.Title} deleted");
            }
            else
            {                             
                // Prompt the user for a note title
                PromptDialog.Text(context, After_DeleteTitlePrompt, "What is the title of the note you want to delete?");                         
            }

        }

        private async Task After_DeleteTitlePrompt(IDialogContext context, IAwaitable<string> result)
        {
            Note note;
            string titleToDelete = await result;
            bool foundNote = this.noteByTitle.TryGetValue(titleToDelete, out note);

            if (foundNote)
            {
                this.noteByTitle.Remove(note.Title);
                await context.PostAsync($"Note {note.Title} deleted");
            }
            else
            {
                await context.PostAsync($"Did not find note named {titleToDelete}.");
            }

            context.Wait(MessageReceived);
        }

        /// <summary>
        /// Handles the Note.ReadAloud intent by displaying a note or notes. 
        /// If a title of an existing note is found in the LuisResult, that note is displayed. 
        /// If no title is detected in the LuisResult, all of the notes are displayed.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">LUIS result.</param>
        /// <returns></returns>
        [LuisIntent("Note.ReadAloud")]
        public async Task FindNote(IDialogContext context, LuisResult result)
        {
            Note note;
            if (FindOneNote(result, out note))
            {
                await context.PostAsync($"**{note.Title}**: {note.Text}.");
            }
            else
            {
                // Print out all the notes if no specific note name was detected
                string NoteList = "Here's the list of all notes: \n\n";
                foreach (KeyValuePair<string, Note> entry in noteByTitle)
                {
                    Note noteInList = entry.Value;
                    NoteList += $"**{noteInList.Title}**: {noteInList.Text}.\n\n";
                }
                await context.PostAsync(NoteList);
            }

            context.Wait(MessageReceived);
        }

        private Note noteToCreate;
        private string currentTitle;

        /// <summary>
        /// Handles the Note.Create intent. Prompts the user for the note title if the title isn't detected in the LuisResult.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">LUIS result.</param>
        /// <returns></returns>
        [LuisIntent("Note.Create")]
        public Task CreateNote(IDialogContext context, LuisResult result)
        {
            EntityRecommendation title;
            if (!result.TryFindEntity(Entity_Note_Title, out title))
            {
                // Prompt the user for a note title
                PromptDialog.Text(context, After_TitlePrompt, "What is the title of the note you want to create?");
            }
            else
            {
                var note = new Note() { Title = title.Entity };
                noteToCreate = this.noteByTitle[note.Title] = note;

                // Prompt the user for what they want to say in the note           
                PromptDialog.Text(context, After_TextPrompt, "What do you want to say in your note?");
            }

            return Task.CompletedTask;
        }

        private async Task After_TitlePrompt(IDialogContext context, IAwaitable<string> result)
        {
            EntityRecommendation title;
            // Set the title (used for creation, deletion, and reading)
            currentTitle = await result;
            if (currentTitle != null)
            {
                title = new EntityRecommendation(type: Entity_Note_Title) { Entity = currentTitle };
            }
            else
            {
                // Use the default note title
                title = new EntityRecommendation(type: Entity_Note_Title) { Entity = DefaultNoteTitle };
            }

            // Create a new note object 
            var note = new Note() { Title = title.Entity };
            // Add the new note to the list of notes and also save it in order to add text to it later
            noteToCreate = this.noteByTitle[note.Title] = note;

            // Prompt the user for what they want to say in the note           
            PromptDialog.Text(context, After_TextPrompt, "What do you want to say in your note?");

        }

        private async Task After_TextPrompt(IDialogContext context, IAwaitable<string> result)
        {
            // Set the text of the note
            noteToCreate.Text = await result;
            
            await context.PostAsync($"Created note **{this.noteToCreate.Title}** that says \"{this.noteToCreate.Text}\".");
            
            context.Wait(MessageReceived);
        }


        public EchoDialog() { }

        public EchoDialog(ILuisService service): base(service) { }

        [Serializable]
        public sealed class Note : IEquatable<Note>
        {

            public string Title { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return $"[{this.Title} : {this.Text}]";
            }

            public bool Equals(Note other)
            {
                return other != null
                    && this.Text == other.Text
                    && this.Title == other.Title;
            }

            public override bool Equals(object other)
            {
                return Equals(other as Note);
            }

            public override int GetHashCode()
            {
                return this.Title.GetHashCode();
            }
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    //state stuff
    
    //  [Serializable]
    //  public class StateDialog : IDialog<object>
    //  {
    //      private const string HelpMessage = "\n * If you want to know which city I'm using for my searches type 'current city'. \n * Want to change the current city? Type 'change city to cityName'. \n * Want to change it just for your searches? Type 'change my city to cityName'";
    //      private bool userWelcomed;

    //      public async Task StartAsync(IDialogContext context)
    //      {
    //          string defaultCity;
            
    //          if (!context.ConversationData.TryGetValue(ContextConstants.CityKey, out defaultCity))
    //          {
    //              defaultCity = "Seattle";
    //              context.ConversationData.SetValue(ContextConstants.CityKey, defaultCity);
    //          }

    //          await context.PostAsync($"Welcome to the Search City bot. I'm currently configured to search for things in {defaultCity}");
    //          context.Wait(this.MessageReceivedAsync);
    //      }

    //      public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
    //      {
    //          var message = await result;

    //          string userName;

    //          if (!context.UserData.TryGetValue(ContextConstants.UserNameKey, out userName))
    //          {
    //              PromptDialog.Text(context, this.ResumeAfterPrompt, "Before get started, please tell me your name?");
    //              return;
    //          }

    //          if (!this.userWelcomed)
    //          {
    //              this.userWelcomed = true;
    //              await context.PostAsync($"Welcome back {userName}! Remember the rules: {HelpMessage}");

    //              context.Wait(this.MessageReceivedAsync);
    //              return;
    //          }

    //          if (message.Text.Equals("current city", StringComparison.InvariantCultureIgnoreCase))
    //          {
    //              string userCity;

    //              var city = context.ConversationData.GetValue<string>(ContextConstants.CityKey);

    //              if (context.PrivateConversationData.TryGetValue(ContextConstants.CityKey, out userCity))
    //              {
    //                  await context.PostAsync($"{userName}, you have overridden the city. Your searches are for things in  {userCity}. The default conversation city is {city}.");
    //              }
    //              else
    //              {
    //                  await context.PostAsync($"Hey {userName}, I'm currently configured to search for things in {city}.");
    //              }
    //          } 
    //          else if (message.Text.StartsWith("change city to", StringComparison.InvariantCultureIgnoreCase))
    //          {
    //              var newCity = message.Text.Substring("change city to".Length).Trim();
    //              context.ConversationData.SetValue(ContextConstants.CityKey, newCity);

    //              await context.PostAsync($"All set {userName}. From now on, all my searches will be for things in {newCity}.");
    //          }
    //          else if (message.Text.StartsWith("change my city to", StringComparison.InvariantCultureIgnoreCase))
    //          {
    //              var newCity = message.Text.Substring("change my city to".Length).Trim();
    //              context.PrivateConversationData.SetValue(ContextConstants.CityKey, newCity);

    //              await context.PostAsync($"All set {userName}. I have overridden the city to {newCity} just for you.");
    //          } 
    //          else
    //          {
    //              string city;

    //              if (!context.PrivateConversationData.TryGetValue(ContextConstants.CityKey, out city))
    //              {
    //                  city = context.ConversationData.GetValue<string>(ContextConstants.CityKey);
    //              }

    //              await context.PostAsync($"{userName}, wait a few seconds. Searching for '{message.Text}' in '{city}'...");
    //              await context.PostAsync($"https://www.bing.com/search?q={HttpUtility.UrlEncode(message.Text)}+in+{HttpUtility.UrlEncode(city)}");
    //          }

    //          context.Wait(this.MessageReceivedAsync);
    //      }

    //      private async Task ResumeAfterPrompt(IDialogContext context, IAwaitable<string> result)
    //      {
    //          try
    //          {
    //              var userName = await result;
    //              this.userWelcomed = true;

    //              await context.PostAsync($"Welcome {userName}! {HelpMessage}");

    //              context.UserData.SetValue(ContextConstants.UserNameKey, userName);
    //          }
    //          catch (TooManyAttemptsException)
    //          {
    //          }

    //          context.Wait(this.MessageReceivedAsync);
    //      }
    //  }
    //  public class ContextConstants
    //  {
    //      public const string UserNameKey = "UserName";

    //      public const string CityKey = "City";
    //  }

}