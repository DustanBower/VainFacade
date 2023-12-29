using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Peacekeeper
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class PeacekeeperIncapStatusEffect : StatusEffect
    {
        [Obfuscation(Exclude = true)]
        public Card Source
        {
            get;
            private set;
        }

        public override bool GeneratesTriggers => false;

        public PeacekeeperIncapStatusEffect(Card source)
        {
            Source = source;
        }

        public override string ToString()
        {
            return "The next time a hero uses a power, that hero deals 1 target 1 projectile damage.";
        }
    }
}
