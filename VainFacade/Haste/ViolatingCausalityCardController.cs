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
	public class ViolatingCausalityCardController:HasteUtilityCardController
	{
		public ViolatingCausalityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        public override string DecisionAction => " to put the top card of another trash into play or on the bottom of its deck";

        public override IEnumerator Play()
        {
            //Add 2 tokens to your speed pool.
            IEnumerator coroutine = AddSpeedTokens(2);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may remove a token from your speed pool to put the top card of another trash into play or on the bottom of its deck.
            bool flag = true;
            while (flag)
            {
                List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
                coroutine = RemoveSpeedTokens(1, null, true, results);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidRemoveTokens(results))
                {
                    List<SelectCardDecision> cardResults = new List<SelectCardDecision>();
                    coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCard, new LinqCardCriteria((Card c) => c.Location.IsTrash && c.Location.TopCard == c && c.Location != this.TurnTaker.Trash && base.GameController.IsLocationVisibleToSource(c.Location, GetCardSource()), "in another trash", false, true, ignoreBattleZone: true), cardResults, false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidSelectCard(cardResults))
                    {
                        Card selected = GetSelectedCard(cardResults);
                        List<PlayCardAction> playResults = new List<PlayCardAction>();
                        IEnumerable<Function> functionChoices = new Function[2]
                        {
                        new Function(base.HeroTurnTakerController, $"Put {selected.Title} into play", SelectionType.PutIntoPlay, () => base.GameController.PlayCard(this.TurnTakerController, selected, true, storedResults: playResults, responsibleTurnTaker: this.TurnTaker, cardSource:GetCardSource())),
                        new Function(base.HeroTurnTakerController, $"Put {selected.Title} on the bottom of its deck", SelectionType.MoveCardOnBottomOfDeck, () => base.GameController.MoveCard(this.TurnTakerController, selected, GetNativeDeck(selected), true, responsibleTurnTaker: this.TurnTaker, cardSource:GetCardSource()))
                        };

                        SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, new Card[] {selected}, GetCardSource());
                        IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(choose);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(choose);
                        }

                        //If a card enters play this way, {Haste} deals himself 2 energy damage. Otherwise, repeat this text.
                        if (DidPlayCards(playResults))
                        {
                            coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 2, DamageType.Energy, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(coroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(coroutine);
                            }
                            flag = false;
                        }
                    }
                }
                else
                {
                    flag = false;
                }
            }
        }
    }
}

