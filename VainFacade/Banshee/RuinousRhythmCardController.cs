using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class RuinousRhythmCardController:CardController
	{
		public RuinousRhythmCardController(Card card, TurnTakerController turnTakerController)
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
                        //Destroy a non-character, non-target card. If a non-ongoing is destroyed this way, destroy this card.
                        List<DestroyCardAction> results = new List<DestroyCardAction>();
                        coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => !c.IsCharacter && !c.IsTarget, "non-character, non-target"), false, results, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        if (DidDestroyCard(results))
                        {
                            Card card = GetDestroyedCards(results).FirstOrDefault();
                            if (card != null && !IsOngoing(card))
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
                        break;
                    }
                case 1:
                    {
                        //Draw a card.
                        coroutine = DrawCard();
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
    }
}

