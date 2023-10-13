using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class FrostLanceCardController:IceworkWithAbilityCardController
	{
		public FrostLanceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator ActivateIce()
        {
            //Discard any number of cards. {Arctis} deals 1 target X plus 2 melee damage, where X = the number of cards discarded this way. Destroy this card.
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDiscardCards(DecisionMaker, null, false, 0, results, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            int amount = GetNumberOfCardsDiscarded(results) + 2;
            coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), amount, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
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

