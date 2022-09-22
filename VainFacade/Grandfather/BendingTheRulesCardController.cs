using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class BendingTheRulesCardController : GrandfatherUtilityCardController
    {
        public BendingTheRulesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is dealt damage, {Grandfather} deals himself X energy damage that cannot be redirected, where X = the damage this card takes this way."
            AddTrigger((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.Card, GrampsHurtsHimselfResponse, TriggerType.DealDamage, TriggerTiming.After);
            // "At the end of the villain turn, discard the top card of the villain deck. If a target is discarded this way, play the top card of the villain deck. Otherwise, discard the top 5 cards of each hero deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardPlayOrDiscardResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard });
        }

        private IEnumerator GrampsHurtsHimselfResponse(DealDamageAction dda)
        {
            // "... {Grandfather} deals himself X energy damage that cannot be redirected, where X = the damage this card takes this way."
            ITrigger tempTrigger = AddMakeDamageNotRedirectableTrigger((DealDamageAction damage) => damage != null && damage.CardSource != null && damage.CardSource.Card == base.Card);
            IEnumerator selfDamageCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => c == base.CharacterCard, (Card c) => dda.Amount, DamageType.Energy, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDamageCoroutine);
            }
            RemoveTrigger(tempTrigger);
        }

        private IEnumerator DiscardPlayOrDiscardResponse(PhaseChangeAction pca)
        {
            // "... discard the top card of the villain deck."
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, results, showCard: (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            bool wasTargetDiscarded = false;
            MoveCardAction discard = results.Where((MoveCardAction mca) => mca.WasCardMoved).FirstOrDefault();
            if (discard != null)
            {
                Card discarded = discard.CardToMove;
                if (discarded.IsTarget)
                {
                    wasTargetDiscarded = true;
                }
            }
            // "If a target is discarded this way, play the top card of the villain deck."
            if (wasTargetDiscarded)
            {
                IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            else
            {
                // "Otherwise, discard the top 5 cards of each hero deck."
                IEnumerator massDiscardCoroutine = DiscardTopXCardsOfEachHeroDeckResponse(5, pca);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(massDiscardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(massDiscardCoroutine);
                }
            }
        }
    }
}
