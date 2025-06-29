﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public class ThePoetOfThePaleCardController : ThreenCardController
    {
        public ThePoetOfThePaleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show the non-Threen target with the lowest HP
            SpecialStringMaker.ShowLowestHP(1, cardCriteria: new LinqCardCriteria((Card c) => !IsThreen(c), "non-Threen"));
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "Threens are indestructible if any Threen has 1 or more HP."
            if (FindCardsWhere(new LinqCardCriteria((Card c) => IsThreen(c) && c.IsInPlayAndHasGameText && c.IsTarget && c.HitPoints >= 1 && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()))).Any())
            {
                return IsThreen(card) && base.GameController.IsCardVisibleToCardSource(card, GetCardSource());
            }
            return false;
        }

        protected const string IsCleaningUp = "IsCleaningUp";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals the non-Threen target with the lowest HP {H - 2} psychic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse,new TriggerType[] { TriggerType.DealDamage, TriggerType.DiscardCard });
            // When the last Threen with >0 HP leaves play, other Threen lose indestructibility
            AddAfterLeavesPlayAction((GameAction ga) => base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: true, cardSource: GetCardSource()), TriggerType.DestroyCard);
            AddTrigger((DestroyCardAction dca) => IsThreen(dca.CardToDestroy.Card), CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After);
            AddTrigger((BulkMoveCardsAction bmc) => bmc.CardsToMove.Any((Card c) => IsThreen(c)) && bmc.Destination.IsOutOfGame, CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: true);
            AddTrigger((SwitchBattleZoneAction sb) => sb.Origin == base.Card.BattleZone, CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: false, null, null, ignoreBattleZone: true);
            AddTrigger((MoveCardAction mc) => mc.Origin.BattleZone == base.BattleZone && mc.Destination.BattleZone != base.BattleZone, CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: false, null, null, ignoreBattleZone: true);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(IsCleaningUp), TriggerType.Hidden);
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator coroutine = DealDamageToLowestHP(this.Card, 1, (Card c) => !IsThreen(c), (Card c) => H - 2, DamageType.Psychic, storedResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If no damage is dealt this way, a player discards a card
            if (!DidDealDamage(results))
            {
                coroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, false, false, cardSource: GetCardSource());
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

        private IEnumerator CleanUpThreens(GameAction ga)
        {
            //Log.Debug("ThePoetOfThePaleCardController.CleanUpThreens(" + ga.GetHashCode().ToString() + ")");
            //Log.Debug("ThePoetOfThePaleCardController.CleanUpThreens(" + ga.GetHashCode().ToString() + ") started, ga = " + ga.ToString());
            // When the last Threen with >0 HP leaves play, other Threen lose indestructibility and need cleaning up
            if (Journal.GetCardPropertiesBoolean(base.Card, IsCleaningUp).HasValue && Journal.GetCardPropertiesBoolean(base.Card, IsCleaningUp).Value)
            {
                // We don't need to start this process again if it's already running
                //Log.Debug("ThePoetOfThePaleCardController.CleanUpThreens(" + ga.GetHashCode().ToString() + ") exited (already running)");
                yield break;
            }
            if (IsRealAction())
            {
                Journal.RecordCardProperties(base.Card, IsCleaningUp, true);
            }
            IEnumerator cleanupCoroutine = base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanupCoroutine);
            }
            //Log.Debug("ThePoetOfThePaleCardController.CleanUpThreens(" + ga.GetHashCode().ToString() + ") finished");
        }
    }
}
