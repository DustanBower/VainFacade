using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class FrozenHelmCardController:IceworkCardController
	{
		public FrozenHelmCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}


        public override IEnumerator Play()
        {
            //When this card enters play, destroy all other copies of this card
            IEnumerator coroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.Identifier == this.Card.Identifier && c != this.Card), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();

            //When {Arctis} would be dealt damage by a target other than {Arctis}, reduce that damage by 1.
            AddReduceDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageSource.IsTarget && dd.DamageSource.Card != this.CharacterCard, (DealDamageAction dd) => 1);
        }
    }
}

