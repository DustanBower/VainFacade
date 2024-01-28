using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Glyph
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class EmbroideredCloakStatusEffect : StatusEffect
    {
        [Obfuscation(Exclude = true)]
        public Card Source
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public TurnTaker Owner
        {
            get;
            private set;
        }

        public override bool GeneratesTriggers => false;

        public EmbroideredCloakStatusEffect(Card source, TurnTaker owner)
        {
            Source = source;
            Owner = owner;
        }

        public override string ToString()
        {
            return $"Cards in {Owner.Name}'s play area can only be destroyed by {Owner.Name}.";
        }
    }
}
