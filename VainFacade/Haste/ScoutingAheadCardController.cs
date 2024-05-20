using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class ScoutingAheadCardController:HasteUtilityCardController
	{
		public ScoutingAheadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        public override IEnumerator Play()
        {
            //When this card enters play, add 4 tokens to your speed pool.
            return AddSpeedTokens(4);
        }

        public override void AddTriggers()
        {
            //At the end of your turn, you may remove 2 tokens from your speed pool. If you do, discard the top card of a deck or reveal and replace it, then repeat this text.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.ModifyTokens, TriggerType.DiscardCard, TriggerType.RevealCard });
        }

        public override string DecisionAction => " to discard or reveal the top card of a deck";

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
            IEnumerator coroutine = RemoveSpeedTokens(2, null, true, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            while (DidRemoveTokens(results,2))
            {
                List<SelectLocationDecision> deckResults = new List<SelectLocationDecision>();
                coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.None, (Location L) => base.GameController.IsLocationVisibleToSource(L, GetCardSource()), deckResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidSelectDeck(deckResults))
                {
                    Location selectedDeck = GetSelectedLocation(deckResults);
                    IEnumerable<Function> functionChoices = new Function[2]
                    {
                    new Function(base.HeroTurnTakerController, $"Discard the top card of {selectedDeck.GetFriendlyName()}", SelectionType.DiscardFromDeck, () => base.GameController.DiscardTopCard(selectedDeck, null, responsibleTurnTaker: this.TurnTaker,cardSource:GetCardSource()),null, $"{selectedDeck.GetFriendlyName()} has no cards to reveal"),
                    new Function(base.HeroTurnTakerController, $"Reveal and replace the top card of {selectedDeck.GetFriendlyName()}", SelectionType.RevealTopCardOfDeck, () => base.GameController.RevealAndReplaceCards(DecisionMaker, selectedDeck, 1, null, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards,cardSource:GetCardSource()),selectedDeck.HasCards)
                    };

                    SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
                    IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(choose);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(choose);
                    }
                }

                if (DidRemoveTokens(results, 2))
                {
                    results = new List<RemoveTokensFromPoolAction>();
                    coroutine = RemoveSpeedTokens(2, null, true, results);
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
}

