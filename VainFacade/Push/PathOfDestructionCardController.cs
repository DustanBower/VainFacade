using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PathOfDestructionCardController:AnchorCardController
	{
		public PathOfDestructionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => (IsOngoing(c) || IsEquipment(c)) && c.Owner != this.TurnTaker, "ongoing or equipment"), null,null, " belonging to other heroes", false);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();

            //At the start of your turn, Push deals 3 targets 3 irreducible projectile damage each.
            //Then you may destroy a hero ongoing or equipment belonging to another player.
            //If no card is destroyed this way, Push deals each target other than Push 0 projectile damage.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[2] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            //...Push deals 3 targets 3 irreducible projectile damage each.
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 3, DamageType.Projectile, 3, false, 3, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Then you may destroy a hero ongoing or equipment belonging to another player.
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => (IsOngoing(c) || IsEquipment(c)) && c.IsHero && c.Owner.IsPlayer && c.Owner != this.TurnTaker, "", false, false, "ongoing or equipment card belonging to another player", "ongoing or equipment cards belonging to other players"), true, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If no card is destroyed this way, Push deals each target other than Push 0 projectile damage
            if (!DidDestroyCard(results))
            {
                coroutine = DealDamage(this.CharacterCard, (Card c) => c != this.CharacterCard, 0, DamageType.Projectile);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            //Then destroy this card.
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

