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
            else
            {
                // Let the players know that this effect won't work because Dr. Wendigo is not in play
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("Dr. Wendigo is not in play, so he can't deal damage to " + base.Card.Title + ".", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }

        private IEnumerator HeroGriefResponse(GameAction ga)
        {
            //Log.Debug("TestSubjectsCardController.HeroGriefResponse: ");
            //Log.Debug("TestSubjectsCardController.HeroGriefResponse: base.Card.HitPoints.HasValue: " + base.Card.HitPoints.HasValue.ToString());
            if (base.Card.HitPoints.HasValue)
            {
                //Log.Debug("TestSubjectsCardController.HeroGriefResponse: base.Card.HitPoints.Value: " + base.Card.HitPoints.Value.ToString());
                //Log.Debug("TestSubjectsCardController.HeroGriefResponse: base.Card.HitPoints.Value <= 0: " + (base.Card.HitPoints.Value <= 0).ToString());
                if (base.Card.HitPoints.Value <= 0)
                {
                    // "... each hero deals themself 1 psychic damage."
                    //Log.Debug("TestSubjectsCardController.HeroGriefResponse: running DealDamageToSelf");
                    IEnumerator damageCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => c.IsHeroCharacterCard, 1, DamageType.Psychic, cardSource: GetCardSource());
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
}
