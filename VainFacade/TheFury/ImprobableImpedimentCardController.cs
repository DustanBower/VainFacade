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
        }

        public Guid? DamageReacted { get; set; }

        private readonly string PlayedThisTurn = "ThisCardWasPlayedThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "If this card is destroyed, return it to your hand."
            AddAfterDestroyedAction(ReturnToHandResponse);
            // "When {TheFuryCharacter} is dealt damage on the turn this card enters play, you may play a card. If you do, increase the next damage dealt to {TheFuryCharacter} by 1. If it is a Coincidence, repeat this text."
            AddTrigger((DealDamageAction dda) => HasBeenSetToTrueThisTurn(PlayedThisTurn) && dda.Target == base.CharacterCard && dda.DidDealDamage, PlayAndIncreaseNextResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.CreateStatusEffect }, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(PlayedThisTurn);
            // "When {TheFuryCharacter} would be dealt damage by a source other than {TheFuryCharacter}, you may prevent that damage. If you do, destroy this card."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && (dda.DamageSource == null || dda.DamageSource.Card == null || dda.DamageSource.Card != base.CharacterCard) && dda.CanDealDamage && !base.Card.IsBeingDestroyed, PreventAndDestroyResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Do you want to return this card to your hand?", "deciding whether to return " + base.Card.Title + " to their hand", "Vote for whether to return " + base.Card.Title + " to " + base.TurnTaker.Name + "'s hand", "whether to return " + base.Card.Title + " to their hand");
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

        private IEnumerator PlayAndIncreaseNextResponse(DealDamageAction dda)
        {
            // "... you may play a card."
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
                // "If you do, increase the next damage dealt to {TheFuryCharacter} by 1."
                IEnumerator increaseCoroutine = IncreaseNextDamageTo(base.CharacterCard, 1, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(increaseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(increaseCoroutine);
                }
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
                    IEnumerator repeatCoroutine = PlayAndIncreaseNextResponse(null);
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

        private IEnumerator PreventAndDestroyResponse(DealDamageAction dda)
        {
            // "... you may prevent that damage. If you do, destroy this card."
            if (!DamageReacted.HasValue || DamageReacted.Value != dda.InstanceIdentifier)
            {
                List<YesNoCardDecision> choices = new List<YesNoCardDecision>();
                IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.DestroyCard, base.Card, dda, choices, cardSource: GetCardSource());
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
                    IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destructCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destructCoroutine);
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
