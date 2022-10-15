using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class RuinousRampageCardController : CardController
    {
        public RuinousRampageCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddStartOfGameTriggers()
        {
            base.AddStartOfGameTriggers();
            // "When this card is revealed, play it."
            AddTrigger((MoveCardAction mca) => mca.CardToMove == base.Card && mca.Destination.IsRevealed, PlayWithMessageResponse, TriggerType.PlayCard, TriggerTiming.After, outOfPlayTrigger: true);
            AddTrigger((RevealCardsAction rca) => rca.RevealedCards.Contains(base.Card), PlayWithMessageResponse, TriggerType.PlayCard, TriggerTiming.After, outOfPlayTrigger: true);
        }

        private IEnumerator PlayWithMessageResponse(GameAction ga)
        {
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " was revealed, and plays itself!", Priority.High, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, base.Card, actionSource: ga, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        public override IEnumerator Play()
        {
            List<DealDamageAction> meleeHits = new List<DealDamageAction>();
            IEnumerator loopCoroutine = DamageDamageHealRepeatResponse(meleeHits);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(loopCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(loopCoroutine);
            }
        }

        private IEnumerator DamageDamageHealRepeatResponse(List<DealDamageAction> meleeResults)
        {
            // "{TheFuryCharacter} deals herself 0 psychic damage, ..."
            List<DealDamageAction> psychicHits = new List<DealDamageAction>();
            IEnumerator psychicCoroutine = DealDamage(base.CharacterCard, base.CharacterCard, 0, DamageType.Psychic, storedResults: psychicHits, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(psychicCoroutine);
            }
            // "... then deals 1 target that has not been dealt melee damage this way this turn X melee damage, where X = 2 plus the damage dealt this way minus the number of targets dealt melee damage this way this turn."
            int x = 2;
            foreach(DealDamageAction dda in psychicHits)
            {
                if (dda.DidDealDamage)
                {
                    x += dda.Amount;
                }
            }
            List<Card> meleeTargetsSoFar = (from DealDamageAction dda in meleeResults where dda.DidDealDamage && dda.DamageType == DamageType.Melee select dda.Target).Distinct().ToList();
            x -= meleeTargetsSoFar.Count;
            List<DealDamageAction> currentMeleeHits = new List<DealDamageAction>();
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), x, DamageType.Melee, 1, false, 1, additionalCriteria: (Card c) => !meleeTargetsSoFar.Contains(c), storedResultsDamage: currentMeleeHits, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "If melee damage is dealt this way, ..."
            bool didDealMelee = false;
            foreach (DealDamageAction dda in currentMeleeHits)
            {
                if (dda.DidDealDamage && dda.DamageType == DamageType.Melee)
                {
                    didDealMelee = true;
                }
            }
            if (didDealMelee)
            {
                // "... {TheFuryCharacter} regains 1 HP, ..."
                IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
                // "... then repeat this text."
                meleeResults.AddRange(currentMeleeHits);
                IEnumerator repeatCoroutine = DamageDamageHealRepeatResponse(meleeResults);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(repeatCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(repeatCoroutine);
                }
            }
        }
    }
}
