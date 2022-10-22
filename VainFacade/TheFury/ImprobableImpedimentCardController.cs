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
    public class ImprobableImpedimentCardController : TheFuryUtilityCardController
    {
        public ImprobableImpedimentCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            // If in play, show whether this card was played this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(PlayedThisTurn, base.Card.Title + " was played this turn.", base.Card.Title + " was not played this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, base.CharacterCard.Title + " has already been dealt damage this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not been dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText && HasBeenSetToTrueThisTurn(PlayedThisTurn);
        }

        public Guid? DamageReacted { get; set; }

        private readonly string PlayedThisTurn = "ThisCardWasPlayedThisTurn";
        private readonly string FirstDamageThisTurn = "HasBeenDealtDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "If this card is destroyed, return it to your hand."
            AddAfterDestroyedAction(ReturnToHandResponse);
            // "The first time {TheFuryCharacter} is dealt damage on the turn this card enters play, you may play a card. If it is a Coincidence, repeat this text."
            AddTrigger((DealDamageAction dda) => HasBeenSetToTrueThisTurn(PlayedThisTurn) && !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.Target == base.CharacterCard && dda.DidDealDamage, PlayCardResponse, new TriggerType[] { TriggerType.PlayCard }, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(PlayedThisTurn);
            // "When {TheFuryCharacter} would be dealt damage by another source, you may prevent it. If you do, increase the next damage dealt to her by X, where X = 1 plus the amount that damage was increased by, and discard or destroy this card."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && (dda.DamageSource == null || dda.DamageSource.Card == null || dda.DamageSource.Card != base.CharacterCard) && dda.CanDealDamage && !base.Card.IsBeingDestroyed, PreventIncreaseDiscardOrDestroyResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
        }

        private IEnumerator ReturnToHandResponse(GameAction ga)
        {
            // "... return it to your hand."
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.ToHero().Hand, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        public override IEnumerator Play()
        {
            SetCardPropertyToTrueIfRealAction(PlayedThisTurn);
            yield break;
        }

        private IEnumerator PlayCardResponse(DealDamageAction dda)
        {
            // "... you may play a card."
            SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
            List<PlayCardAction> played = new List<PlayCardAction>();
            IEnumerator playCoroutine = base.GameController.SelectAndPlayCardFromHand(DecisionMaker, true, played, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            if (DidPlayCards(played))
            {
                // "If it is a Coincidence, repeat this text."
                Card firstCoincidence = null;
                foreach (PlayCardAction pca in played)
                {
                    if (pca.WasCardPlayed)
                    {
                        if (pca.CardToPlay.DoKeywordsContain(CoincidenceKeyword))
                        {
                            firstCoincidence = pca.CardToPlay;
                            break;
                        }
                    }
                }
                if (firstCoincidence != null)
                {
                    IEnumerator repeatCoroutine = PlayCardResponse(null);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(repeatCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(repeatCoroutine);
                    }
                }
            }
        }

        private IEnumerator PreventIncreaseDiscardOrDestroyResponse(DealDamageAction dda)
        {
            // "... you may prevent it."
            if (!DamageReacted.HasValue || DamageReacted.Value != dda.InstanceIdentifier)
            {
                List<YesNoCardDecision> choices = new List<YesNoCardDecision>();
                IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.PreventDamage, base.Card, dda, choices, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (DidPlayerAnswerYes(choices))
                {
                    DamageReacted = dda.InstanceIdentifier;
                }
            }
            if (DamageReacted.HasValue && DamageReacted.Value == dda.InstanceIdentifier)
            {
                IEnumerator cancelCoroutine = CancelAction(dda, isPreventEffect: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                if (IsRealAction(dda))
                {
                    // "If you do, increase the next damage dealt to her by X, where X = 1 plus the amount that damage was increased by..."
                    List<ModifyDealDamageAction> mods = dda.DamageModifiers.ToList();
                    int x = 1;
                    foreach (ModifyDealDamageAction mod in mods)
                    {
                        if (mod is IncreaseDamageAction plus)
                        {
                            x += plus.AdjustmentAmount;
                        }
                        else if (mod is ReduceDamageAction minus)
                        {
                            x -= minus.AdjustmentAmount;
                        }
                    }
                    if (x > 0)
                    {
                        IEnumerator increaseCoroutine = IncreaseNextDamageTo(base.CharacterCard, x, GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(increaseCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(increaseCoroutine);
                        }
                    }
                    // "... and discard or destroy this card."
                    IEnumerable<Function> options = new Function[2]
                    {
                        new Function(DecisionMaker, "Discard " + base.Card.Title, SelectionType.DiscardCard, () => base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.Trash, showMessage: true, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource())),
                        new Function(DecisionMaker, "Destroy " + base.Card.Title, SelectionType.DestroySelf, () => base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource()))
                    };
                    SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, false, cardSource: GetCardSource());
                    IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(chooseCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(chooseCoroutine);
                    }
                }
            }
            if (IsRealAction(dda))
            {
                DamageReacted = null;
            }
        }
    }
}
