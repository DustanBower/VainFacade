using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Haste
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class HastePowerStatusEffect : StatusEffect
    {
        [Obfuscation(Exclude = true)]
        public Card Source
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public TurnTaker Hero
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public int Num
        {
            get;
            private set;
        }

        public override bool GeneratesTriggers => false;

        public HastePowerStatusEffect(Card source, TurnTaker hero, int num)
        {
            Source = source;
            Hero = hero;
            Num = num;
        }

        public override string ToString()
        {
            return $"When a non-hero card enters play, {Hero.Name} may remove {Num} tokens from their speed pool to play a card.";
        }
    }
}
