using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class BystanderCardController:BastionCityCardController
	{
		public BystanderCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        private bool InDestroyResponse;


        public override void AddTriggers()
        {
            //When this card is dealt damage, one player discards a card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !InDestroyResponse && dd.Target == this.Card && dd.DidDealDamage, (DealDamageAction dd) => base.GameController.SelectHeroToDiscardCard(DecisionMaker, false, false, cardSource: GetCardSource()), TriggerType.DiscardCard, TriggerTiming.After);

            //When this card reduced to 0 or fewer hp, each hero target deals itself 1 irreducible psychic damage. Then shuffle this card into the environment deck.
            //AddWhenDestroyedTrigger(DestroyedResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.ShuffleCardIntoDeck, TriggerType.ChangePostDestroyDestination }, (DestroyCardAction dc) => dc.DealDamageAction != null && this.Card.HitPoints <= 0);
            AddBeforeDestroyAction((GameAction ga) => DestroyedResponse(null));
        }

        private IEnumerator DestroyedResponse(DestroyCardAction dc = null)
        {
            if (this.Card.HitPoints <= 0)
            {
                //Console.WriteLine("Bystander is reacting to being destroyed");
                InDestroyResponse = true;
                IEnumerator coroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => IsHeroTarget(c), 1, DamageType.Psychic, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                //After the card is finished getting destroyed, shuffle the environment deck.
                //Shuffling this card into the environment deck in this routine does not work, since the game then moves this card to its final destroy destination
                AddAfterDestroyedAction(AfterDestroyResponse);
            }
        }

        public override MoveCardDestination GetTrashDestination()
        {
            if (this.Card.HitPoints <= 0)
            {
                return new MoveCardDestination(this.TurnTaker.Deck);
            }
            return base.GetTrashDestination();
        }

        private IEnumerator AfterDestroyResponse(GameAction action)
        {
            InDestroyResponse = false;
            return base.GameController.ShuffleLocation(this.TurnTaker.Deck, null, GetCardSource());
        }
    }
}

