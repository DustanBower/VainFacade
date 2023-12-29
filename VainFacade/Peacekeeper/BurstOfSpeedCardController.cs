using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class BurstOfSpeedCardController:PeacekeeperCardUtilities
	{
		public BurstOfSpeedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //You may draw 1 card...
            IEnumerator coroutine = DrawCard(optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //...and play 1 card.
            coroutine = SelectAndPlayCardFromHand(DecisionMaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //{Peacekeeper} deals himself 2 irreducible toxic damage.
            List<DealDamageAction> results = new List<DealDamageAction>();
            coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 2, DamageType.Toxic, true, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If { Peacekeeper} was dealt damage this way, you may play 1 card.
            if (DidDealDamage(results,this.CharacterCard))
            {
                coroutine = SelectAndPlayCardFromHand(DecisionMaker);
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

