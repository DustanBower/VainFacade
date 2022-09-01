using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class SheriffSmithyCardController : AfflictedCardController
    {
        public SheriffSmithyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Quaint Country Town: show the non-environment target with the second highest HP
            SpecialStringMaker.ShowNonEnvironmentTargetWithHighestHP(ranking: 2).Condition = () => CanActivateEffect(base.TurnTakerController, QuaintKey);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When an environment target is dealt damage or destroyed, this card deals the non-environment target with the second highest HP 2 sonic damage."
            AddTrigger((DealDamageAction dda) => dda.Target.IsEnvironmentTarget && dda.DidDealDamage && CanActivateEffect(base.TurnTakerController, QuaintKey), (DealDamageAction dda) => DealDamageToHighestHP(base.Card, 2, (Card c) => !c.IsEnvironmentTarget, (Card c) => 2, DamageType.Sonic), TriggerType.DealDamage, TriggerTiming.After);
            AddTrigger((DestroyCardAction dca) => dca.CardToDestroy.Card.IsEnvironmentTarget && dca.WasCardDestroyed && CanActivateEffect(base.TurnTakerController, QuaintKey), (DestroyCardAction dca) => DealDamageToHighestHP(base.Card, 2, (Card c) => !c.IsEnvironmentTarget, (Card c) => 2, DamageType.Sonic), TriggerType.DealDamage, TriggerTiming.After);
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "... play the top card of the villain deck."
            yield return PlayTheTopCardOfTheVillainDeckResponse(null);
        }
    }
}
