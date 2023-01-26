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
    public class ArcProjectorCardController : CardController
    {
        public ArcProjectorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play with >0 cards under: show list of hero targets with highest HP with length equal to number of cards under this card
            SpecialStringMaker.ShowHighestHP(1, () => base.Card.UnderLocation.NumberOfCards, new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget, "hero", singular: "target", plural: "targets")).Condition = () => base.Card.IsInPlayAndHasGameText && base.Card.UnderLocation.HasCards;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {BlitzCharacter} is dealt lightning damage, put 1 card from the villain trash beneath this card for each point of damage dealt."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageType == DamageType.Lightning && dda.DidDealDamage, StockpileFromTrashResponse, TriggerType.MoveCard, TriggerTiming.After);
            // "At the end of the villain turn, {BlitzCharacter} deals the X hero targets with the highest HP {H - 2} lightning damage, where X = the number of cards beneath this card. When a target is dealt damage this way, discard a card from beneath this card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDiscardResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DiscardCard });
        }

        private IEnumerator StockpileFromTrashResponse(DealDamageAction dda)
        {
            // "... put 1 card from the villain trash beneath this card for each point of damage dealt."
            int numToMove = dda.Amount;
            string messageText = base.Card.Title + " moves the top " + numToMove.ToString() + " cards of the villain trash under itself.";
            if (numToMove == 1)
            {
                messageText = base.Card.Title + " moves the top card of the villain trash under itself.";
            }
            if (!base.TurnTaker.Trash.HasCards)
            {
                numToMove = 0;
                messageText = "There are no cards in the villain trash for " + base.Card.Title + " to move.";
            }
            else if (base.TurnTaker.Trash.NumberOfCards < dda.Amount)
            {
                numToMove = base.TurnTaker.Trash.NumberOfCards;
                if (numToMove == 1)
                {
                    messageText = "There is only " + base.TurnTaker.Trash.NumberOfCards.ToString() + " card in the villain trash, so " + base.Card.Title + " moves it under itself.";
                }
                else
                {
                    messageText = "There are only " + base.TurnTaker.Trash.NumberOfCards.ToString() + " cards in the villain trash, so " + base.Card.Title + " moves all of them under itself.";
                }
            }
            List<Card> cardsToMove = new List<Card>();
            if (numToMove > 0)
            {
                cardsToMove = base.TurnTaker.Trash.GetTopCards(numToMove).ToList();
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(messageText, Priority.Medium, cardSource: GetCardSource(), associatedCards: cardsToMove, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, cardsToMove, base.Card.UnderLocation, toBottom: true, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        private IEnumerator DamageDiscardResponse(PhaseChangeAction pca)
        {
            // "... {BlitzCharacter} deals the X hero targets with the highest HP {H - 2} lightning damage, where X = the number of cards beneath this card."
            // "When a target is dealt damage this way, discard a card from beneath this card."
            ITrigger discardTrigger = AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.CardSource.Card == base.Card && dda.DidDealDamage, (DealDamageAction dda) => base.GameController.DiscardTopCard(base.Card.UnderLocation, null, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), TriggerType.DiscardCard, TriggerTiming.After);
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero && c.IsTarget, (Card c) => H - 2, DamageType.Lightning, numberOfTargets: () => base.Card.UnderLocation.NumberOfCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            RemoveTrigger(discardTrigger);
        }
    }
}
