using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacade.TheMidnightBazaar
{
    public class MrWolfCardController : TheMidnightBazaarUtilityCardController
    {
        public MrWolfCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether The Blinded Queen is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(BlindedQueenIdentifier);
            // Show target with lowest HP
            SpecialStringMaker.ShowLowestHP(1, () => 1);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card would deal damage, if [i]The Blinded Queen[/i] is in play, play the top card of the villain deck instead."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card == base.Card && IsBlindedQueenInPlay(), PlayVillainCardInsteadResponse, new TriggerType[]{ TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.PlayCard}, TriggerTiming.Before);
            // "At the end of the environment turn, this card deals the target with the lowest HP {H + 1} melee damage."
            AddDealDamageAtEndOfTurnTrigger(TurnTaker, base.Card, (Card c) => c.IsTarget, TargetType.LowestHP, H + 1, DamageType.Melee);
        }

        private IEnumerator PlayVillainCardInsteadResponse(DealDamageAction dda)
        {
            // "... play the top card of the villain deck instead."
            IEnumerator cancelCoroutine = CancelAction(dda);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckWithMessageResponse(dda);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }
    }
}
