using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class VerseForTheVanquishedCardController:DirgeCardController
	{
		public VerseForTheVanquishedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            base.AddTriggers();

            //The first time each turn that target is dealt damage, a player may discard a card to draw or play a card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => GetCardThisCardIsNextTo() != null && dd.Target == GetCardThisCardIsNextTo() && dd.DidDealDamage && !IsPropertyTrue(FirstDamageKey), DamageResponse, new TriggerType[2] { TriggerType.DiscardCard, TriggerType.PlayCard }, TriggerTiming.After); 
            ResetFlagAfterLeavesPlay(FirstDamageKey);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);

            SelectTurnTakerDecision decision = new SelectTurnTakerDecision(base.GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && tt.ToHero().HasCardsInHand), SelectionType.DiscardCard, true, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectTurnTakerAndDoAction(decision, DiscardToPlay);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DiscardToPlay(TurnTaker tt)
        {
            HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = SelectAndDiscardCards(httc, 1, false, 0, results, responsibleTurnTaker: tt);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(results))
            {
                IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(httc, "Draw a card", SelectionType.DrawCard, () => DrawCard(tt.ToHero()), CanDrawCards(httc), $"{tt.Name} cannot play a card, so they must draw a card"),
                new Function(httc, "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(httc,false), CanPlayCards(httc) && tt.ToHero().HasCardsInHand, $"{tt.Name} cannot draw cards, so they must play a card")
                };

                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, httc, functionChoices, false, null, $"{tt.Name} cannot play a card or draw a card", null, GetCardSource());
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
}

