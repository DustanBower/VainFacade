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
    public class TestSubjectsCardController : CardController
    {
        public TestSubjectsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show current location of Dr. Wendigo
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c.Identifier == DrWendigoIdentifier, "Dr. Wendigo", false, false));
        }

        public static readonly string DrWendigoIdentifier = "DrWendigo";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, 1 player may draw a card. Then, {DrWendigo} deals this card 2 toxic damage. If {DrWendigo} deals damage this way, he regains 2 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DrawDamageHealResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.DealDamage, TriggerType.GainHP });
            // "When this card is reduced to 0 or fewer HP, each hero deals themself 2 psychic damage."
            AddBeforeDestroyAction(HeroGriefResponse);
        }

        private IEnumerator DrawDamageHealResponse(PhaseChangeAction pca)
        {
            // "... 1 player may draw a card."
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCard(DecisionMaker, optionalSelectHero: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Then, {DrWendigo} deals this card 2 toxic damage."
            Card madDoctor = FindCard(DrWendigoIdentifier);
            if (madDoctor != null && madDoctor.IsInPlayAndHasGameText)
            {
                List<DealDamageAction> damageResults = new List<DealDamageAction>();
                IEnumerator damageCoroutine = DealDamage(madDoctor, base.Card, 2, DamageType.Toxic, storedResults: damageResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
                // "If {DrWendigo} deals damage this way, he regains 2 HP."
                if (DidDealDamage(damageResults, fromDamageSource: madDoctor) && madDoctor.IsInPlayAndHasGameText && madDoctor.IsTarget)
                {
                    IEnumerator healCoroutine = base.GameController.GainHP(madDoctor, 2, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(healCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(healCoroutine);
                    }
                }
            }
        }

        private IEnumerator HeroGriefResponse(GameAction ga)
        {
            if (base.Card.HitPoints.Value <= 0)
            {
                // "... each hero deals themself 2 psychic damage."
                IEnumerator damageCoroutine = base.GameController.SelectTargetsToDealDamageToSelf(DecisionMaker, 2, DamageType.Psychic, null, false, null, allowAutoDecide: true, additionalCriteria: (Card c) => c.IsHeroCharacterCard, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
        }
    }
}
