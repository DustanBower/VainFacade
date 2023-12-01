using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class VoiceForTheVoicelessCardController:CardController
	{
		public VoiceForTheVoicelessCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Deck, new LinqCardCriteria((Card c) => base.GameController.DoesCardContainKeyword(c, "dirge"), "", false, false, "dirge", "dirges"));
		}

		public override IEnumerator Play()
		{
            //Search your deck for a dirge and put it into your hand. Shuffle your deck.
            IEnumerator coroutine = SearchForCards(DecisionMaker, true, false, 1, 1, new LinqCardCriteria((Card c) => base.GameController.DoesCardContainKeyword(c, "dirge"), "", false, false, "dirge", "dirges"), false, true, false, shuffleAfterwards: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may draw a card.
            coroutine = DrawCard(this.HeroTurnTaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may play a card.
            coroutine = SelectAndPlayCardFromHand(DecisionMaker);
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

