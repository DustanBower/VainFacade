using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class BloodBoundFateCardController:DoomsayerCardUtilities
	{
		public BloodBoundFateCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            //AllowFastCoroutinesDuringPretend = false;
		}

        public override IEnumerator Play()
        {
            //When this card enters play, a player may move a card from their trash to their hand.
            IEnumerator coroutine = base.GameController.SelectHeroToMoveCardFromTrash(DecisionMaker, (HeroTurnTakerController httc) => httc.HeroTurnTaker.Hand, true, cardSource: GetCardSource());
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
            //When a hero would deal damage to {Doomsayer}, they may first deal themselves that same damage.
            //When a hero would deal damage to {Doomsayer}, prevent that damage if that hero has not dealt themselves damage this turn.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageSource.IsHeroCharacterCard && dd.DamageSource.IsTarget && !dd.IsPretend && dd.Amount > 0, DamageResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
        }


        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            Card hero = dd.DamageSource.Card;
            IEnumerator coroutine = DealDamage(hero, hero, dd.Amount, dd.DamageType, dd.IsIrreducible, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!HasBeenDealtDamageThisTurn((DealDamageJournalEntry e) => e.SourceCard != null && e.SourceCard == hero && e.TargetCard == hero && e.Amount > 0))
            {
                coroutine = CancelAction(dd);
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

