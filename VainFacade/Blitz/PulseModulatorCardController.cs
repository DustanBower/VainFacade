using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class PulseModulatorCardController : CardController
    {
        public PulseModulatorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show list of One-Shot cards under this card
            SpecialStringMaker.ShowListOfCardsAtLocation(base.Card.UnderLocation, new LinqCardCriteria((Card c) => c.IsOneShot, "One-Shot")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {BlitzCharacter} is dealt lightning damage, move a card from the top of the villain deck beneath this card for each point of damage dealt this way."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageType == DamageType.Lightning && dda.DidDealDamage, (DealDamageAction dda) => base.GameController.MoveCards(base.TurnTakerController, base.TurnTaker.Deck.GetTopCards(dda.Amount), base.Card.UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), TriggerType.MoveCard, TriggerTiming.After);
            // "At the start of the villain turn, shuffle the cards beneath this one and discard cards from under this card until a One-Shot is discarded. If a One-Shot is discarded this way, put it into play."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardPlayOneShotResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        private IEnumerator DiscardPlayOneShotResponse(PhaseChangeAction pca)
        {
            // "... shuffle the cards beneath this one..."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.Card.UnderLocation, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            // "... and discard cards from under this card until a One-Shot is discarded."
            Card oneShotDiscarded = null;
            List<MoveCardAction> results = new List<MoveCardAction>();
            while (base.Card.UnderLocation.HasCards && oneShotDiscarded == null)
            {
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.Card.UnderLocation, results, (Card c) => true, base.TurnTaker, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                MoveCardAction firstRelevant = results.Where((MoveCardAction mca) => mca.WasCardMoved && mca.CardToMove.IsOneShot).FirstOrDefault();
                if (firstRelevant != null)
                {
                    oneShotDiscarded = firstRelevant.CardToMove;
                }
            }
            // "If a One-Shot is discarded this way, put it into play."
            IEnumerator oneShotCoroutine = base.GameController.SendMessageAction("None of the discarded cards were One-Shots.", Priority.Medium, GetCardSource(), showCardSource: true);
            if (oneShotDiscarded != null)
            {
                oneShotCoroutine = base.GameController.PlayCard(base.TurnTakerController, oneShotDiscarded, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(oneShotCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(oneShotCoroutine);
            }
        }
    }
}
