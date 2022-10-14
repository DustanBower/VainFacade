using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class AllottedTimeCardController : TheFuryUtilityCardController
    {
        public AllottedTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        private Card ProtectedCard { get; set; }
        private Guid ReactedDamage { get; set; }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a target would be dealt damage, you may make that target indestructible this turn."
            AddTrigger((DealDamageAction dda) => !dda.IsPretend && dda.IsSuccessful, MayProtectResponse, TriggerType.CreateStatusEffect, TriggerTiming.Before);
            // "Resolve that damage normally, then that target regains X HP and destroys this card, where X = 1 plus the damage dealt this way."
            AddTrigger((DealDamageAction dda) => !dda.IsPretend && dda.InstanceIdentifier == ReactedDamage, HealDestructResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DestroySelf }, TriggerTiming.After);
        }

        private IEnumerator MayProtectResponse(DealDamageAction dda)
        {
            // "... you may make that target indestructible this turn."
            YesNoCardDecision choice = new YesNoCardDecision(base.GameController, DecisionMaker, SelectionType.Custom, dda.Target, dda, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choice))
            {
                ReactedDamage = dda.InstanceIdentifier;
                ProtectedCard = dda.Target;

                IEnumerator protectCoroutine = MakeIndestructibleThisTurn(dda.Target, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(protectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(protectCoroutine);
                }
            }
        }

        private IEnumerator HealDestructResponse(DealDamageAction dda)
        {
            // "... then that target regains X HP and destroys this card, where X = 1 plus the damage dealt this way."
            if (dda.DidDealDamage)
            {
                int x = 1 + dda.Amount;
                IEnumerator healCoroutine = base.GameController.GainHP(ProtectedCard, x, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
            }
            else
            {
                string message = "No damage was dealt, so " + ProtectedCard.Title + " does not regain HP.";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.High, GetCardSource(), ProtectedCard.ToEnumerable(), true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
