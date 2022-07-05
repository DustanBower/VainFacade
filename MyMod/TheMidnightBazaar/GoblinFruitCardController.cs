using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacade.TheMidnightBazaar
{
    public class GoblinFruitCardController : TheMidnightBazaarUtilityCardController
    {
        public GoblinFruitCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, a player may move a card from their hand under [i]The Empty Well[/i] to put the bottom card of their deck into play. If they do not, put the bottom card of the villain deck into play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && IsEmptyWellInPlay(), SelectPlayerResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.PutIntoPlay });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, search the environment deck and trash for [i]The Empty Well[/i] and put it into play. Shuffle the environment deck."
            IEnumerator fetchCoroutine = FetchWellResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(fetchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(fetchCoroutine);
            }
        }

        private IEnumerator SelectPlayerResponse(PhaseChangeAction pca)
        {
            // "... a player may move a card from their hand under [i]The Empty Well[/i] to put the bottom card of their deck into play. If they do not, put the bottom card of the villain deck into play."
            List<bool> cardsMoved = new List<bool>();
            currentMode = CustomMode.PlayerToDropCard;
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, DecisionMaker, GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.MoveCard, isOptional: true, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => BottomCardResponse(tt, cardsMoved));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            int cardsDropped = 0;
            foreach (bool b in cardsMoved)
            {
                if (b)
                    cardsDropped++;
            }
            if (cardsDropped <= 0)
            {
                // "If they do not, put the bottom card of the villain deck into play."
                List<SelectLocationDecision> locationDecisions = new List<SelectLocationDecision>();
                IEnumerator selectDeckCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.PlayBottomCardOfVillainDeck, (Location l) => l.IsVillain, locationDecisions, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectDeckCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectDeckCoroutine);
                }
                SelectLocationDecision choice = locationDecisions.FirstOrDefault();
                if (choice != null && choice.SelectedLocation.Location != null)
                {
                    Location deck = choice.SelectedLocation.Location;
                    TurnTakerController ttc = base.GameController.FindTurnTakerController(deck.OwnerTurnTaker);
                    IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, ttc, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, playBottomInstead: true, showMessage: true, cardSource: GetCardSource());
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
            yield break;
        }

        private IEnumerator BottomCardResponse(TurnTaker tt, List<bool> cardsMoved)
        {
            // "... a player may move a card from their hand under [i]The Empty Well[/i] to put the bottom card of their deck into play."
            IEnumerator moveCoroutine = DropCardsFromHand(tt, 1, false, true, cardsMoved, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            int cardsDropped = 0;
            foreach (bool b in cardsMoved)
            {
                if (b)
                    cardsDropped++;
            }
            if (cardsDropped > 0)
            {
                IEnumerator playCoroutine = base.GameController.PlayTopCard(base.GameController.FindTurnTakerController(tt).ToHero(), base.GameController.FindTurnTakerController(tt), isPutIntoPlay: true, responsibleTurnTaker: tt, playBottomInstead: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }
    }
}
