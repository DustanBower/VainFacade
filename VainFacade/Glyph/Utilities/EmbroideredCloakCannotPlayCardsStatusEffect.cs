using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Glyph
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class EmbroideredCloakCannotPlayCardsStatusEffect : CannotPlayCardsStatusEffect
    {
        [Obfuscation(Exclude = true)]
        public Card Source
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public TurnTaker User
        {
            get;
            private set;
        }

        public override bool GeneratesTriggers => false;

        public EmbroideredCloakCannotPlayCardsStatusEffect(Card source, TurnTaker user)
        {
            Source = source;
            User = user;
        }

        public override string ToString()
        {
            return $"Cards from decks other than {User.Name}'s cannot be played.";
        }
    }
}
