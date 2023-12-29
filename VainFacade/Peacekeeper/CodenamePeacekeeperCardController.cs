using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class CodenamePeacekeeperCardController:PeacekeeperCardUtilities
	{
		public CodenamePeacekeeperCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            List<DealDamageAction> results = new List<DealDamageAction>();
            int X = FindCardsWhere((Card c) => IsSymptom(c) && c.IsInPlayAndHasGameText).Count();
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), X, DamageType.Melee, 1, false, 1, storedResultsDamage: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If the damage was redirected, this should get the target that was actually dealt damage.
            //If the damage was prevented or reduced, this should get the target that it was trying to hit.
            Card original = null;
            if (results.FirstOrDefault() != null)
            {
                original = results.FirstOrDefault().Target;
            }

            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.
            List<DealDamageAction> results2 = new List<DealDamageAction>();
            coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 2, DamageType.Toxic, storedResults: results2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDealDamage(results2, this.CharacterCard) && original != null)
            {
                coroutine = DealDamage(this.CharacterCard, original, 3, DamageType.Melee, cardSource: GetCardSource());
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
}

