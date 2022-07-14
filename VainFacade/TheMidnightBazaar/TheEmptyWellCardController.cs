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
    public class TheEmptyWellCardController : TheMidnightBazaarUtilityCardController
    {
        public TheEmptyWellCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show list of cards under this card (if this is in play and has cards under it)
            SpecialStringMaker.ShowListOfCardsAtLocation(base.Card.UnderLocation, new LinqCardCriteria((Card c) => true, ""), () => base.Card.IsInPlayAndHasGameText && base.Card.UnderLocation.HasCards).Condition = () => base.Card.IsInPlayAndHasGameText && base.Card.UnderLocation.HasCards;
            // Show "There are no cards under The Empty Well." (if this is in play and has no cards under it)
            SpecialStringMaker.ShowSpecialString(() => "There are no cards under " + base.Card.Title + ".", () => base.Card.IsInPlayAndHasGameText && !base.Card.UnderLocation.HasCards).Condition = () => base.Card.IsInPlayAndHasGameText && !base.Card.UnderLocation.HasCards;
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible."
            return card == base.Card;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a card with the same title from the same deck as a card under this card would enter play, remove it from the game instead."
            AddTrigger((CardEntersPlayAction c) => base.Card.UnderLocation.Cards.Any((Card u) => u.Title == c.CardEnteringPlay.Title && u.Owner == c.CardEnteringPlay.Owner) && base.GameController.IsCardVisibleToCardSource(c.CardEnteringPlay, GetCardSource()), RemoveInsteadResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.RemoveFromGame }, TriggerTiming.Before);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, play the top card of the environment deck."
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator RemoveInsteadResponse(CardEntersPlayAction cepa)
        {
            // "... remove it from the game instead."
            Card entering = cepa.CardEnteringPlay;
            List<Card> associated = new List<Card>(1);
            associated.Add(entering);
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(entering.Title + " matches a card under " + base.Card.Title + ", so " + base.Card.Title + " removes it from the game!", Priority.High, GetCardSource(), associatedCards: associated);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator cancelCoroutine = CancelAction(cepa);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator removeCoroutine = base.GameController.MoveCard(base.TurnTakerController, entering, entering.Owner.OutOfGame, responsibleTurnTaker: base.TurnTaker, actionSource: cepa, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }
            yield break;
        }
    }
}
