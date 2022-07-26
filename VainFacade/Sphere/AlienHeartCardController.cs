using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class AlienHeartCardController : SphereUtilityCardController
    {
        public AlienHeartCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.ActivatesEffects);
            // Show whether Sphere has been prevented from playing a card this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(PlayPrevented), () => base.TurnTaker.Name + " has already been prevented from playing a card this turn.", () => base.TurnTaker.Name + " has not been prevented from playing a card this turn.");
            // Show whether Sphere has been prevented from drawing a card this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(DrawPrevented), () => base.TurnTaker.Name + " has already been prevented from drawing a card this turn.", () => base.TurnTaker.Name + " has not been prevented from drawing a card this turn.");
            // Show whether Sphere has been prevented from using a power this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(PowerPrevented), () => base.TurnTaker.Name + " has already been prevented from using a power this turn.", () => base.TurnTaker.Name + " has not been prevented from using a power this turn.");
        }

        public override bool? AskIfActivatesEffect(TurnTakerController turnTakerController, string effectKey)
        {
            if (turnTakerController == base.TurnTakerController && effectKey == HeartKey)
            {
                return true;
            }
            return base.AskIfActivatesEffect(turnTakerController, effectKey);
        }

        protected const string PlayPrevented = "PlayPrevented";
        protected const string DrawPrevented = "DrawPrevented";
        protected const string PowerPrevented = "PowerPrevented";

        protected enum CustomMode
        {
            ToPlayCard,
            ToDrawCard,
            ToUsePower,
            UnlistedPlay,
            UnlistedPower,
            UnlistedDraw
        }

        protected enum AlternativeMode
        {
            PlayCard,
            PlayTopCard,
            PlayBottomCard,
            UsePower,
            UsePowerOnCard,
            HeroToUsePower
        }

        protected CustomMode currentMode;
        protected AlternativeMode currentAlt;
        protected SelectionType[] playing = new SelectionType[] { SelectionType.PlayCard, SelectionType.PlayTopCard, SelectionType.PlayBottomCard };
        protected SelectionType[] usingPower = new SelectionType[] { SelectionType.UsePower, SelectionType.UsePowerOnCard, SelectionType.HeroToUsePower };

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (currentMode is CustomMode.ToPlayCard)
            {
                return new CustomDecisionText("A player couldn't be selected to play a card. Would " + base.TurnTaker.Name + " have been selected?", "Choosing whether " + base.TurnTaker.Name + " was prevented from playing a card", "Vote for whether " + base.TurnTaker.Name + " was prevented from playing a card", "choose " + base.TurnTaker.Name + " to be prevented from playing a card");
            }
            else if (currentMode is CustomMode.ToDrawCard)
            {
                return new CustomDecisionText("A player couldn't be selected to draw a card. Would " + base.TurnTaker.Name + " have been selected?", "Choosing whether " + base.TurnTaker.Name + " was prevented from drawing a card", "Vote for whether " + base.TurnTaker.Name + " was prevented from drawing a card", "choose " + base.TurnTaker.Name + " to be prevented from drawing a card");
            }
            else if (currentMode is CustomMode.ToUsePower)
            {
                return new CustomDecisionText("A hero couldn't be selected to use a power. Would " + base.CharacterCard.Title + " have been selected?", "Choosing whether " + base.CharacterCard.Title + " was prevented from using a power", "Vote for whether " + base.CharacterCard.Title + " was prevented from using a power", "choose " + base.CharacterCard.Title + " to be prevented from using a power");
            }
            else if (currentMode is CustomMode.UnlistedPlay)
            {
                string gerund = "playing a card";
                string infinitive = "to play a card";
                if (currentAlt is AlternativeMode.PlayTopCard)
                {
                    gerund = "playing the top card of their deck";
                    infinitive = "to play the top card of their deck";
                }
                else if (currentAlt is AlternativeMode.PlayBottomCard)
                {
                    gerund = "playing the bottom card of their deck";
                    infinitive = "to play the bottom card of their deck";
                }
                return new CustomDecisionText(base.TurnTaker.Name + " can't be selected " + infinitive + ". Should " + base.CharacterCard.Title + " use a power instead? This will replace anyone else " + gerund + ".", "Choosing whether to let " + base.CharacterCard.Title + " use a power instead of someone else " + gerund, "Vote for whether to let " + base.CharacterCard.Title + " use a power instead of someone else " + gerund, "choose " + base.CharacterCard.Title + " to use a power instead of someone else " + gerund);
            }
            else if (currentMode is CustomMode.UnlistedDraw)
            {
                string gerund = "drawing a card";
                string infinitive = "to draw a card";
                return new CustomDecisionText(base.TurnTaker.Name + " can't be selected " + infinitive + ". Should " + base.TurnTaker.Name + " play a card instead? This will replace anyone else " + gerund + ".", "Choosing whether to let " + base.TurnTaker.Name + " play a card instead of someone else " + gerund, "Vote for whether to let " + base.TurnTaker.Name + " play a card instead of someone else " + gerund, "choose " + base.TurnTaker.Name + " to play a card instead of someone else " + gerund);
            }
            else if (currentMode is CustomMode.UnlistedPower)
            {
                string gerund = "using a power";
                string infinitive = "to use a power";
                return new CustomDecisionText(base.CharacterCard.Title + " can't be selected " + infinitive + ". Should " + base.TurnTaker.Name + " draw a card instead? This will replace anyone else " + gerund + ".", "Choosing whether to let " + base.TurnTaker.Name + " draw a card instead of someone else " + gerund, "Vote for whether to let " + base.TurnTaker.Name + " draw a card instead of someone else " + gerund, "choose " + base.TurnTaker.Name + " to draw a card instead of someone else " + gerund);
            }
            return base.GetCustomDecisionText(decision);
        }

        public static readonly string preventingHeroesPlaying = "preventing heroes from playing cards";
        public static readonly string noOtherHeroesCanPlay = "There are no other heroes that can play cards";
        public static readonly string noSelectableHeroesWithUsablePowers = "There are no selectable heroes with usable powers left";
        public static readonly string cannotCurrentlyUsePowers = "cannot currently use powers";
        public string preventsPlayingMoreCards()
        {
            return "prevents " + base.TurnTaker.Name + " from playing any more cards";
        }

        public string preventedPlayingCards()
        {
            return "prevented " + base.TurnTaker.Name + " from playing cards";
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            //AddTrigger((CancelAction ca) => true, LogCancelAction, TriggerType.Hidden, TriggerTiming.After);
            //AddTrigger((SetPhaseActionCountAction spca) => true, LogSetPhaseActionCountAction, TriggerType.Hidden, TriggerTiming.After);
            //AddTrigger((PhaseChangeAction pca) => pca.FromPhase.TurnTaker == base.TurnTaker || pca.ToPhase.TurnTaker == base.TurnTaker, LogPhaseChangeAction, TriggerType.Hidden, TriggerTiming.After);
            //AddTrigger((MessageAction ma) => true, LogMessageAction, TriggerType.Hidden, TriggerTiming.After);

            // "The first time each turn a non-hero card prevents you from playing a card, you may use a power."
            // PlayCardAction canceled? You may use a power
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(PlayPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is PlayCardAction && (ca.ActionToCancel as PlayCardAction).ResponsibleTurnTaker == base.TurnTaker, PreventedPlayResponse, TriggerType.UsePower, TriggerTiming.After);
            // MakeDecisionAction for which card to play canceled? You may use a power
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(PlayPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && (ca.ActionToCancel as MakeDecisionAction).Decision.DecisionMaker == base.HeroTurnTaker && (ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.PlayCard, PreventedPlayResponse, TriggerType.UsePower, TriggerTiming.After);
            // SetPhaseActionCountAction setting Sphere's play phase count to 0? You may use a power
            AddTrigger((SetPhaseActionCountAction spca) => !HasBeenSetToTrueThisTurn(PlayPrevented) && spca.CardSource != null && !spca.CardSource.Card.IsHero && spca.TurnPhase.TurnTaker == base.TurnTaker && spca.TurnPhase.Phase == Phase.PlayCard && (!spca.AmountToSet.HasValue || spca.AmountToSet.HasValueLessThan(1)), PreventedPlayResponse, TriggerType.UsePower, TriggerTiming.After);
            // PhaseChangeAction skipping Sphere's play phase due to a status effect from a non-hero card? You may use a power
            AddPhaseChangeTrigger((TurnTaker tt) => tt == base.TurnTaker, (Phase p) => !HasBeenSetToTrueThisTurn(PlayPrevented), PlayPhaseCriteria, PreventedPlayResponse, new TriggerType[] { TriggerType.UsePower }, TriggerTiming.After);
            AddPhaseChangeTrigger((TurnTaker tt) => true, (Phase p) => !HasBeenSetToTrueThisTurn(PlayPrevented), (PhaseChangeAction pca) => pca.ToPhase.WasSkipped && pca.ToPhase.Phase == Phase.PlayCard && pca.ToPhase.TurnTaker == base.TurnTaker, PreventedPlayResponse, new TriggerType[] {TriggerType.UsePower}, TriggerTiming.After);
            // MakeDecisionAction for which hero gets to play a card where Sphere was an option canceled? Ask if they would've chosen Sphere- if so, you may use a power
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(PlayPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && playing.Contains((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType) && ((ca.ActionToCancel as MakeDecisionAction).Decision is SelectTurnTakerDecision || (ca.ActionToCancel as MakeDecisionAction).Decision is SelectTurnTakersDecision), PreventedPlayChoiceResponse, TriggerType.UsePower, TriggerTiming.After);
            // A hero is being chosen to play a card and Sphere can't be chosen? Ask if they would've chosen Sphere- if so, you may use a power (and no one may play a card)
            AddTrigger((MakeDecisionAction mda) => !HasBeenSetToTrueThisTurn(PlayPrevented) && mda.Decision is SelectTurnTakerDecision && (mda.CardSource == null || base.GameController.IsCardVisibleToCardSource(base.CharacterCard, mda.CardSource)) && playing.Contains(mda.Decision.SelectionType) && (!CanPlayCards(base.TurnTakerController) || !CanPlayCardsFromHand(base.HeroTurnTakerController)), UnlistedPlayChoiceResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.UsePower }, TriggerTiming.Before);
            // SelectHeroToPlayCard sends a failure message instead of letting a hero be chosen to play a card? Ask if they would've chosen Sphere- if so, you may use a power
            AddTrigger((MessageAction ma) => !HasBeenSetToTrueThisTurn(PlayPrevented) && (ma.Message.Contains(preventingHeroesPlaying) || ma.Message.Contains(noOtherHeroesCanPlay)) && (!CanPlayCards(base.TurnTakerController) || !CanPlayCardsFromHand(base.HeroTurnTakerController)), PreventedPlayChoiceResponse, TriggerType.UsePower, TriggerTiming.After);
            // Impulsion Beam cuts off Sphere's play phase early or stops him chaining plays with Field Projection Stabilizer? You may use a power
            // SelectAndPlayCardsFromHand sends a failure message instead of letting Sphere play cards? You may use a power
            AddTrigger((MessageAction ma) => !HasBeenSetToTrueThisTurn(PlayPrevented) && (!CanPlayCards(base.TurnTakerController) || !CanPlayCardsFromHand(base.HeroTurnTakerController)) && (ma.Message.Contains(preventsPlayingMoreCards()) || ma.Message.Contains(preventedPlayingCards())), PreventedPlayResponse, TriggerType.UsePower, TriggerTiming.After);

            // "The first time each turn a non-hero card prevents you from drawing a card, you may play a card."
            // DrawCardAction canceled? You may play a card
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(DrawPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is DrawCardAction && (ca.ActionToCancel as DrawCardAction).HeroTurnTaker == base.HeroTurnTaker, PreventedDrawResponse, TriggerType.PlayCard, TriggerTiming.After);
            // SetPhaseActionCountAction setting Sphere's draw phase count to 0? You may play a card
            AddTrigger((SetPhaseActionCountAction spca) => !HasBeenSetToTrueThisTurn(DrawPrevented) && spca.CardSource != null && !spca.CardSource.Card.IsHero && spca.TurnPhase.TurnTaker == base.TurnTaker && spca.TurnPhase.Phase == Phase.DrawCard && (!spca.AmountToSet.HasValue || spca.AmountToSet.HasValueLessThan(1)), PreventedDrawResponse, TriggerType.PlayCard, TriggerTiming.After);
            // PhaseChangeAction skipping Sphere's draw phase due to a status effect from a non-hero card? You may play a card
            AddPhaseChangeTrigger((TurnTaker tt) => tt == base.TurnTaker, (Phase p) => !HasBeenSetToTrueThisTurn(DrawPrevented), DrawPhaseCriteria, PreventedDrawResponse, new TriggerType[] { TriggerType.PlayCard }, TriggerTiming.After);
            AddPhaseChangeTrigger((TurnTaker tt) => true, (Phase p) => !HasBeenSetToTrueThisTurn(DrawPrevented), (PhaseChangeAction pca) => pca.ToPhase.WasSkipped && pca.ToPhase.Phase == Phase.DrawCard && pca.ToPhase.TurnTaker == base.TurnTaker && (base.TurnTaker.Deck.HasCards || base.TurnTaker.Trash.HasCards), PreventedDrawResponse, new TriggerType[] { TriggerType.PlayCard }, TriggerTiming.After);
            // MakeDecisionAction for which hero gets to draw a card where Sphere was an option canceled? Ask if they would've chosen Sphere- if so, you may play a card
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(DrawPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && (ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.DrawCard && ((ca.ActionToCancel as MakeDecisionAction).Decision is SelectTurnTakerDecision || (ca.ActionToCancel as MakeDecisionAction).Decision is SelectTurnTakersDecision), PreventedDrawChoiceResponse, TriggerType.PlayCard, TriggerTiming.After);
            // A hero is being chosen to draw a card and Sphere can't draw cards? Ask if they would've chosen Sphere- if so, you may play a card (and no one may draw a card)
            AddTrigger((MakeDecisionAction mda) => !HasBeenSetToTrueThisTurn(DrawPrevented) && mda.Decision is SelectTurnTakerDecision && (mda.CardSource == null || base.GameController.IsCardVisibleToCardSource(base.CharacterCard, mda.CardSource)) && mda.Decision.SelectionType == SelectionType.DrawCard && !CanDrawCards(base.HeroTurnTakerController), UnlistedDrawChoiceResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.PlayCard }, TriggerTiming.Before);
            
            // "The first time each turn a non-hero card prevents you from using a power, you may draw a card."
            // UsePowerAction canceled? You may draw a card
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(PowerPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is UsePowerAction && (ca.ActionToCancel as UsePowerAction).HeroUsingPower == base.HeroTurnTakerController, PreventedPowerResponse, TriggerType.DrawCard, TriggerTiming.After);
            // MakeDecisionAction for which power to use canceled? You may draw a card
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(PowerPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && (ca.ActionToCancel as MakeDecisionAction).DecisionMaker == base.HeroTurnTakerController && (ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.UsePower, PreventedPowerResponse, TriggerType.DrawCard, TriggerTiming.After);
            // SetPhaseActionCountAction setting Sphere's power phase count to 0? You may draw a card
            AddTrigger((SetPhaseActionCountAction spca) => !HasBeenSetToTrueThisTurn(PowerPrevented) && spca.CardSource != null && !spca.CardSource.Card.IsHero && spca.TurnPhase.TurnTaker == base.TurnTaker && spca.TurnPhase.Phase == Phase.UsePower && (!spca.AmountToSet.HasValue || spca.AmountToSet.HasValueLessThan(1)), PreventedPowerResponse, TriggerType.DrawCard, TriggerTiming.After);
            // PhaseChangeAction skipping Sphere's power phase due to a status effect from a non-hero card? You may draw a card
            AddPhaseChangeTrigger((TurnTaker tt) => tt == base.TurnTaker, (Phase p) => !HasBeenSetToTrueThisTurn(PowerPrevented), PowerPhaseCriteria, PreventedPowerResponse, new TriggerType[] { TriggerType.DrawCard }, TriggerTiming.After);
            AddPhaseChangeTrigger((TurnTaker tt) => true, (Phase p) => !HasBeenSetToTrueThisTurn(PowerPrevented), (PhaseChangeAction pca) => pca.ToPhase.WasSkipped && pca.ToPhase.Phase == Phase.UsePower && pca.ToPhase.TurnTaker == base.TurnTaker && UnusedPowersThisTurn().Count() > 0 && !base.GameController.CanPerformAction<UsePowerAction>(base.TurnTakerController, GetCardSource()), PreventedPowerResponse, new TriggerType[] { TriggerType.DrawCard }, TriggerTiming.After);
            // MakeDecisionAction for which hero gets to use a power where Sphere was an option canceled? Ask if they would've chosen Sphere- if so, you may draw a card
            AddTrigger((CancelAction ca) => !HasBeenSetToTrueThisTurn(PowerPrevented) && ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && usingPower.Contains((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType) && ((ca.ActionToCancel as MakeDecisionAction).Decision is SelectTurnTakerDecision || (ca.ActionToCancel as MakeDecisionAction).Decision is SelectTurnTakersDecision), PreventedPowerChoiceResponse, TriggerType.DrawCard, TriggerTiming.After);
            // A hero is being chosen to use a power and Sphere can't use powers? Ask if they would've chosen Sphere- if so, you may draw a card (and no one may use a power)
            AddTrigger((MakeDecisionAction mda) => !HasBeenSetToTrueThisTurn(PowerPrevented) && mda.Decision is SelectTurnTakerDecision && (mda.CardSource == null || base.GameController.IsCardVisibleToCardSource(base.CharacterCard, mda.CardSource)) && usingPower.Contains(mda.Decision.SelectionType) && !base.GameController.CanPerformAction<UsePowerAction>(base.TurnTakerController, GetCardSource()), UnlistedPowerChoiceResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.DrawCard }, TriggerTiming.Before);
            // Something behind the scenes sends a failure message instead of letting Sphere use a power? You may draw a card
            // SelectAndUsePower sends a failure message instead of letting Sphere use a power? You may draw a card
            AddTrigger((MessageAction ma) => !HasBeenSetToTrueThisTurn(PowerPrevented) && (ma.Message.Contains(base.TurnTaker.Name) || ma.Message.Contains(base.CharacterCard.Title)) && ma.Message.Contains(cannotCurrentlyUsePowers) && !base.GameController.CanPerformAction<UsePowerAction>(base.TurnTakerController, GetCardSource()), PreventedPowerResponse, TriggerType.DrawCard, TriggerTiming.After);
            // SelectHeroToUsePower sends a failure message instead of letting a hero be chosen to use a power? Ask if they would've chosen Sphere- if so, you may draw a card
            AddTrigger((MessageAction ma) => !HasBeenSetToTrueThisTurn(PowerPrevented) && ma.Message.Contains(noSelectableHeroesWithUsablePowers) && !base.GameController.CanPerformAction<UsePowerAction>(base.TurnTakerController, GetCardSource()), PreventedPowerChoiceResponse, TriggerType.DrawCard, TriggerTiming.After);

            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(PlayPrevented));
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(DrawPrevented));
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(PowerPrevented));
        }

        private bool PlayPhaseCriteria(PhaseChangeAction pca)
        {
            TurnPhase nextTurnPhase = GameController.FindNextTurnPhase(pca.FromPhase);
            if (nextTurnPhase.Phase == Phase.PlayCard && nextTurnPhase.TurnTaker == base.TurnTaker)
            {
                if (GameController.StatusEffectControllers.Any(c => c.StatusEffect is PreventPhaseActionStatusEffect s && s.ToTurnPhaseCriteria.Phase == Phase.PlayCard && s.ToTurnPhaseCriteria.TurnTaker == base.TurnTaker && !s.CardSource.IsHero))
                {
                    return true;
                }
            }
            return false;
        }

        private bool PowerPhaseCriteria(PhaseChangeAction pca)
        {
            TurnPhase nextTurnPhase = GameController.FindNextTurnPhase(pca.FromPhase);
            if (nextTurnPhase.Phase == Phase.UsePower && nextTurnPhase.TurnTaker == base.TurnTaker)
            {
                if (GameController.StatusEffectControllers.Any(c => c.StatusEffect is PreventPhaseActionStatusEffect s && s.ToTurnPhaseCriteria.Phase == Phase.UsePower && s.ToTurnPhaseCriteria.TurnTaker == base.TurnTaker && !s.CardSource.IsHero))
                {
                    return true;
                }
            }
            return false;
        }

        private bool DrawPhaseCriteria(PhaseChangeAction pca)
        {
            TurnPhase nextTurnPhase = GameController.FindNextTurnPhase(pca.FromPhase);
            if (nextTurnPhase.Phase == Phase.DrawCard && nextTurnPhase.TurnTaker == base.TurnTaker)
            {
                if (GameController.StatusEffectControllers.Any(c => c.StatusEffect is PreventPhaseActionStatusEffect s && s.ToTurnPhaseCriteria.Phase == Phase.DrawCard && s.ToTurnPhaseCriteria.TurnTaker == base.TurnTaker && !s.CardSource.IsHero))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator LogMessageAction(MessageAction ma)
        {
            Log.Debug("AlienHeartCardController.LogMessageAction activated: ma: " + ma.ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: ma.Message: {" + ma.Message + "}");
            Log.Debug("AlienHeartCardController.LogMessageAction: HasBeenSetToTrueThisTurn(PlayPrevented): " + HasBeenSetToTrueThisTurn(PlayPrevented).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: HasBeenSetToTrueThisTurn(PowerPrevented): " + HasBeenSetToTrueThisTurn(PowerPrevented).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: HasBeenSetToTrueThisTurn(DrawPrevented): " + HasBeenSetToTrueThisTurn(DrawPrevented).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: CanPlayCards: " + base.GameController.CanPlayCards(base.TurnTakerController, GetCardSource()).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: CanPlayCardsFromHand: " + CanPlayCardsFromHand(base.HeroTurnTakerController).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: CanPerformAction<UsePowerAction>: " + base.GameController.CanPerformAction<UsePowerAction>(base.TurnTakerController, GetCardSource()).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: ma.Message.Contains(" + preventingHeroesPlaying + "): " + ma.Message.Contains(preventingHeroesPlaying).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: ma.Message.Contains(" + noOtherHeroesCanPlay + "): " + ma.Message.Contains(noOtherHeroesCanPlay).ToString());
            Log.Debug("AlienHeartCardController.LogMessageAction: ma.Message.Contains(" + noSelectableHeroesWithUsablePowers + "): " + ma.Message.Contains(noSelectableHeroesWithUsablePowers).ToString());

            Log.Debug("AlienHeartCardController.LogMessageAction: " + ma.ToString());
            yield break;
        }

        private IEnumerator LogCancelAction(CancelAction ca)
        {
            Log.Debug("AlienHeartCardController.LogCancelAction activated: ca: " + ca.ToString());
            Log.Debug("AlienHeartCardController.LogCancelAction: HasBeenSetToTrueThisTurn(PlayPrevented): " + HasBeenSetToTrueThisTurn(PlayPrevented).ToString());
            Log.Debug("AlienHeartCardController.LogCancelAction: HasBeenSetToTrueThisTurn(PowerPrevented): " + HasBeenSetToTrueThisTurn(PowerPrevented).ToString());
            Log.Debug("AlienHeartCardController.LogCancelAction: HasBeenSetToTrueThisTurn(DrawPrevented): " + HasBeenSetToTrueThisTurn(DrawPrevented).ToString());
            if (ca.CardSource != null)
            {
                Log.Debug("AlienHeartCardController.LogCancelAction: ca.CardSource: " + ca.CardSource.ToString());
                Log.Debug("AlienHeartCardController.LogCancelAction: ca.CardSource.Card.IsHero: " + ca.CardSource.Card.IsHero.ToString());
            }
            else
            {
                Log.Debug("AlienHeartCardController.LogCancelAction: ca.CardSource == null");
            }
            if (ca.ActionToCancel != null)
            {
                Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel: " + ca.ActionToCancel.ToString());
                Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.GetType: " + ca.ActionToCancel.GetType().ToString());
                if (ca.ActionToCancel is PlayCardAction)
                {
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel is PlayCardAction");
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.ResponsibleTurnTaker.Name: " + (ca.ActionToCancel as PlayCardAction).ResponsibleTurnTaker.Name);
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.ResponsibleTurnTaker == base.TurnTaker: " + ((ca.ActionToCancel as PlayCardAction).ResponsibleTurnTaker == base.TurnTaker).ToString());
                }
                else if (ca.ActionToCancel is UsePowerAction)
                {
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel is UsePowerAction");
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.HeroUsingPower.Name: " + (ca.ActionToCancel as UsePowerAction).HeroUsingPower.Name);
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.HeroUsingPower == base.HeroTurnTakerController: " + ((ca.ActionToCancel as UsePowerAction).HeroUsingPower == base.HeroTurnTakerController).ToString());
                }
                else if (ca.ActionToCancel is DrawCardAction)
                {
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel is DrawCardAction");
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.HeroTurnTaker: " + (ca.ActionToCancel as DrawCardAction).HeroTurnTaker.Name);
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.HeroTurnTaker == base.HeroTurnTaker: " + ((ca.ActionToCancel as DrawCardAction).HeroTurnTaker == base.HeroTurnTaker).ToString());
                }
                else if (ca.ActionToCancel is MakeDecisionAction)
                {
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel is MakeDecisionAction");
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.GetType: " + (ca.ActionToCancel as MakeDecisionAction).Decision.GetType().ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.SelectionType: " + (ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType.ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.DecisionMaker.Name: " + (ca.ActionToCancel as MakeDecisionAction).Decision.DecisionMaker.Name);
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.DecisionMaker == base.HeroTurnTaker: " + ((ca.ActionToCancel as MakeDecisionAction).Decision.DecisionMaker == base.HeroTurnTaker).ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.SelectionType == SelectionType.PlayCard: " + ((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.PlayCard).ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.SelectionType == SelectionType.UsePower: " + ((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.UsePower).ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: ca.ActionToCancel.Decision.SelectionType == SelectionType.DrawCard: " + ((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.DrawCard).ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: playing.Contains(ca.ActionToCancel.Decision.SelectionType): " + playing.Contains((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType).ToString());
                    Log.Debug("AlienHeartCardController.LogCancelAction: usingPower.Contains(ca.ActionToCancel.Decision.SelectionType): " + usingPower.Contains((ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType).ToString());
                }
            }
            yield break;
        }

        private IEnumerator LogSetPhaseActionCountAction(SetPhaseActionCountAction spca)
        {
            Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction activated");
            Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca: " + spca.ToString());
            if (spca.AmountToSet.HasValue)
            {
                Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.AmountToSet.HasValue returns true");
                Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.AmountToSet.Value: " + spca.AmountToSet.Value.ToString());
            }
            else
            {
                Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.AmountToSet.HasValue returns false");
            }
            Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.TurnPhase.TurnTaker.Name: " + spca.TurnPhase.TurnTaker.Name);
            Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.TurnPhase.TurnTaker == base.TurnTaker: " + (spca.TurnPhase.TurnTaker == base.TurnTaker).ToString());
            switch (spca.TurnPhase.Phase)
            {
                case Phase.PlayCard:
                    Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.TurnPhase.Phase == Phase.PlayCard");
                    break;
                case Phase.UsePower:
                    Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.TurnPhase.Phase == Phase.UsePower");
                    break;
                case Phase.DrawCard:
                    Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.TurnPhase.Phase == Phase.DrawCard");
                    break;
            }
            Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction activated");
            if (spca.CardSource != null)
            {
                Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.CardSource: " + spca.CardSource.ToString());
                Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.CardSource.Card.IsHero: " + spca.CardSource.Card.IsHero.ToString());
            }
            else
            {
                Log.Debug("AlienHeartCardController.LogSetPhaseActionCountAction: spca.CardSource == null");
            }
            yield break;
        }

        private IEnumerator LogPhaseChangeAction(PhaseChangeAction pca)
        {
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction activated: pca: " + pca.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.TurnTaker: " + pca.FromPhase.TurnTaker.Name);
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.TurnPhase: " + pca.FromPhase.Phase.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.GetPhaseActionCount: " + pca.FromPhase.GetPhaseActionCount().ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.PhaseActionCountRoot: " + pca.FromPhase.PhaseActionCountRoot.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.PhaseActionCountModifiers: " + pca.FromPhase.PhaseActionCountModifiers.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.PhaseActionCountUsed: " + pca.FromPhase.PhaseActionCountUsed.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.CanPerformAction: " + pca.FromPhase.CanPerformAction.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.FromPhase.WasSkipped: " + pca.FromPhase.WasSkipped.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.TurnTaker: " + pca.ToPhase.TurnTaker.Name);
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.TurnPhase: " + pca.ToPhase.Phase.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.GetPhaseActionCount: " + pca.ToPhase.GetPhaseActionCount().ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.PhaseActionCountRoot: " + pca.ToPhase.PhaseActionCountRoot.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.PhaseActionCountModifiers: " + pca.ToPhase.PhaseActionCountModifiers.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.PhaseActionCountUsed: " + pca.ToPhase.PhaseActionCountUsed.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.CanPerformAction: " + pca.ToPhase.CanPerformAction.ToString());
            Log.Debug("AlienHeartCardController.LogPhaseChangeAction: pca.ToPhase.WasSkipped: " + pca.ToPhase.WasSkipped.ToString());

            //Log.Debug("AlienHeartCardController.LogPhaseChangeAction: ");
            yield break;
        }

        public IEnumerator PreventedPlayResponse(GameAction ga)
        {
            // "... you may use a power."
            if (!HasBeenSetToTrueThisTurn(PlayPrevented))
            {
                SetCardPropertyToTrueIfRealAction(PlayPrevented);
                IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
        }

        private IEnumerator PreventedPlayChoiceResponse(GameAction ga)
        {
            bool couldChooseSphere = false;
            HeroTurnTakerController decisionMaker = base.HeroTurnTakerController;
            if (ga is MessageAction ma)
            {
                couldChooseSphere = true;
                decisionMaker = ma.DecisionMaker;
            }
            else if (ga is CancelAction ca)
            {
                // A MakeDecisionAction for a SelectTurnTaker(s)Decision with SelectionType.PlayCard was prevented because none of the listed TurnTakers can currently play cards
                // This only counts as Sphere being prevented if the Decision would've selected Sphere, so we have to verify that he would be chosen- if so, run PreventedPlayResponse
                MakeDecisionAction canceled = ca.ActionToCancel as MakeDecisionAction;
                IDecision preventedChoice = canceled.Decision;
                if (preventedChoice is SelectTurnTakerDecision)
                {
                    SelectTurnTakerDecision unasked = preventedChoice as SelectTurnTakerDecision;
                    IEnumerable<TurnTaker> options = unasked.Choices;
                    if (options.Contains(base.TurnTaker))
                    {
                        couldChooseSphere = true;
                        decisionMaker = canceled.DecisionMaker;
                    }
                }
                else if (preventedChoice is SelectTurnTakersDecision)
                {
                    SelectTurnTakersDecision unasked = preventedChoice as SelectTurnTakersDecision;
                    LinqTurnTakerCriteria criteria = unasked.Criteria;
                    if (GameController.FindTurnTakersWhere(criteria.Criteria).Contains(base.TurnTaker))
                    {
                        couldChooseSphere = true;
                        decisionMaker = canceled.DecisionMaker;
                    }
                }
                if (couldChooseSphere)
                {
                    // Sphere was on the list, but could he have performed the relevant action if not for the effect preventing him?
                    if (preventedChoice.SelectionType == SelectionType.PlayCard)
                    {
                        // Looking for a hero to play a card from hand. If Sphere has no cards in hand, he couldn't do it.
                        couldChooseSphere = base.HeroTurnTaker.HasCardsInHand;
                    }
                    else if (preventedChoice.SelectionType == SelectionType.PlayTopCard || preventedChoice.SelectionType == SelectionType.PlayBottomCard)
                    {
                        // Looking for a hero to play a card from their deck. If Sphere has no cards in deck or trash, he couldn't do it.
                        couldChooseSphere = (base.TurnTaker.Deck.HasCards || base.TurnTaker.Trash.HasCards);
                    }
                }
            }
            if (couldChooseSphere)
            {
                List<YesNoCardDecision> results = new List<YesNoCardDecision>();
                List<Card> relevant = new List<Card>();
                relevant.Add(base.Card);
                currentMode = CustomMode.ToPlayCard;
                IEnumerator askCoroutine = base.GameController.MakeYesNoCardDecision(decisionMaker, SelectionType.Custom, base.CharacterCard, storedResults: results, associatedCards: relevant, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(askCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(askCoroutine);
                }
                YesNoCardDecision choice = results.FirstOrDefault();
                if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
                {
                    // Decision maker verified that Sphere was prevented from playing a card!
                    // Run PreventedPlayResponse
                    IEnumerator respondCoroutine = PreventedPlayResponse(ga);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(respondCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(respondCoroutine);
                    }
                }
            }
        }

        private IEnumerator UnlistedPlayChoiceResponse(MakeDecisionAction mda)
        {
            // A player is about to be selected to play a card, and Sphere can't be chosen because he's under a CannotPlayCardsStatusEffect
            // Let's ask whoever's in charge of the decision whether they'd like to go through with it without Sphere as an option, or skip it and let Sphere use a power
            HeroTurnTakerController decisionMaker = mda.DecisionMaker;
            List<YesNoCardDecision> results = new List<YesNoCardDecision>();
            List<Card> relevant = new List<Card>();
            relevant.Add(base.Card);
            currentMode = CustomMode.UnlistedPlay;
            if (mda.Decision.SelectionType is SelectionType.PlayCard)
            {
                currentAlt = AlternativeMode.PlayCard;
            }
            else if (mda.Decision.SelectionType is SelectionType.PlayTopCard)
            {
                currentAlt = AlternativeMode.PlayTopCard;
            }
            else if (mda.Decision.SelectionType is SelectionType.PlayBottomCard)
            {
                currentAlt = AlternativeMode.PlayBottomCard;
            }
            IEnumerator askCoroutine = base.GameController.MakeYesNoCardDecision(decisionMaker, SelectionType.Custom, base.CharacterCard, storedResults: results, associatedCards: relevant, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(askCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(askCoroutine);
            }
            YesNoCardDecision choice = results.FirstOrDefault();
            if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
            {
                // Decision maker chose to prevent Sphere playing a card!
                // Cancel their original decision and run PreventedPlayResponse instead
                IEnumerator cancelCoroutine = CancelAction(mda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                IEnumerator respondCoroutine = PreventedPlayResponse(mda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(respondCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(respondCoroutine);
                }
            }
        }

        private IEnumerator PreventedDrawResponse(GameAction ga)
        {
            // "... you may play a card."
            if (!HasBeenSetToTrueThisTurn(DrawPrevented))
            {
                SetCardPropertyToTrueIfRealAction(DrawPrevented);
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCardFromHand(base.HeroTurnTakerController, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
        }

        private IEnumerator PreventedDrawChoiceResponse(GameAction ga)
        {
            bool couldChooseSphere = false;
            HeroTurnTakerController decisionMaker = base.HeroTurnTakerController;
            if (ga is MessageAction ma)
            {
                couldChooseSphere = true;
                decisionMaker = ma.DecisionMaker;
            }
            else if (ga is CancelAction ca)
            {
                // A MakeDecisionAction for a SelectTurnTaker(s)Decision with SelectionType.DrawCard was prevented because none of the listed TurnTakers can currently draw cards
                // This only counts as Sphere being prevented if the Decision would've selected Sphere, so we have to verify that he would be chosen- if so, run PreventedDrawResponse
                MakeDecisionAction canceled = ca.ActionToCancel as MakeDecisionAction;
                IDecision preventedChoice = canceled.Decision;
                if (preventedChoice is SelectTurnTakerDecision)
                {
                    SelectTurnTakerDecision unasked = preventedChoice as SelectTurnTakerDecision;
                    IEnumerable<TurnTaker> options = unasked.Choices;
                    if (options.Contains(base.TurnTaker))
                    {
                        couldChooseSphere = true;
                        decisionMaker = canceled.DecisionMaker;
                    }
                }
                else if (preventedChoice is SelectTurnTakersDecision)
                {
                    SelectTurnTakersDecision unasked = preventedChoice as SelectTurnTakersDecision;
                    LinqTurnTakerCriteria criteria = unasked.Criteria;
                    if (GameController.FindTurnTakersWhere(criteria.Criteria).Contains(base.TurnTaker))
                    {
                        couldChooseSphere = true;
                        decisionMaker = canceled.DecisionMaker;
                    }
                }
                if (couldChooseSphere)
                {
                    // Sphere was on the list, but could he have drawn a card if not for the effect preventing him?
                    // Sphere could only have drawn a card if there's at least 1 card in his deck or trash
                    couldChooseSphere = (base.TurnTaker.Deck.HasCards || base.TurnTaker.Trash.HasCards);
                }
            }
            if (couldChooseSphere)
            {
                List<YesNoCardDecision> results = new List<YesNoCardDecision>();
                List<Card> relevant = new List<Card>();
                relevant.Add(base.Card);
                currentMode = CustomMode.ToDrawCard;
                IEnumerator askCoroutine = base.GameController.MakeYesNoCardDecision(decisionMaker, SelectionType.Custom, base.CharacterCard, storedResults: results, associatedCards: relevant, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(askCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(askCoroutine);
                }
                YesNoCardDecision choice = results.FirstOrDefault();
                if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
                {
                    // Decision maker verified that Sphere was prevented from drawing a card!
                    // Run PreventedPlayResponse
                    IEnumerator respondCoroutine = PreventedDrawResponse(ga);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(respondCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(respondCoroutine);
                    }
                }
            }
        }

        private IEnumerator UnlistedDrawChoiceResponse(MakeDecisionAction mda)
        {
            // A player is about to be selected to draw a card, and Sphere can't be chosen, probably because he's under a CannotDrawCardsStatusEffect
            // Let's ask whoever's in charge of the decision whether they'd like to go through with it without Sphere as an option, or skip it and let Sphere play a card
            HeroTurnTakerController decisionMaker = mda.DecisionMaker;
            List<YesNoCardDecision> results = new List<YesNoCardDecision>();
            List<Card> relevant = new List<Card>();
            relevant.Add(base.Card);
            currentMode = CustomMode.UnlistedDraw;
            IEnumerator askCoroutine = base.GameController.MakeYesNoCardDecision(decisionMaker, SelectionType.Custom, base.CharacterCard, storedResults: results, associatedCards: relevant, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(askCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(askCoroutine);
            }
            YesNoCardDecision choice = results.FirstOrDefault();
            if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
            {
                // Decision maker chose to prevent Sphere drawing a card!
                // Cancel their original decision and run PreventedDrawResponse instead
                IEnumerator cancelCoroutine = CancelAction(mda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                IEnumerator respondCoroutine = PreventedDrawResponse(mda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(respondCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(respondCoroutine);
                }
            }
        }

        private IEnumerator PreventedPowerResponse(GameAction ga)
        {
            // "... you may draw a card."
            if (!HasBeenSetToTrueThisTurn(PowerPrevented))
            {
                SetCardPropertyToTrueIfRealAction(PowerPrevented);
                IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
        }

        private IEnumerator PreventedPowerChoiceResponse(GameAction ga)
        {
            bool couldChooseSphere = false;
            HeroTurnTakerController decisionMaker = base.HeroTurnTakerController;
            if (ga is MessageAction ma)
            {
                // SelectHeroToUsePower sent this message instead of choosing a hero because no one can use powers
                // This only counts as Sphere being prevented if the Decision (had there been one) would've selected Sphere, so we have to verify that he would be chosen- if so, run PreventedPowerResponse
                couldChooseSphere = true;
                decisionMaker = ma.DecisionMaker;
            }
            else if (ga is CancelAction ca)
            {
                // A MakeDecisionAction for a SelectTurnTaker(s)Decision with SelectionType.UsePower was prevented because none of the listed TurnTakers can currently use powers
                // This only counts as Sphere being prevented if the Decision would've selected Sphere, so we have to verify that he would be chosen- if so, run PreventedPowerResponse
                MakeDecisionAction canceled = ca.ActionToCancel as MakeDecisionAction;
                IDecision preventedChoice = canceled.Decision;
                if (preventedChoice is SelectTurnTakerDecision)
                {
                    SelectTurnTakerDecision unasked = preventedChoice as SelectTurnTakerDecision;
                    IEnumerable<TurnTaker> options = unasked.Choices;
                    if (options.Contains(base.TurnTaker))
                    {
                        couldChooseSphere = true;
                        decisionMaker = canceled.DecisionMaker;
                    }
                }
                else if (preventedChoice is SelectTurnTakersDecision)
                {
                    SelectTurnTakersDecision unasked = preventedChoice as SelectTurnTakersDecision;
                    LinqTurnTakerCriteria criteria = unasked.Criteria;
                    if (GameController.FindTurnTakersWhere(criteria.Criteria).Contains(base.TurnTaker))
                    {
                        couldChooseSphere = true;
                        decisionMaker = canceled.DecisionMaker;
                    }
                }
            }
            if (couldChooseSphere)
            {
                List<YesNoCardDecision> results = new List<YesNoCardDecision>();
                List<Card> relevant = new List<Card>();
                relevant.Add(base.Card);
                currentMode = CustomMode.ToUsePower;
                IEnumerator askCoroutine = base.GameController.MakeYesNoCardDecision(decisionMaker, SelectionType.Custom, base.CharacterCard, storedResults: results, associatedCards: relevant, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(askCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(askCoroutine);
                }
                YesNoCardDecision choice = results.FirstOrDefault();
                if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
                {
                    // Decision maker verified that Sphere was prevented from drawing a card!
                    // Run PreventedPlayResponse
                    IEnumerator respondCoroutine = PreventedPowerResponse(ga);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(respondCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(respondCoroutine);
                    }
                }
            }
        }

        private IEnumerator UnlistedPowerChoiceResponse(MakeDecisionAction mda)
        {
            // A player is about to be selected to use a power, and Sphere can't be chosen, probably because he's under a CannotUsePowersStatusEffect
            // Let's ask whoever's in charge of the decision whether they'd like to go through with it without Sphere as an option, or skip it and let Sphere draw a card
            HeroTurnTakerController decisionMaker = mda.DecisionMaker;
            List<YesNoCardDecision> results = new List<YesNoCardDecision>();
            List<Card> relevant = new List<Card>();
            relevant.Add(base.Card);
            currentMode = CustomMode.UnlistedPower;
            IEnumerator askCoroutine = base.GameController.MakeYesNoCardDecision(decisionMaker, SelectionType.Custom, base.CharacterCard, storedResults: results, associatedCards: relevant, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(askCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(askCoroutine);
            }
            YesNoCardDecision choice = results.FirstOrDefault();
            if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
            {
                // Decision maker chose to prevent Sphere using a power!
                // Cancel their original decision and run PreventedPowerResponse instead
                IEnumerator cancelCoroutine = CancelAction(mda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                IEnumerator respondCoroutine = PreventedPowerResponse(mda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(respondCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(respondCoroutine);
                }
            }
        }
    }
}
