using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class ProstheticEverymanCardController : MasqueCardController
    {
        public ProstheticEverymanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show whether Carnaval has been dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, base.CharacterCard.Title + " has already been dealt damage this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not been dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstDamageThisTurn = "FirstDamageThisTurn";
        private ITrigger _reduceTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce the first damage dealt to {CarnavalCharacter} each turn by 1."
            AddTrigger((DealDamageAction dda) => true, LogDealDamageActionResponse, TriggerType.Hidden, TriggerTiming.Before);
            //AddTrigger((DealDamageAction dda) => dda.CanDealDamage && dda.Target == base.CharacterCard && !HasBeenSetToTrueThisTurn(FirstDamageThisTurn), ReduceDamageResponse, TriggerType.ReduceDamage, TriggerTiming.Before);
            _reduceTrigger = AddReduceDamageTrigger((DealDamageAction dda) => !IsPropertyTrue(FirstDamageThisTurn) && dda.CanDealDamage, ReduceDamageResponse, (Card c) => c == base.CharacterCard, oncePerTurn: true);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageThisTurn), TriggerType.Hidden);
        }

        private IEnumerator LogDealDamageActionResponse(DealDamageAction dda)
        {
            Log.Debug("ProstheticEverymanCardController.LogDealDamageActionResponse activated");
            Log.Debug("ProstheticEverymanCardController.LogDealDamageActionResponse: dda: " + dda.ToString());
            Log.Debug("ProstheticEverymanCardController.LogDealDamageActionResponse: dda.Target: " + dda.Target.Title);
            Log.Debug("ProstheticEverymanCardController.LogDealDamageActionResponse: dda.CanDealDamage: " + (dda.CanDealDamage).ToString());
            Log.Debug("ProstheticEverymanCardController.LogDealDamageActionResponse: dda.Target == base.CharacterCard: " + (dda.Target == base.CharacterCard).ToString());
            Log.Debug("ProstheticEverymanCardController.LogDealDamageActionResponse: !HasBeenSetToTrueThisTurn(FirstDamageThisTurn): " + (!HasBeenSetToTrueThisTurn(FirstDamageThisTurn)).ToString());
            yield break;
        }

        private IEnumerator ReduceDamageResponse(DealDamageAction dda)
        {
            Log.Debug("ProstheticEverymanCardController.ReduceDamageResponse activated");
            SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, _reduceTrigger, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Reveal the top 2 cards of a non-hero deck. Put them back in any order."
            int numCards = GetPowerNumeral(0, 2);
            List<SelectLocationDecision> choices = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealCardsFromDeck, (Location l) => !l.IsHero, choices, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectLocationDecision chosen = choices.FirstOrDefault();
            if (chosen != null && chosen.SelectedLocation.Location != null)
            {
                Location deck = chosen.SelectedLocation.Location;
                MoveCardDestination toTopDeck = new MoveCardDestination(deck);
                List<Card> revealedCards = new List<Card>();
                IEnumerator revealCoroutine = RevealCardsFromTopOfDeck_DetermineTheirLocation(DecisionMaker, base.TurnTakerController, deck, toTopDeck, toTopDeck, numCards, 0, numCards, base.TurnTaker, revealedCards);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }
                List<Location> toClean = new List<Location>();
                toClean.Add(deck.OwnerTurnTaker.Revealed);
                IEnumerator cleanupCoroutine = CleanupCardsAtLocations(toClean, deck, cardsInList: revealedCards);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cleanupCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cleanupCoroutine);
                }
            }
            // "You may play a Trap or Masque."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(DecisionMaker, cardCriteria: new LinqCardCriteria((Card c) => c.DoKeywordsContain(MasqueKeyword) || c.DoKeywordsContain(TrapKeyword), "Trap or Masque"));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
