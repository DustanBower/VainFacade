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
    public class InfernalTwinCardController : CardController
    {
        public InfernalTwinCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a One-Shot enters play, you may destroy this card. If you do, resolve the text of that One-Shot a second time, then the character from that card's deck with the lowest HP deals itself 2 irreducible infernal damage."
            AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsOneShot && !base.Card.IsBeingDestroyed, OneShotResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard, TriggerType.DealDamage }, TriggerTiming.After);
        }

        private IEnumerator OneShotResponse(CardEntersPlayAction cepa)
        {
            // "...you may destroy this card."
            Func<GameAction, IEnumerator> action = AddBeforeDestroyAction((GameAction ga) => PlayAgainInfernalResponse(cepa));
            List<DestroyCardAction> destructionResults = new List<DestroyCardAction>();
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, optional: true, storedResults: destructionResults, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            RemoveDestroyAction(BeforeOrAfter.Before, action);
        }

        private IEnumerator PlayAgainInfernalResponse(CardEntersPlayAction cepa)
        {
            // "If you do, resolve the text of that One-Shot a second time, ..."
            Card oneshot = cepa.CardEnteringPlay;
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " resolves the text of " + oneshot.Title + " an additional time...", Priority.High, GetCardSource(), associatedCards: oneshot.ToEnumerable());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator playAgainCoroutine = FindCardController(oneshot).Play();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playAgainCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playAgainCoroutine);
            }
            // "... then the character from that card's deck with the lowest HP deals itself 2 irreducible infernal damage."
            List<Card> lowestResults = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => c.Owner == oneshot.Owner && c.IsCharacter && c.IsInPlayAndHasGameText, lowestResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card lowest = lowestResults.FirstOrDefault();
            if (lowest != null)
            {
                IEnumerator infernalCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => c == lowest, 2, DamageType.Infernal, isIrreducible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(infernalCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(infernalCoroutine);
                }
            }
        }
    }
}
