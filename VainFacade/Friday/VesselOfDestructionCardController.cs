using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class VesselOfDestructionCardController:CardController
	{
		public VesselOfDestructionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When this card is destroyed, {Friday} deals 1 target 1 irreducible fire damage or destroys 1 environment or ongoing card.
            AddWhenDestroyedTrigger((DestroyCardAction dc) => DealDamageAndDestroyCard(1,1), new TriggerType[2] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Select a hero. Destroy any number of their ongoing or equipment cards. For each card destroyed this way, {Friday} deals 1 target 1 irreducible fire damage or destroys an ongoing or environment card. Destroy this card.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 1);
            SelectTurnTakerDecision decision = new SelectTurnTakerDecision(base.GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && FindCardsWhere((Card c) => (IsOngoing(c) || IsEquipment(c)) && c.Owner == tt && c.IsInPlayAndHasGameText).Any()), SelectionType.DestroyCard, true, cardSource:GetCardSource());
            IEnumerator coroutine = base.GameController.SelectTurnTakerAndDoAction(decision, (TurnTaker tt) => DestroyHerosCards(tt, num1, num2));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

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

        private IEnumerator DestroyHerosCards(TurnTaker tt, int num1, int num2)
        {
            SelectCardsDecision decision = new SelectCardsDecision(base.GameController, DecisionMaker, (Card c) => c.Owner == tt && c.IsInPlayAndHasGameText && (IsOngoing(c) || IsEquipment(c)), SelectionType.DestroyCard, null, false, 0, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardsAndDoAction(decision, (SelectCardDecision d) => base.GameController.DestroyCard(DecisionMaker, d.SelectedCard, false, postDestroyAction: () => DealDamageAndDestroyCard(num1, num2), cardSource:GetCardSource()), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DealDamageAndDestroyCard(int num1, int num2)
        {
            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Deal " + num1 + (num1 == 1?" target ":" targets ") + num2 + " irreducible fire damage", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController,this.CharacterCard), (Card c) => num2, DamageType.Fire, () => num1, false, num1, true, cardSource:GetCardSource())),
                new Function(base.HeroTurnTakerController, "Destroy an ongoing or environment card", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) || c.IsEnvironment,"ongoing or environment"), false, cardSource:GetCardSource()))
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
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

