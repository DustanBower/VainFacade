using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class TakeEmCardController : BurgessUtilityCardController
    {
        public TakeEmCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            // If in play: show whether this card has increased damage this round
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisRound(IncreasedDamageThisRound), () => base.Card.Title + " has already increased damage this round.", () => base.Card.Title + " has not increased damage yet this round.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // Show number of Backup cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BackupCard);
        }

        protected readonly string IncreasedDamageThisRound = "IncreasedDamageThisRound";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Once per round when {BurgessCharacter} or a Backup would deal damage, you may increase that damage by 1 plus the number of Backup in play."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisRound(IncreasedDamageThisRound) && dda.DamageSource != null && dda.DamageSource.IsCard && (dda.DamageSource.IsSameCard(base.CharacterCard) || BackupCard.Criteria(dda.DamageSource.Card)), IncreaseDamageResponse, TriggerType.IncreaseDamage, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(IncreasedDamageThisRound), TriggerType.Hidden);
            // "If damage is dealt this way, discard 3 cards or destroy this card."
            AddTrigger((DealDamageAction dda) => HasBeenSetToTrueThisRound(IncreasedDamageThisRound) && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsCard && (dda.DamageSource.IsSameCard(base.CharacterCard) || BackupCard.Criteria(dda.DamageSource.Card)) && dda.DamageModifiers.Where((ModifyDealDamageAction m) => m is IncreaseDamageAction && m.CardSource.Card == base.Card).Any(), DiscardOrSelfDestructResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroySelf }, TriggerTiming.After);
        }

        private IEnumerator IncreaseDamageResponse(DealDamageAction dda)
        {
            // "... you may increase that damage by 1 plus the number of Backup in play."
            YesNoDecision choice = new YesNoDecision(base.GameController, base.HeroTurnTakerController, SelectionType.Custom, gameAction: dda, associatedCards: base.Card.ToEnumerable(), cardSource: GetCardSource());
            IEnumerator decideCoroutine = base.GameController.MakeDecisionAction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(decideCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(decideCoroutine);
            }
            if (choice != null && choice.Answer.HasValue && choice.Answer.Value)
            {
                SetCardPropertyToTrueIfRealAction(IncreasedDamageThisRound, gameAction: dda);
                IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, (DealDamageAction a) => base.GameController.FindCardsWhere(BackupInPlay, visibleToCard: GetCardSource()).Count() + 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(increaseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(increaseCoroutine);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Do you want to increase the damage?", "Selecting whether to increase the damage", "Vote for whether to increase the damage.", "Increase the damage");
        }

        private IEnumerator DiscardOrSelfDestructResponse(DealDamageAction dda)
        {
            // "... discard 3 cards or destroy this card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            List<Function> choices = new List<Function>();
            choices.Add(new Function(base.HeroTurnTakerController, "Discard 3 cards", SelectionType.DiscardCard, () => SelectAndDiscardCards(base.HeroTurnTakerController, 3, requiredDecisions: 3, storedResults: discards, allowAutoDecide: base.HeroTurnTaker.NumberOfCardsInHand == 3, responsibleTurnTaker: base.TurnTaker), onlyDisplayIfTrue: base.HeroTurnTaker.NumberOfCardsInHand >= 3, forcedActionMessage: base.Card.Title + " cannot be destroyed, so " + base.TurnTaker.Name + " must discard 3 cards."));
            choices.Add(new Function(base.HeroTurnTakerController, "Destroy " + base.Card.Title, SelectionType.DestroySelf, () => DestroyThisCardResponse(null), onlyDisplayIfTrue: !AskIfCardIsIndestructible(base.Card), forcedActionMessage: base.TurnTaker.Name + " does not have at least 3 cards in hand, so " + base.Card.Title + " must be destroyed."));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, choices, false, noSelectableFunctionMessage: base.TurnTaker.Name + " can neither discard 3 cards nor destroy " + base.Card.Title + ".", cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
