using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class X517BoosterShotCardController:SerumCardController
	{
		public X517BoosterShotCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Damage dealt by {Peacekeeper} to other targets is increased by 2 and irreducible.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.Target != this.CharacterCard, 2);
            AddMakeDamageIrreducibleTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.Target != this.CharacterCard);

            //Reduce damage dealt to {Peacekeeper} by 1.
            AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 1);

            //At the start of your turn, {Peacekeeper} deals himself 2 irreducible toxic damage. Then you may play a symptom. Then destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard, TriggerType.DestroySelf });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 2, DamageType.Toxic, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = SelectAndPlayCardFromHand(DecisionMaker, cardCriteria: SymptomCriteria());
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
    }
}

