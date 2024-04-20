using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
	public class SuspiciousBodyDoubleCardController:CardController
	{
		public SuspiciousBodyDoubleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If this card is in play, make sure the player can tell what play area it's in
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.HighestRecursiveLocation.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override IEnumerator Play()
        {
            //When this card enters play, you may destroy a target with 3 or fewer HP.
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            Location originalLocation = null;
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker,SelectionType.DestroyCard, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints <= 3 && c.IsInPlayAndHasGameText, "", false, false, "target with 3 or fewer HP", "targets with 3 or fewer HP"), results, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectCard(results))
            {
                Card selected = GetSelectedCard(results);
                if (selected != null)
                {
                    originalLocation = selected.Location.HighestRecursiveLocation;
                    coroutine = base.GameController.DestroyCard(DecisionMaker, selected, false, destroyResults, responsibleCard: this.Card, cardSource: GetCardSource());
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

            //If a target is destroyed this way, put this card in that card's play area, otherwise destroy this card.
            if (DidDestroyCard(destroyResults) && originalLocation != null)
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, originalLocation,playCardIfMovingToPlayArea: false, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            else
            {
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

        public override void AddTriggers()
        {
            //At the end of that play area's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage, then destroy this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.Card.Location.OwnerTurnTaker, GasResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        private IEnumerator GasResponse(PhaseChangeAction pca)
        {
            List<Card> storedTargets = new List<Card>();
            DealDamageAction damageAction = new DealDamageAction(GetCardSource(), null, null, 2, DamageType.Toxic);
            IEnumerator coroutine = base.GameController.FindTargetsWithHighestHitPoints(1, 3, (Card c) => c.Location.HighestRecursiveLocation == this.Card.Location.HighestRecursiveLocation, storedTargets, damageAction, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => storedTargets.Contains(c), 2, DamageType.Toxic, cardSource: GetCardSource());
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

