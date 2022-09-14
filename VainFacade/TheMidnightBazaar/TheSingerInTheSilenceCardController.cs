using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public class TheSingerInTheSilenceCardController : ThreenCardController
    {
        public TheSingerInTheSilenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Threens in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(ThreenCards);
            // Show list of Threens in trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, ThreenCards);
            // Show list of Unbindings in trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, UnbindingCards);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by Threens by the number of Threens in play."
            Func<DealDamageAction, int> amountToIncrease = (DealDamageAction dda) => FindCardsWhere((Card c) => IsThreen(c) && c.IsInPlayAndHasGameText).Count();
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && IsThreen(dda.DamageSource.Card), amountToIncrease);
            // "At the end of the environment turn, this card deals each non-Threen target 1 sonic damage, then plays a Threen and an Unbinding from the environment trash."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, SingResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard });
            // ...
        }

        private IEnumerator SingResponse(PhaseChangeAction pca)
        {
            // "... this card deals each non-Threen target 1 sonic damage..."
            IEnumerator damageCoroutine = DealDamage(base.Card, (Card c) => !IsThreen(c), 1, DamageType.Sonic);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "... then plays a Threen and an Unbinding from the environment trash."
            IEnumerator playThreenCoroutine = PlayCardsFromLocation(FindEnvironment().TurnTaker.Trash, ThreenCards, isPutIntoPlay: false, numberOfCards: 1, useFixedList: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playThreenCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playThreenCoroutine);
            }
            IEnumerator playUnbindingCoroutine = PlayCardsFromLocation(FindEnvironment().TurnTaker.Trash, UnbindingCards, isPutIntoPlay: false, numberOfCards: 1, useFixedList: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playUnbindingCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playUnbindingCoroutine);
            }
            yield break;
        }
    }
}
