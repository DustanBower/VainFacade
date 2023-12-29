using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Model;
namespace VainFacadePlaytest.Peacekeeper
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class BulletHellStatusEffect : OnDealDamageStatusEffect
    {
        [Obfuscation(Exclude = true)]
        public int ID
        {
            get;
            private set;
        }

        //public override bool GeneratesTriggers => false;

        public BulletHellStatusEffect(Card cardWithMethod, string methodToExecute, string description, TriggerType[] triggerTypes, TurnTaker decisionMaker, Card cardSource, int[] powerNumerals = null, int id = 0)
            : base(cardWithMethod, methodToExecute, description, triggerTypes, decisionMaker, cardSource, powerNumerals)
        {
            ID = id;
        }
    }
}
