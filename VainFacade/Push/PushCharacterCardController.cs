using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PushCharacterCardController:PushBaseCharacterCardController
	{
		public PushCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator UsePower(int index = 0)
        {
            int num = GetPowerNumeral(0, 2);

            //You may discard a card or destroy an alteration.
            IEnumerable<Function> functionChoices = new Function[2]
            {
            new Function(base.HeroTurnTakerController, "Discard a card", SelectionType.DiscardCard, () => SelectAndDiscardCards(DecisionMaker, 1, false, 0), this.HeroTurnTaker.Hand.Cards.Count() > 0),
            new Function(base.HeroTurnTakerController, "Destroy an alteration", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsAlteration(c),"",false,false,"alteration","alterations"),true,cardSource:GetCardSource()),FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsAlteration(c)).Count() > 0)
            };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }

            //Draw up to 2 cards.
            IEnumerator coroutine = DrawCards(DecisionMaker, num, false, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private bool IsAlteration(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "alteration");
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    //One player may discard a card. If a card is discarded this way, one of that player's heroes may deal one target 2 projectile damage.
                    List<SelectTurnTakerDecision> turnTakerResults = new List<SelectTurnTakerDecision>();
                    List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
                    coroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, storedResultsTurnTaker: turnTakerResults, storedResultsDiscard: discardResults, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidDiscardCards(discardResults))
                    {
                        TurnTaker hero = GetSelectedTurnTaker(turnTakerResults);
                        HeroTurnTakerController httc = FindHeroTurnTakerController(hero.ToHero());
                        List<Card> heroResults = new List<Card>();
                        coroutine = FindCharacterCard(hero, SelectionType.HeroToDealDamage, heroResults, damageInfo: new DealDamageAction[] { new DealDamageAction(GetCardSource(), null, null, 2, DamageType.Projectile) });
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        Card selectedHero = heroResults.FirstOrDefault();
                        if (selectedHero != null)
                        {
                            coroutine = base.GameController.SelectTargetsAndDealDamage(httc, new DamageSource(base.GameController, selectedHero), 2, DamageType.Projectile, 1, false, 0, cardSource: GetCardSource());
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
                case 1:
                    //1 player may draw a card.
                    coroutine = base.GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
                case 2:
                    //Destroy an environment card.
                    coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, cardSource: GetCardSource());
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

