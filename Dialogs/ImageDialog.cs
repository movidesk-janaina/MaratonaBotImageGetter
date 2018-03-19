using MaratonaBotImageGetter.Controllers;
using MaratonaBotImageGetter.Enums;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MaratonaBotImageGetter.Dialogs
{
    [Serializable]
    [LuisModel("5b0b5b95-eeb4-4bea-9d82-d029521124ca", "35111886f0e74e74812a279140ee70e5")]
    public class ImageDialog : LuisDialog<object>
    {
        

        Face[] faces;                   // The list of detected faces.
        String[] faceDescriptions;      // The list of descriptions for the detected faces.
        double resizeFactor;

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Não entendi sua solicitação {result.Query}. Por gentileza tente outra abordagem...");
        }

        [LuisIntent("Sobre")]
        public async Task About(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Você está se referindo a minha roboticência");
            //await context.Activity.CreateReply("vdf");
        }

        [LuisIntent("Cumprimento")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Olá, me chamo Mika. Em que posso ajudar?");
        }

        [LuisIntent("Emoção")]
        public async Task Emocao(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Detectei uma solicitçaão de emoção");

            if (result.Entities.Any())
            {
                var quantityEntity = result.Entities.FirstOrDefault(a => a.Type == "builtin.datetimeV2.time");
                foreach (var item in result.Entities)
                {
                    Feeling feeling = Feeling.None;
                    switch (item.Type)
                    {
                        case "Emoção":
                            feeling = GetEmotion(item.Entity);
                            var quantity = quantityEntity != null ? GetQuantity(quantityEntity.Entity) : GetQuantity("");
                            var heroCard = await new ImageProcess().ProcessImages(context, feeling, quantity);

                            await CreateCarroussel(context, heroCard, item.Entity);
                            break;
                    }


                }
            }
            else
            {
                var heroCard = await new ImageProcess().ProcessImages(context, Feeling.None, 0);

                await CreateCarroussel(context, heroCard, "(ﾉ◕ヮ◕)ﾉ*:･ﾟ✧ A imagem solicitada não foi encontrada. Por favor realize uma nova abordagem ");
            }
        }

        private Feeling GetEmotion(string entity)
        {
            switch (entity.ToLower())
            {
                case "feliz": case "felizes": case "contente": case "alegre": case "empolgada": case "animada":
                    return Feeling.Happiness;
                case "triste":
                case "tristes":
                case "chateado": case "irritado": case "emburrado": case "carrancudo": case "puto": case "bravo":
                    return Feeling.Sadness;
            }

            return Feeling.None;
        }

        private int GetQuantity(string entity)
        {
            switch (entity.ToLower().Trim())
            {
                case "um":
                case "uma":
                    return 1;
                case "dois":
                case "duas":
                    return 2;
                case "três":
                    return 3;
                case "quatro":
                    return 4;
                case "cinco":
                    return 5;
            }
            return 1;
        }

        private async Task CreateCarroussel(IDialogContext context, List<HeroCard> heroCard, string title)
        {
            var msj = context.MakeMessage();
            msj.Text = title;
            msj.Attachments = heroCard.Select(a => a.ToAttachment()).ToList();
            msj.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            await context.PostAsync(msj);
        }

        public override Task StartAsync(IDialogContext context)
        {
            return base.StartAsync(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
             
            // calculate something for us to return   
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            var message = activity.CreateReply("oi");

            context.Wait(MessageReceivedAsync);
        }

    }
}