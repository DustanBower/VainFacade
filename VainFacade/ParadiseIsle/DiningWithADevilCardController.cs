using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class DiningWithADevilCardController : CardController
    {
        public DiningWithADevilCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt to non-environment targets by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target.IsNonEnvironmentTarget, 1);
            // "Increase HP recovery by 2."
            AddTrigger((GainHPAction gha) => true, (GainHPAction gha) => base.GameController.IncreaseHPGain(gha, 2, GetCardSource()), new TriggerType[] { TriggerType.IncreaseHPGain, TriggerType.ModifyHPGain }, TriggerTiming.Before);
            // "At the start of the environment turn, play the top card of the environment deck and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayDestructResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroySelf });
        }

        private IEnumerator PlayDestructResponse(PhaseChangeAction pca)
        {
            // "... play the top card of the environment deck..."
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(pca);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... and destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(pca);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            yield break;
        }
    }
}
