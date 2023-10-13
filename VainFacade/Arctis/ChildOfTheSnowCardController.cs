using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class ChildOfTheSnowCardController:ArctisCardUtilities
	{
		public ChildOfTheSnowCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsIcework(c), "", false, false, "icework", "iceworks"));
		}

        public override IEnumerator Play()
        {
            //Draw a card.
            IEnumerator coroutine = DrawCard();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Search your deck for an icework and put it into your hand. Shuffle your deck.
            coroutine = SearchForCards(DecisionMaker, true, false, 1, 1, new LinqCardCriteria((Card c) => IsIcework(c), "", false, false, "icework", "iceworks"), false, true, false, shuffleAfterwards: true);
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

            //{Arctis} deals himself 1 cold damage.
            coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Cold, cardSource: GetCardSource());
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

