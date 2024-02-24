using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class InginiCardController:SideCharacterCardController
	{
		public InginiCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
		{
            base.SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //The first time each turn this card is dealt damage, 1 hero may use a power.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && dd.Target == this.Card && dd.DidDealDamage, DamageResponse, TriggerType.UsePower, TriggerTiming.After);

            //At the end of the villain turn, Ingini deals the hero target with the highest HP H - 3 irreducible infernal damage.
            //Increase damage dealt by Ingini by 1 until this card leaves play.
            AddDealDamageAtEndOfTurnTrigger(this.TurnTaker, this.Card, (Card c) => IsHeroTarget(c), TargetType.HighestHP, base.H - 3, DamageType.Infernal, true);
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, TriggerType.CreateStatusEffect);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            IEnumerator coroutine = base.GameController.SelectHeroToUsePower(DecisionMaker, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
            effect.UntilTargetLeavesPlay(this.Card);
            effect.SourceCriteria.IsSpecificCard = this.Card;
            IEnumerator coroutine = AddStatusEffect(effect);
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

