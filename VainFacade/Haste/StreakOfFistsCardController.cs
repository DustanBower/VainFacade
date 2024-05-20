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
	public class StreakOfFistsCardController:HasteUtilityCardController
	{
		public StreakOfFistsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        public override IEnumerator Play()
        {
            //Remove any number of tokens from your speed pool.
            List<int?> results = new List<int?>();
            IEnumerator coroutine = RemoveAnyNumberOfSpeedTokens(results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }



            //{Haste} deals X targets 2 melee damage each, where X = 2 plus the number of tokens removed this way.
            int X = results.FirstOrDefault().HasValue ? results.FirstOrDefault().Value + 2 : 2;
            coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 2, DamageType.Melee, X, false, X, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may draw or play a card. If you draw a card this way, shuffle this card into your deck.
            List<DrawCardAction> drawResults = new List<DrawCardAction>();
            IEnumerable<Function> functionChoices = new Function[2]
                    {
                    new Function(base.HeroTurnTakerController, $"Draw a card", SelectionType.DrawCard, () => DrawCard(this.HeroTurnTaker, cardsDrawn: drawResults), CanDrawCards(this.HeroTurnTakerController)),
                    new Function(base.HeroTurnTakerController, $"Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(this.HeroTurnTakerController, false), CanPlayCardsFromHand(this.HeroTurnTakerController))
                    };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, $"{this.TurnTaker.Name} cannot draw nor play any cards, so {Card.Title} has no effect.", null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }

            if (DidDrawCards(drawResults))
            {
                coroutine = base.GameController.ShuffleCardIntoLocation(DecisionMaker, this.Card, this.TurnTaker.Deck, false, cardSource: GetCardSource());
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

        public override bool DoNotMoveOneShotToTrash
        {
            get
            {
                if (base.Card.Location == this.TurnTaker.Deck)
                {
                    return true;
                }
                return base.DoNotMoveOneShotToTrash;
            }
        }
    }
}

