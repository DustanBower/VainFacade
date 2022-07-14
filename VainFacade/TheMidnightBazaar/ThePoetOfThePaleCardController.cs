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
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => !IsThreen(c), TargetType.LowestHP, H - 2, DamageType.Psychic);
            // When the last Threen with >0 HP leaves play, other Threen lose indestructibility
            AddAfterLeavesPlayAction(CleanUpThreens, TriggerType.DestroyCard);
            AddTrigger((DestroyCardAction dca) => IsThreen(dca.CardToDestroy.Card), CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After);
            AddTrigger((BulkMoveCardsAction bmc) => bmc.CardsToMove.Any((Card c) => IsThreen(c)) && bmc.Destination.IsOutOfGame, CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: true);
            AddTrigger((SwitchBattleZoneAction sb) => sb.Origin == base.Card.BattleZone, CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: false, null, null, ignoreBattleZone: true);
            AddTrigger((MoveCardAction mc) => mc.Origin.BattleZone == base.BattleZone && mc.Destination.BattleZone != base.BattleZone, CleanUpThreens, TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: false, null, null, ignoreBattleZone: true);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(IsCleaningUp), TriggerType.Hidden);
        }

        private IEnumerator CleanUpThreens(GameAction ga)
        {
            // When the last Threen with >0 HP leaves play, other Threen lose indestructibility and need cleaning up
            if (Journal.GetCardPropertiesBoolean(base.Card, IsCleaningUp).HasValue && Journal.GetCardPropertiesBoolean(base.Card, IsCleaningUp).Value)
            {
                // We don't need to start this process again if it's already running
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
        }
    }
}
