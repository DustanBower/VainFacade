using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class SympatheticEchoCardController:PushCardControllerUtilities
	{
		public SympatheticEchoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(ChangeDamageTypeKey,this.Card.Title + " has been used to change damage type this turn", this.Card.Title + " has not been used to change damage type this turn");
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey, this.CharacterCard.Title + " has been dealt melee, projectile, or sonic damage by any target this turn", this.CharacterCard.Title + " has not been dealt melee, projectile, or sonic damage by a target this turn");
		}

        private const string ChangeDamageTypeKey = "ChangeDamageTypeKey";
        private const string FirstDamageKey = "FirstDamageKey";
        private DamageType? _selectedDamageType;
        public Card CardToDiscard { get; set; }

        public override void AddTriggers()
        {
            //Once per turn, when damage would be dealt to a target, you may discard a card to change the damage type to projectile or melee.
            //Based on Mind over Matter
            //ChangeDamageTypeTrigger trigger = new ChangeDamageTypeTrigger(base.GameController, (DealDamageAction dd) => !IsPropertyTrue(ChangeDamageTypeKey), ChangeTypeResponse, new TriggerType[1] { TriggerType.ChangeDamageType }, new DamageType[2] { DamageType.Melee, DamageType.Projectile }, GetCardSource());
            //AddTrigger(trigger);

            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(ChangeDamageTypeKey), ChangeTypeResponse, new TriggerType[1] { TriggerType.ChangeDamageType }, TriggerTiming.Before);

            //The first time each turn melee, projectile, or sonic damage is dealt to Push by a target, Push may deal the source of that damage 3 projectile damage.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey)  && (dd.DamageType == DamageType.Melee || dd.DamageType == DamageType.Projectile || dd.DamageType == DamageType.Sonic) && dd.DamageSource.IsTarget && dd.Target == this.CharacterCard && dd.DidDealDamage, CounterDamageResponse, TriggerType.DealDamage, TriggerTiming.After);

            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageKey), TriggerType.Hidden);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(ChangeDamageTypeKey), TriggerType.Hidden);
        }

        private IEnumerator CounterDamageResponse(DealDamageAction dd)
        {
            SetCardProperty(FirstDamageKey, true);
            if (dd.DamageSource.Card.IsInPlayAndHasGameText && dd.DamageSource.Card.IsTarget)
            {
                IEnumerator coroutine = DealDamage(this.CharacterCard, dd.DamageSource.Card, 3, DamageType.Projectile, false, true, true, cardSource: GetCardSource());
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

        private IEnumerator ChangeTypeResponse(DealDamageAction dd)
        {
            if (base.GameController.PretendMode)
            {
                List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
                List<DealDamageAction> list = new List<DealDamageAction>();
                list.Add(dd);
                IEnumerator coroutine = base.GameController.SelectAndDiscardCard(DecisionMaker, optional: true, null, discardResults, SelectionType.Custom, list, this.TurnTaker, ignoreBattleZone: false, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidDiscardCards(discardResults))
                {
                    SetCardPropertyToTrueIfRealAction(ChangeDamageTypeKey, null, dd);
                    if (discardResults.Any((DiscardCardAction dc) => dc.IsPretend))
                    {
                        CardToDiscard = discardResults.First().CardToDiscard;
                    }
                    if (dd.DamageType != DamageType.Projectile && dd.DamageType != DamageType.Melee)
                    {
                        List<SelectDamageTypeDecision> storedType = new List<SelectDamageTypeDecision>();
                        DamageType[] choices = new DamageType[2]
                        {
                            DamageType.Melee,
                            DamageType.Projectile
                        };
                        coroutine = base.GameController.SelectDamageType(DecisionMaker, storedType, choices, dd, SelectionType.DamageType, GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        _selectedDamageType = GetSelectedDamageType(storedType);
                    }
                    else if (dd.DamageType == DamageType.Projectile)
                    {
                        _selectedDamageType = DamageType.Melee;
                    }
                    else if (dd.DamageType == DamageType.Melee)
                    {
                        _selectedDamageType = DamageType.Projectile;
                    }
                }
            }
            if (CardToDiscard != null)
            {
                //SetCardProperty(ChangeDamageTypeKey, true);
                SetCardPropertyToTrueIfRealAction(ChangeDamageTypeKey, null, dd);
                IEnumerator coroutine2 = base.GameController.DiscardCard(DecisionMaker, CardToDiscard, null, base.TurnTaker, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (!_selectedDamageType.HasValue)
            {
                yield break;
            }
            if (_selectedDamageType.HasValue)
            {
                IEnumerator coroutine3 = base.GameController.ChangeDamageType(dd, _selectedDamageType.Value, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
            }
            if (!base.GameController.PretendMode)
            {
                _selectedDamageType = null;
                CardToDiscard = null;
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is SelectCardDecision)
            {
                DamageType type = ((SelectCardDecision)decision).DealDamageInfo.FirstOrDefault().DamageType;
                string text1 = "";
                if (type == DamageType.Projectile)
                {
                    text1 = "melee";
                }
                else if (type == DamageType.Melee)
                {
                    text1 = "projectile";
                }
                else
                {
                    text1 = "melee or projectile";
                }
                //string text1 = "melee or projectile";
                string text = $"to change the damage type to {text1}";
                return new CustomDecisionText(
                $"discard a card {text}.",
                $"{decision.DecisionMaker.Name} is selecting a card to discard {text}.",
                $"Vote for a card to discard {text}.",
                $"a card to discard {text}."
                );
            }
            return null;

        }
    }
}

