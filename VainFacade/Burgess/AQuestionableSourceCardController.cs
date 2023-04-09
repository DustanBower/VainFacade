using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class AQuestionableSourceCardController : BurgessUtilityCardController
    {
        public AQuestionableSourceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If not in play: show number of Clue cards in Burgess's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, ClueCard).Condition = () => !base.Card.IsInPlayAndHasGameText;
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, reveal the top card of your deck."
            List<Card> revealed = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, 1, revealed, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "If it is a Clue card, you may put it into play or in your hand."
            if (revealed.Count > 0)
            {
                Card revealedCard = revealed.FirstOrDefault();
                if (ClueCard.Criteria(revealedCard))
                {
                    MoveCardDestination[] options = new MoveCardDestination[3]
                    {
                        new MoveCardDestination(base.TurnTaker.PlayArea),
                        new MoveCardDestination(base.HeroTurnTaker.Hand),
                        new MoveCardDestination(base.TurnTaker.Deck)
                    };
                    IEnumerator moveCoroutine = base.GameController.SelectLocationAndMoveCard(base.HeroTurnTakerController, revealedCard, options, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
                // If the card wasn't moved, replace it
                List<Location> places = new List<Location>();
                places.Add(base.TurnTaker.Revealed);
                IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, places, base.TurnTaker.Deck, cardsInList: revealed);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cleanupCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cleanupCoroutine);
                }
            }
            // "Then draw a card."
            IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "{BurgessCharacter} deals himself up to 4 psychic damage."
            int maxAmt = GetPowerNumeral(0, 4);
            List<SelectNumberDecision> decisions = new List<SelectNumberDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectNumber(base.HeroTurnTakerController, SelectionType.DealDamage, 0, maxAmt, storedResults: decisions, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            /*Log.Debug("AQuestionableSourceCardController.UsePower: SelectNumber completed");
            foreach (SelectNumberDecision dec in decisions)
            {
                Log.Debug("AQuestionableSourceCardController.UsePower: decision: " + dec.ToString());
                Log.Debug("AQuestionableSourceCardController.UsePower: decision.Completed: " + dec.Completed.ToString());
                Log.Debug("AQuestionableSourceCardController.UsePower: decision.SelectedNumber: " + dec.SelectedNumber.ToString());
            }*/
            SelectNumberDecision firstCompleted = decisions.First((SelectNumberDecision d) => d.Completed);
            /*Log.Debug("AQuestionableSourceCardController.UsePower: firstCompleted: " + firstCompleted.ToString());
            Log.Debug("AQuestionableSourceCardController.UsePower: firstCompleted.Completed: " + firstCompleted.Completed.ToString());
            Log.Debug("AQuestionableSourceCardController.UsePower: firstCompleted.SelectedNumber: " + firstCompleted.SelectedNumber.ToString());*/
            int? damageAmt = firstCompleted.SelectedNumber;
            if (damageAmt.HasValue)
            {
                List<DealDamageAction> results = new List<DealDamageAction>();
                IEnumerator damageCoroutine = DealDamage(base.CharacterCard, base.CharacterCard, damageAmt.Value, DamageType.Psychic, storedResults: results, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
                // "For each point of damage he is dealt this way, draw a card."
                int totalDamage = 0;
                foreach (DealDamageAction dda in results)
                {
                    if (dda.DidDealDamage && dda.Target == base.CharacterCard)
                    {
                        totalDamage += dda.Amount;
                    }
                }
                IEnumerator drawCoroutine = DrawCards(base.HeroTurnTakerController, totalDamage);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
            // "Then destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            //Log.Debug("AQuestionableSourceCardController.UsePower: ");
        }
    }
}
