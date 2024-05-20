using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
    public class UnmatchedAlacrityCardController : HasteUtilityCardController
    {
        public UnmatchedAlacrityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(SpeedPool);
        }

        private string key = "UnmatchedAlacrityKey";

        public override IEnumerator Play()
        {
            SetKey(null);
            return base.Play();
        }

        public override void AddTriggers()
        {
            //When you would add tokens to your speed pool, reduce the number of tokens added this way by 2, to a minimum of 0.
            //AddTrigger<AddTokensToPoolAction>((AddTokensToPoolAction tp) => HasteSpeedPoolUtility.GetSpeedPool(this) != null && tp.TokenPool == HasteSpeedPoolUtility.GetSpeedPool(this) && tp.CardSource != null && tp.CardSource.Card.Owner == this.TurnTaker && (tp.CardSource.Card != this.Card || ( this.Card != this.CardWithoutReplacements && !tp.CardSource.AssociatedCardSources.Any((CardSource cs) => cs.Card == this.CardWithoutReplacements))), ReduceTokensResponse, TriggerType.Hidden, TriggerTiming.Before);

            AddTrigger<GameAction>((GameAction ga) => !IsPropertyCurrentlyTrue(key), SetKey, TriggerType.Hidden, TriggerTiming.Before);
            AddAfterLeavesPlayAction(ResetKey, TriggerType.HiddenLast);
            AddBeforeLeavesPlayActions(ResetKey);

            //At the end of each turn, you may remove 1 token from your speed pool to draw a card, play a card, or use a power.
            AddEndOfTurnTrigger((TurnTaker tt) => HasteSpeedPoolUtility.GetSpeedPool(this) != null && HasteSpeedPoolUtility.GetSpeedPool(this).CurrentValue > 0, EndOfTurnResponse, new TriggerType[] { TriggerType.ModifyDamageAmount, TriggerType.DrawCard, TriggerType.PlayCard, TriggerType.UsePower });

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        private IEnumerator SetKey(GameAction ga)
        {
            SetCardProperty(key, true);
            yield return null;
        }

        private IEnumerator ResetKey(GameAction ga)
        {
            if (this.Card != this.CardWithoutReplacements)
            {
                //If Uh Yeah is destroyed while copying this card, it should only reset its own properties
                SetCardProperty(key, false);
            }
            else
            {
                //If this card is destroyed, reset all cards' properties so that Uh Yeah no longer copies this card
                ResetAllCardProperties(key);
            }
            yield return null;
        }

        //private IEnumerator ReduceTokensResponse(AddTokensToPoolAction tp)
        //{
        //    int num = tp.NumberOfTokensToAdd - 2;
        //    CardSource source = tp.CardSource;
        //    IEnumerator coroutine = CancelAction(tp,false);
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    //coroutine = base.GameController.AddTokensToPool(SpeedPool, num >= 0 ? num : 0, GetCardSource());
        //    if (num > 0)
        //    {
        //        coroutine = AddSpeedTokens(num >= 0 ? num : 0);
        //        if (base.UseUnityCoroutines)
        //        {
        //            yield return base.GameController.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            base.GameController.ExhaustCoroutine(coroutine);
        //        }
        //    }
        //}

        public override string DecisionAction => " to draw a card, play a card, or use a power";

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerable<Function> functionChoices = new Function[3]
                {
                new Function(base.HeroTurnTakerController, "Remove 1 speed token and draw a card", SelectionType.DrawCard, () => DrawResponse()),
                new Function(base.HeroTurnTakerController, "Remove 1 speed token and play a card", SelectionType.PlayCard, () => PlayResponse()),
                new Function(base.HeroTurnTakerController, "Remove 1 speed token and use a power", SelectionType.UsePower, () => PowerResponse())
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        private IEnumerator DrawResponse()
        {
            List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
            IEnumerator coroutine = RemoveSpeedTokens(1, null, false, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidRemoveTokens(results))
            {
                coroutine = DrawCard(this.HeroTurnTaker);
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

        private IEnumerator PlayResponse()
        {
            List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
            IEnumerator coroutine = RemoveSpeedTokens(1, null, false, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidRemoveTokens(results))
            {
                coroutine = SelectAndPlayCardFromHand(this.HeroTurnTakerController, false);
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

        private IEnumerator PowerResponse()
        {
            List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
            IEnumerator coroutine = RemoveSpeedTokens(1, null, false, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidRemoveTokens(results))
            {
                coroutine = base.GameController.SelectAndUsePower(this.HeroTurnTakerController, false, cardSource: GetCardSource());
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
}

