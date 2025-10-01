using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class SecretLairCardController : AreaCardController
    {
        public SecretLairCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show non-conspirator target with second highest HP
            SpecialStringMaker.ShowHighestHP(2,() => 1, isNonConspirator());
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            //Increase damage ddealt to conspirators by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => base.GameController.DoesCardContainKeyword(dd.Target, "conspirator"), 1);

            //At the start of the environment turn, each conspirator deals the non-conspirator target with the second highest HP 2 projectile damage each.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, TriggerType.DealDamage);
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            return MultipleDamageSourcesDealDamage(isConspirator(), TargetType.HighestHP, 2, isNonConspirator(), 2, DamageType.Projectile);
        }
    }
}
