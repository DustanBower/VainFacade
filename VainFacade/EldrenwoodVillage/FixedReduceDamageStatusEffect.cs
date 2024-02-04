using System;
using System.Reflection;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;


namespace VainFacadePlaytest.EldrenwoodVillage
{
    [Serializable]
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class FixedReduceDamageStatusEffect:ReduceDamageStatusEffect
	{
		public FixedReduceDamageStatusEffect(int amount):base(amount)
		{
		}

        public override bool IsSameAs(StatusEffect other)
        {
            if (other is ReduceDamageStatusEffect && ((ReduceDamageStatusEffect)other).TargetCriteria.OwnedBy == this.TargetCriteria.OwnedBy)
            {
                return base.IsSameAs(other);
            }
            return false;
        }
    }
}

