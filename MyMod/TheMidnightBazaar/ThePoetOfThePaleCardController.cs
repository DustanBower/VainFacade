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
            if (FindCardsWhere(new LinqCardCriteria((Card c) => IsThreen(c) && c.IsInPlayAndHasGameText && c.IsTarget && c.HitPoints >= 1)).Any())
            {
                return IsThreen(card);
            }
            return false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals the non-Threen target with the lowest HP {H - 2} psychic damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => !IsThreen(c), TargetType.LowestHP, H - 2, DamageType.Psychic);
            // When the last Threen with >0 HP leaves play, other Threen lose indestructibility
            AddAfterLeavesPlayAction((GameAction g) => base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: false, GetCardSource()), TriggerType.DestroyCard);
            AddTrigger((DestroyCardAction dca) => IsThreen(dca.CardToDestroy.Card), (DestroyCardAction dca) => base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: true, GetCardSource()), TriggerType.DestroyCard, TriggerTiming.After);
            AddTrigger((BulkMoveCardsAction bmc) => bmc.CardsToMove.Any((Card c) => IsThreen(c)) && bmc.Destination.IsOutOfGame, (BulkMoveCardsAction bmc) => base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: true, GetCardSource()), TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: true);
            AddTrigger((SwitchBattleZoneAction sb) => sb.Origin == base.Card.BattleZone, (SwitchBattleZoneAction sb) => base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: true, GetCardSource()), TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: false, null, null, ignoreBattleZone: true);
            AddTrigger((MoveCardAction mc) => mc.Origin.BattleZone == base.BattleZone && mc.Destination.BattleZone != base.BattleZone, (MoveCardAction mc) => base.GameController.DestroyAnyCardsThatShouldBeDestroyed(ignoreBattleZone: true, GetCardSource()), TriggerType.DestroyCard, TriggerTiming.After, ActionDescription.Unspecified, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: false, null, null, ignoreBattleZone: true);
        }
    }
}
