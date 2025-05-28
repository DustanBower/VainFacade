using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class ByTheThroatCardController : BaronessBaseCardController
    {
        public ByTheThroatCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to an active hero."
            yield return SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsActive, "active hero character"), storedResults, isPutIntoPlay, decisionSources);
        }

        public override void AddTriggers()
        {
            //Log.Debug("ByTheThroatCardController.AddTriggers: ")
            base.AddTriggers();
            // "When that target leaves play, destroy this card."
            AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
            // "Increase damage dealt to that hero by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => IsThisCardNextToCard(dda.Target), 1);
            // "At the start of that hero's turn, {TheBaroness} deals them 2 melee damage and regains 2 HP."
            AddStartOfTurnTrigger((TurnTaker tt) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner == tt, DrinkResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.GainHP });
            // "When {TheBaroness} is dealt {H} or more damage from a single source, destroy this card."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.CharacterCard && dda.Target.Owner.IsVillain && dda.Amount >= H, DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
        }

        private IEnumerator DrinkResponse(PhaseChangeAction pca)
        {
            // "... {TheBaroness} deals them 2 melee damage..."
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, GetCardThisCardIsNextTo(), 2, DamageType.Melee, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "... and regains 2 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }
    }
}
