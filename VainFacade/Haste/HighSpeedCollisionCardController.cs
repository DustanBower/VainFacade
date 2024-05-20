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
	public class HighSpeedCollisionCardController:HasteUtilityCardController
	{
		public HighSpeedCollisionCardController(Card card, TurnTakerController turnTakerController)
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

            //{Haste} deals 1 target X melee damage, where X = 2 plus the number of tokens removed this way.
            int num = results.FirstOrDefault().HasValue ? results.FirstOrDefault().Value + 2 : 2;
            coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
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

