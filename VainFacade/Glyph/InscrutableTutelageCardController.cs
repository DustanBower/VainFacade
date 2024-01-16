using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class InscrutableTutelageCardController:GlyphLimitedCard
	{
		public InscrutableTutelageCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsGlyphFaceDownCard, "your face-down cards", false));
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
        }

        private ITrigger _modifyDamageAmount;

        private DealDamageAction DealDamageAction
        {
            get;
            set;
        }

        private SelectFunctionDecision SelectFunctionDecision
        {
            get;
            set;
        }

        private Card Target
        {
            get;
            set;
        }

        private string FirstDamageKey = "FirstDamageKey";

        public override IEnumerator Play()
        {
            //When this card enters play, you may draw a card or play a relic.
            IEnumerable<Function> functionChoices = new Function[2]
            {
            new Function(base.HeroTurnTakerController, "Draw a card.", SelectionType.DrawCard, () => DrawCard(), CanDrawCards(this.HeroTurnTakerController)),
            new Function(base.HeroTurnTakerController, "Play a relic.", SelectionType.PlayCard,() => SelectAndPlayCardFromHand(DecisionMaker, cardCriteria: new LinqCardCriteria(IsRelic, "", false, false, "relic","relics")), this.HeroTurnTaker.Hand.Cards.Any(IsRelic))
            };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, $"{this.TurnTaker.Name} cannot draw cards and {(this.HeroTurnTaker.Hand.Cards.Any(IsRelic) ? "cannot play cards" : "has no relics in hand")}.", null, GetCardSource());
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

        public override void AddTriggers()
        {
            //The first time each turn damage would be dealt to any target in an area containing your face-down cards, you may increase or reduce that damage by 1.
            //The code for this part is almost entirely copied from Twist the Ether
            _modifyDamageAmount = AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && dd.Target.Location.HighestRecursiveLocation.IsPlayArea && NumberOfFaceDownCardsInPlayArea(dd.Target.Location.HighestRecursiveLocation) > 0, ModifyDamageAmountResponse, TriggerType.ModifyDamageAmount, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageKey), TriggerType.Hidden);
        }

        private IEnumerator IncreaseFunction()
        {
            IEnumerator coroutine = base.GameController.IncreaseDamage(DealDamageAction, 1, isNemesisEffect: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator ModifyDamageAmountResponse(DealDamageAction dealDamage)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            DealDamageAction = dealDamage;
            if (base.GameController.PretendMode || dealDamage.Target != Target)
            {
                IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Increase by 1", SelectionType.IncreaseDamage, IncreaseFunction),
                new Function(base.HeroTurnTakerController, "Reduce by 1", SelectionType.ReduceDamageTaken, ReduceFunction)
                };
                List<SelectFunctionDecision> selectFunction = new List<SelectFunctionDecision>();
                SelectFunctionDecision selectFunction2 = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, optional: true, dealDamage, null, null, GetCardSource());

                IEnumerator coroutine = base.GameController.SelectAndPerformFunction(selectFunction2, selectFunction);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (Enumerable.Count<SelectFunctionDecision>((IEnumerable<SelectFunctionDecision>)selectFunction) > 0)
                {
                    SelectFunctionDecision = Enumerable.FirstOrDefault<SelectFunctionDecision>((IEnumerable<SelectFunctionDecision>)selectFunction);
                }
                
                
                Target = dealDamage.Target;
            }
            else if (SelectFunctionDecision.SelectedFunction != null)
            {
                IEnumerator coroutine2 = SelectFunctionDecision.SelectedFunction.FunctionToExecute.Invoke();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (!base.GameController.PretendMode)
            {
                SelectFunctionDecision = null;
                DealDamageAction = null;
            }
        }

        private IEnumerator ReduceFunction()
        {
            IEnumerator coroutine = base.GameController.ReduceDamage(DealDamageAction, 1, _modifyDamageAmount, GetCardSource());
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

