using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Glyph
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class ReturnToSilenceStatusEffect : StatusEffect
    {
        [Obfuscation(Exclude = true)]
        public Card Source
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public Location PlayArea
        {
            get;
            private set;
        }

        public override bool GeneratesTriggers => false;

        public ReturnToSilenceStatusEffect(Card source, Location playArea)
        {
            Source = source;
            PlayArea = playArea;
        }

        public override string ToString()
        {
            return $"Non-character, non-target cards in {PlayArea.GetFriendlyName()} gain the ongoing keyword.";
        }
    }
}
