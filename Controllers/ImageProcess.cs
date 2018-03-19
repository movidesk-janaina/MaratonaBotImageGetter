using MaratonaBotImageGetter.Enums;
using Microsoft.Bot.Builder.Dialogs;
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

namespace MaratonaBotImageGetter.Controllers
{
    public class ImageProcess
    {
        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("34ed42b6c56b4847a9aee5153c7d70ef", "https://brazilsouth.api.cognitive.microsoft.com/face/v1.0");

        Face[] faces;

        public async Task<List<HeroCard>> ProcessImages(IDialogContext context, Feeling feeling, int quantity )
        {
            //string filePath = "C:\\Users\\OEM\\Pictures\\Fotos\\1.jpg";
            var dir = "C:\\Users\\OEM\\Pictures\\Fotos\\";
            if (feeling == Feeling.None)
                return new List<HeroCard>() { CreateHeroCard("images.jpg", dir + "images.jpg") };

            string[] pdfFiles = Directory.GetFiles(dir, "*.jpg")
                                     .Select(Path.GetFileName)
                                     .ToArray();
            var heroCard = new List<HeroCard>();

            foreach (var file in pdfFiles)
            {
                string filePath = dir + file;
                faces = await UploadAndDetectFaces(filePath, context);

                switch (feeling)
                {
                    case Feeling.Happiness:

                        if (faces.Count(a => a.FaceAttributes.Smile > .5) >= quantity)
                        {
                            await Say(context, "está feliz");
                            heroCard.Add(CreateHeroCard("⊆（⌒◎⌒）⊇ " + file, filePath));
                        }
                        break;

                    case Feeling.Sadness:

                        if (faces.Count(a => a.FaceAttributes.Smile < .3) >= quantity)
                        {
                            await Say(context, "não está feliz");
                            heroCard.Add(CreateHeroCard("(ಥ﹏ಥ) " + file, filePath));
                        }
                        break;
                }
            }

            return heroCard;
        }

        private static HeroCard CreateHeroCard(string fileName, string filePath)
        {
            return (new HeroCard
            {
                Title = fileName,
                Images = new List<CardImage> { new CardImage(filePath, fileName) }
            });
        }

        private Task MessageReceived(IDialogContext context, IAwaitable<object> result)
        {
            throw new NotImplementedException();
        }

        private async Task CreateMessageawait(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var message = activity.CreateReply();
        }

        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath, IDialogContext context)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                await Say(context, f.ErrorMessage + f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                await Say(context, e.Message);
                return new Face[0];
            }
        }

        public static async Task Say(IDialogContext context, string phrase)
        {
            await context.PostAsync(phrase);
        }
    }
}