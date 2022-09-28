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
    public class ForteanFoeCardController : TheFuryUtilityCardController
    {
        public ForteanFoeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show number of Coincidence cards in deck, trash, and hand
            SpecialStringMaker.ShowNumberOfCardsAtLocations(() => new Location[3] { base.TurnTaker.Deck, base.TurnTaker.Trash, base.TurnTaker.ToHero().Hand }, IsCoincidence);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "Coincidences are indestructible."
            if (card.DoKeywordsContain(CoincidenceKeyword))
            {
                return true;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of your turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, search your deck and trash for any number of Coincidences and put them into your hand. If you searched your deck, shuffle it."
            // Search your trash and take as many as you want
            List<MoveCardDestination> dests = new List<MoveCardDestination>();
            dests.Add(new MoveCardDestination(base.TurnTaker.ToHero().Hand));
            IEnumerator trashCoroutine = base.GameController.SelectCardsFromLocationAndMoveThem(DecisionMaker, base.TurnTaker.Trash, 0, 3, IsCoincidence, dests, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(trashCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(trashCoroutine);
            }
            // Decide whether to search your deck as well
            YesNoDecision choice = new YesNoDecision(base.GameController, DecisionMaker, SelectionType.SearchDeck, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choice))
            {
                // If yes, search your deck and take as many as you want, then shuffle your deck
                IEnumerator deckCoroutine = base.GameController.SelectCardsFromLocationAndMoveThem(DecisionMaker, base.TurnTaker.Deck, 0, 3, IsCoincidence, dests, shuffleAfterwards: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(deckCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(deckCoroutine);
                }
            }
            // "You may play any number of Coincidences."
            IEnumerator playCoroutine = SelectAndPlayCardsFromHand(DecisionMaker, 3, cardCriteria: IsCoincidence);
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
