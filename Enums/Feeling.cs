using Microsoft.Bot.Builder.FormFlow;

namespace MaratonaBotImageGetter.Enums
{
    public enum Feeling
    {
        None,
        [Terms("feliz", "felizes", "contente", "alegre", "empolgada", "animada")]
        [Describe("Happy")]
        Happiness,
        [Terms("triste", "chateado", "irritado", "emburrado", "carrancudo", "puto", "bravo")]
        [Describe("Sad")]
        Sadness
    }
}