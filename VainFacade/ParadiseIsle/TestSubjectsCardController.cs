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
            AddThisCardControllerToList(CardControllerListType.ModifiesDeckKind);
            // Show current location of Dr. Wendigo
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c.Identifier == DrWendigoIdentifier, "Dr. Wendigo", false, false));
        }

        public static readonly string DrWendigoIdentifier = "DrWendigo";

        public override bool? AskIfIsHero(Card card, CardSource cardSource)
        {
            if (card == base.Card)
                return true;
            return base.AskIfIsHero(card, cardSource);
        }

        public override bool? AskIfIsHeroTarget(Card card, CardSource cardSource)
        {
            if (card == base.Card)
                return true;
            return base.AskIfIsHeroTarget(card, cardSource);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is reduced to 0 or fewer HP, each hero deals themself 1 psychic damage."
            AddBeforeDestroyAction(HeroGriefResponse);
            // "At the end of the environment turn, {DrWendigo} regains 2 HP, then deals this card 2 toxic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DrawDamageHealResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.DealDamage, TriggerType.GainHP });
            //At the start of the environment turn, 1 player may draw a card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => base.GameController.SelectHeroToDrawCard(DecisionMaker, true, cardSource: GetCardSource()),TriggerType.DrawCard);
        }

        private IEnumerator DrawDamageHealResponse(PhaseChangeAction pca)
        {
            Card madDoctor = FindCard(DrWendigoIdentifier);
            if (madDoctor != null && madDoctor.IsInPlayAndHasGameText)
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

                IEnumerator damageCoroutine = DealDamage(madDoctor, base.Card, 2, DamageType.Toxic, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
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
                    IEnumerator damageCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => IsHeroCharacterCard(c), 1, DamageType.Psychic, cardSource: GetCardSource());
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
