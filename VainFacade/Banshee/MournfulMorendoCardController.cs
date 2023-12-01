using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class MournfulMorendoCardController:CardController
	{
		public MournfulMorendoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        //Select an environment card.
                        List<SelectCardDecision> results = new List<SelectCardDecision>();
                        coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ShuffleCardIntoDeck, new LinqCardCriteria((Card c) => c.IsEnvironment && c.IsInPlayAndHasGameText, "environment"), results, false, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        Card card = GetSelectedCard(results);

                        if (card != null)
                        {
                            //You may shuffle it into its deck.
                            List<YesNoCardDecision> yesno = new List<YesNoCardDecision>();
                            coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, card, null, yesno, new Card[] { card }, GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(coroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(coroutine);
                            }

                            if (DidPlayerAnswerYes(yesno))
                            {
                                coroutine = base.GameController.ShuffleCardIntoLocation(DecisionMaker, card, FindEnvironment().TurnTaker.Deck, false);
                                if (base.UseUnityCoroutines)
                                {
                                    yield return base.GameController.StartCoroutine(coroutine);
                                }
                                else
                                {
                                    base.GameController.ExhaustCoroutine(coroutine);
                                }

                                //If you do, destroy this card.
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
                            else
                            {
                                //Otherwise, destroy the selected card.
                                coroutine = base.GameController.DestroyCard(DecisionMaker, card, cardSource: GetCardSource());
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
                        break;
                    }
                case 1:
                    {
                        //Play a card.
                        coroutine = SelectAndPlayCardFromHand(DecisionMaker, false);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        break;
                    }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision && ((YesNoCardDecision)decision).Card != null)
            {
                Card target = ((YesNoCardDecision)decision).Card;
                string text = $"shuffle {target.Title} into its deck";
                return new CustomDecisionText(
                $"Do you want to {text}?",
                $"{decision.DecisionMaker.Name} is deciding whether to {text}.",
                $"Vote for whether to {text}.",
                $"whether to {text}."
                );
            }
            return null;
        }
    }
}

