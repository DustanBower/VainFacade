using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Friday
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class AttuneStatusEffect : StatusEffect
    {
        [Obfuscation(Exclude = true)]
        public TurnTaker TurnTaker
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public Card Target
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public Card Source
        {
            get;
            private set;
        }

        [Obfuscation(Exclude = true)]
        public DamageType Type
        {
            get;
            set;
        }

        public override bool GeneratesTriggers => false;

        public AttuneStatusEffect(TurnTaker turnTaker, Card target, Card source, DamageType type)
        {
            TurnTaker = turnTaker;
            Target = target;
            Source = source;
            Type = type;
        }

        public override string ToString()
        {
            string text = $"{TurnTaker.Name} is attuned to {Type}.";
            if (this.Target.Identifier.Contains("Guise"))
            {
                text += " [i]Wait, I can do this now? Ahem... [b]I'll be back[/b].[/i]";
            }
            return text;
        }
    }
}
