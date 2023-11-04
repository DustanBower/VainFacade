using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace VainFacadePlaytest.Push
{
    public class KineticAbsorptionCardController : PushCardControllerUtilities
    {
        public KineticAbsorptionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            //When Push is dealt melee or projectile damage, draw a card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && (dd.DamageType == DamageType.Melee || dd.DamageType == DamageType.Projectile) && dd.DidDealDamage, (DealDamageAction dd) => DrawCard(), TriggerType.DrawCard, TriggerTiming.After);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Reveal the top 3 cards of your deck. You may put 1 revealed ongoing into your hand or into play. Discard the remaining cards.
            //If an ongoing entered play this way, destroy this card.
            //Based on Foresight

            bool optional = false;
            int numberToReveal = GetPowerNumeral(0, 3);
            List<Card> revealedCards = new List<Card>();
            List<MoveCardAction> moveResults = new List<MoveCardAction>();
            IEnumerator coroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, numberToReveal, revealedCards, fromBottom: false, RevealedCardDisplay.ShowRevealedCards, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            List<Card> actuallyRevealedCards = revealedCards.Where((Card c) => c.Location.IsRevealed).ToList();
            List<Card> allRevealedCards = new List<Card>(actuallyRevealedCards);
            if (actuallyRevealedCards.Count() > 0)
            {
                if (actuallyRevealedCards.Count() < numberToReveal)
                {
                    int num = revealedCards.Count();
                    IEnumerator coroutine2 = base.GameController.SendMessageAction($"There {num.ToString_IsOrAre()} only {num} {num.ToString_CardOrCards()} in {base.TurnTaker.Name}'s deck.", Priority.High, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine2);
                    }
                    optional = true;
                }
                int powerNumeral = GetPowerNumeral(1, 1);
                List<MoveCardDestination> destinations = new List<MoveCardDestination>
                {
                    new MoveCardDestination(base.HeroTurnTaker.Hand),
                    new MoveCardDestination(base.TurnTaker.PlayArea)
                };
                List<SelectCardDecision> storedCards = new List<SelectCardDecision>();
                GameController gameController = base.GameController;
                HeroTurnTakerController decisionMaker = DecisionMaker;
                Location revealed = base.TurnTaker.Revealed;
                int? minNumberOfCards = powerNumeral;
                LinqCardCriteria criteria = new LinqCardCriteria((Card c) => actuallyRevealedCards.Contains(c) && IsOngoing(c));
                bool optional2 = optional;
                CardSource cardSource = GetCardSource();
                IEnumerator coroutine3 = gameController.SelectCardsFromLocationAndMoveThem(decisionMaker, revealed, minNumberOfCards, powerNumeral, criteria, destinations, isPutIntoPlay: true, playIfMovingToPlayArea: true, shuffleAfterwards: false, optional2, storedCards, moveResults, autoDecideCard: false, flipFaceDown: false, showOutput: false, null, isDiscardIfMovingToTrash: false, allowAutoDecide: false, null, null, cardSource);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
            }
            List<Location> list = new List<Location>();
            list.Add(base.TurnTaker.Revealed);
            IEnumerator coroutine5 = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, list, base.TurnTaker.Trash, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: true, isDiscard: true, isReturnedToOriginalLocation: false, allRevealedCards, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine5);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine5);
            }

            //Console.WriteLine($"Move Results: {moveResults.ToCommaList()}");
            //Console.WriteLine($"Is Ongoing: {IsOngoing(moveResults.FirstOrDefault().CardToMove)}");
            //Console.WriteLine($"Destination is play area: {moveResults.FirstOrDefault().Destination.IsPlayArea}");
            //Console.WriteLine($"Was Card Moved: {moveResults.FirstOrDefault().WasCardMoved}");
            if (moveResults.FirstOrDefault() != null && moveResults.Any((MoveCardAction mc) => IsOngoing(mc.CardToMove) && mc.Destination.IsPlayArea))
            {
                coroutine = DestroyThisCardResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }
    }
}

