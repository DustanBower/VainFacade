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
    public class LilMarieCardController : AfflictedCardController
    {
        public LilMarieCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Howls in the Distance is NOT active: show the hero with the lowest HP
            SpecialStringMaker.ShowHeroCharacterCardWithLowestHP().Condition = () => !CanActivateEffect(base.TurnTakerController, HowlsKey);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce energy, projectile, and toxic damage by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => (dda.DamageType == DamageType.Energy || dda.DamageType == DamageType.Projectile || dda.DamageType == DamageType.Toxic) && !CanActivateEffect(base.TurnTakerController, HowlsKey), (DealDamageAction dda) => 1);
            // "At the end of the environment turn, this card deals itself or the hero with the lowest HP 1 psychic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && !CanActivateEffect(base.TurnTakerController, HowlsKey), (PhaseChangeAction pca) => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), 1, DamageType.Psychic, 1, false, 1, additionalCriteria: (Card c) => c == base.Card || CanCardBeConsideredLowestHitPoints(c, (Card x) => IsHeroCharacterCard(x)), cardSource: GetCardSource()), TriggerType.DealDamage);
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "... each non-villain target deals itself 2 irreducible psychic damage."
            yield return base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => !IsVillainTarget(c), 2, DamageType.Psychic, isIrreducible: true, cardSource: GetCardSource());
        }
    }
}
