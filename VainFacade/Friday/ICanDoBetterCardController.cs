using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace VainFacadePlaytest.Friday
{
	public class ICanDoBetterCardController:CardController
	{
		public ICanDoBetterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When a target deals {Friday} damage, {Friday} may deal the source of that damage X plus 2 damage of the same type, where X = the amount of damage dealt to {Friday}. If {Friday} deals damage this way, destroy this card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsTarget && dd.Target == this.CharacterCard && dd.DidDealDamage, DamageResponse, new TriggerType[2] { TriggerType.DealDamage, TriggerType.DestroySelf }, TriggerTiming.After);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator coroutine = DealDamage(this.CharacterCard, dd.DamageSource.Card, dd.Amount + 2, dd.DamageType, false, true, true, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDealDamage(results))
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

        public override IEnumerator UsePower(int index = 0)
        {
            //Play a mimicry or draw 2 cards.
            int num = GetPowerNumeral(0, 2);
            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Play a mimicry", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(DecisionMaker, false, cardCriteria: new LinqCardCriteria((Card c) => base.GameController.DoesCardContainKeyword(c,"mimicry"),"",false,false,"mimicry","mimicries")),this.HeroTurnTaker.Hand.Cards.Any((Card c) => base.GameController.DoesCardContainKeyword(c,"mimicry")) && base.GameController.CanPlayCards(DecisionMaker,GetCardSource()), $"{this.TurnTaker.Name} cannot draw cards"),
                new Function(base.HeroTurnTakerController, "Draw " + num + " cards", SelectionType.DrawCard, () => DrawCards(DecisionMaker, num),base.GameController.CanDrawCards(DecisionMaker, GetCardSource()),(base.GameController.CanPlayCards(DecisionMaker,GetCardSource()))?($"{this.TurnTaker.Name} has no mimicries in their hand"):($"{this.TurnTaker.Name} cannot play cards"))
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, (base.GameController.CanPlayCards(DecisionMaker, GetCardSource())) ? ($"{this.TurnTaker.Name} has no mimicries in their hand and cannot play cards") : ($"{this.TurnTaker.Name} cannot play or draw cards"), null, GetCardSource());
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
    }
}

