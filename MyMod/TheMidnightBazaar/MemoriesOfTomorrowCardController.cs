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
    public class MemoriesOfTomorrowCardController : TheMidnightBazaarUtilityCardController
    {
        public MemoriesOfTomorrowCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, 1 player may move 1 card from their hand under [i]The Empty Well[/i] to discard the top card of each deck and put the top card from a trash into play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && IsEmptyWellInPlay(), SelectPlayerResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.DiscardCard, TriggerType.PutIntoPlay });
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
            // "... 1 player may move 1 card from their hand under [i]The Empty Well[/i] to discard the top card of each deck and put the top card from a trash into play."
            List<bool> cardsMoved = new List<bool>();
            currentMode = CustomMode.PlayerToDropCard;
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, DecisionMaker, GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.MoveCard, isOptional: true, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => DiscardAndPlayResponse(tt));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            yield break;
        }

        private IEnumerator DiscardAndPlayResponse(TurnTaker tt)
        {
            List<bool> cardsMoved = new List<bool>();
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
                TurnTakerController ttc = base.GameController.FindTurnTakerController(tt);
                HeroTurnTakerController httc = ttc.ToHero();
                // "... to discard the top card of each deck..."
                IEnumerator discardCoroutine = base.GameController.DiscardTopCardsOfDecks(httc, (Location l) => !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame, 1, responsibleTurnTaker: tt, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                // "... and put the top card from a trash into play."
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(httc, (Card c) => c.Location.IsTrash && base.GameController.IsLocationVisibleToSource(c.Location, GetCardSource()) && c == c.Location.TopCard, isPutIntoPlay: true, cardSource: GetCardSource(), noValidCardsMessage: "There are no cards in any trashes.");
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
