using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class GlacialShieldStatusEffect:ReduceDamageStatusEffect
	{
		public GlacialShieldStatusEffect(int amount):base(amount)
		{
		}

        public override string ToString()
        {
            string text = $"Reduce damage dealt to {this.TargetCriteria.OwnedBy.Name}'s targets by targets other than {this.SourceCriteria.IsNotSpecificCard.Title} by 1.";
            if (this.TargetCriteria.OwnedBy.Name.Contains("Guise"))
            {
                text += " So cool!";
            }
            return text;
        }
    }
}

