using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class ShardStormCardController:ArctisCardUtilities
	{
		public ShardStormCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => IsIcework(c), "", false, false, "icework", "iceworks"));
		}

        public override IEnumerator Play()
        {
            //You may destroy any number of iceworks.
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => IsIcework(c), "", false, false, "icework", "icework"), null, false, 0, storedResultsAction: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //{Arctis} deals 1 target X plus 1 projectile damage or X targets 2 projectile damage each, where X = the number of cards destroyed this way.
            int amount = GetNumberOfCardsDestroyed(results);
            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, $"Deal 1 target {amount + 1} projectile damage", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), amount + 1, DamageType.Projectile, 1, false,1, cardSource:GetCardSource())),
                new Function(base.HeroTurnTakerController, $"Deal {amount} {(amount == 1 ? "target" : "targets")} 2 projectile damage{(amount <= 1 ? "" : " each")}", SelectionType.DealDamage, () =>  base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 2, DamageType.Projectile, amount, false,(amount > 0 ? 1 : 0), cardSource:GetCardSource()))
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

