﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class VampirismCardController : BaronessUtilityCardController
    {
        public VampirismCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.ActivatesEffects);
            // If in play: show number of Blood cards each hero has
            ShowResonancePerHero();
        }

        protected const string GivenBlood = "HasGivenBloodThisTurn";

        private string BloodHealKey = "BloodHealKey";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {TheBaroness} deals each hero target melee damage, put the top card of that target's deck face-down in the villain play area."
            AddTrigger((DealDamageAction dda) => dda.WasHeroTarget && !IsPropertyTrue(GeneratePerTargetKey(GivenBlood, dda.Target)) && dda.DidDealDamage && dda.DamageType == DamageType.Melee && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.IsSameCard(base.CharacterCard), FirstDrinkResponse, TriggerType.MoveCard, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagsAfterLeavesPlay(GivenBlood), TriggerType.Hidden);
            // "When a card becomes Blood, {TheBaroness} regains 1 HP."
            AddTrigger((MoveCardAction mca) => !IsPropertyTrue(GeneratePerTargetKey(BloodHealKey,mca.CardToMove)) && mca.CardToMove.IsFaceDownNonCharacter && mca.CardToMove.IsHero && mca.WasCardMoved && mca.Destination.IsPlayAreaOf(base.TurnTaker) && !mca.Origin.IsPlayAreaOf(base.TurnTaker), (MoveCardAction mca) => HealResponse(mca.CardToMove), TriggerType.GainHP, TriggerTiming.After);
            AddTrigger((FlipCardAction fca) => !IsPropertyTrue(GeneratePerTargetKey(BloodHealKey, fca.CardToFlip.Card)) && fca.ToFaceDown && fca.CardToFlip.Card.Location.HighestRecursiveLocation == this.TurnTaker.PlayArea, (FlipCardAction fca) => HealResponse(fca.CardToFlip.Card), TriggerType.GainHP, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagsAfterLeavesPlay(BloodHealKey), TriggerType.Hidden);
        }

        private IEnumerator FirstDrinkResponse(DealDamageAction dda)
        {
            // "... put the top card of that target's deck face-down in the villain play area."
            SetCardPropertyToTrueIfRealAction(GeneratePerTargetKey(GivenBlood, dda.Target));
            IEnumerator moveCoroutine = MoveFaceDownToVillainPlayArea(GetNativeDeck(dda.Target).TopCard);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        public override bool? AskIfActivatesEffect(TurnTakerController turnTakerController, string effectKey)
        {
            // "Face-down cards in the villain play area are Blood. A hero's resonance equals the number of Blood cards from that hero's deck."
            // Effects that refer to Blood cards or resonance will require VampirismKey to activate
            bool? result = null;
            if (turnTakerController == base.TurnTakerController && effectKey == VampirismKey)
            {
                result = true;
            }
            return result;
        }

        private IEnumerator HealResponse(Card blood)
        {
            SetCardProperty(GeneratePerTargetKey(BloodHealKey, blood), true);
            IEnumerator coroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
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
