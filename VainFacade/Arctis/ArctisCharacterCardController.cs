using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace VainFacadePlaytest.Arctis
{
	public class ArctisCharacterCardController:HeroCharacterCardController
	{
		public ArctisCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
		}

        ITrigger trigger;
        int reduceAmount;
        public Guid? ReduceAndPlay { get; set; }
        private Card Target;

        public override IEnumerator UsePower(int index = 0)
        {
            //You may draw a card.
            int num = GetPowerNumeral(0, 1);
            List<DrawCardAction> drawResults = new List<DrawCardAction>();
            IEnumerator coroutine = DrawCard(this.HeroTurnTaker, true, drawResults, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (GetNumberOfCardsDrawn(drawResults) == 0)
            {
                //If you don't, then until the end of your next turn, when cold damage would be dealt to a target, you may reduce that damage by 1 and put an icework from your hand into play.
                OnDealDamageStatusEffect effect1 = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "PowerResponse", "When cold damage would be dealt to " + this.Card.Title + ", " + this.TurnTaker.Name + " may reduce that damage by " + num + " and put an icework from their hand into play.", new TriggerType[] {TriggerType.ReduceDamage, TriggerType.PutIntoPlay }, this.TurnTaker, this.Card, new int[] { num });
                effect1.UntilEndOfNextTurn(this.TurnTaker);
                effect1.BeforeOrAfter = BeforeOrAfter.Before;
                effect1.DamageTypeCriteria.AddType(DamageType.Cold);
                effect1.SourceCriteria.IsSpecificCard = this.Card;
                coroutine = AddStatusEffect(effect1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                OnDealDamageStatusEffect effect2 = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "PowerResponse", "When cold damage would be dealt by " + this.Card.Title + ", " + this.TurnTaker.Name + " may reduce that damage by " + num + " and put an icework from their hand into play.", new TriggerType[] { TriggerType.ReduceDamage, TriggerType.PutIntoPlay }, this.TurnTaker, this.Card, new int[] { num });
                effect2.UntilEndOfNextTurn(this.TurnTaker);
                effect2.BeforeOrAfter = BeforeOrAfter.Before;
                effect2.DamageTypeCriteria.AddType(DamageType.Cold);
                effect2.SourceCriteria.IsNotSpecificCard = this.Card; //Only trigger one effect when Arctis hits himself with cold
                effect2.TargetCriteria.IsSpecificCard = this.Card;
                coroutine = AddStatusEffect(effect2);
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

        //public IEnumerator PowerResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        //{
        //    List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
        //    HeroTurnTakerController httc = FindHeroTurnTakerController(hero.ToHero());
        //    reduceAmount = powerNumerals[0];
        //    IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(httc, SelectionType.Custom, this.Card, dda, storedResults, null, GetCardSource());
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }
        //    if (DidPlayerAnswerYes(storedResults))
        //    {
        //        coroutine = base.GameController.ReduceDamage(dda, powerNumerals[0], null, GetCardSource());
        //        if (base.UseUnityCoroutines)
        //        {
        //            yield return base.GameController.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            base.GameController.ExhaustCoroutine(coroutine);
        //        }

        //        trigger = AddTrigger<DealDamageAction>((DealDamageAction dd) => dd == dda, (DealDamageAction dd) => PlayIcework(httc, effect), TriggerType.PutIntoPlay, TriggerTiming.After, requireActionSuccess: false); ;
        //    }
        //}

        public IEnumerator PowerResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            HeroTurnTakerController httc = FindHeroTurnTakerController(hero.ToHero());
            reduceAmount = powerNumerals[0];
            if (base.GameController.PretendMode || dda.Target != Target)
            {
                List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
                IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(httc, SelectionType.Custom, this.Card, dda, storedResults, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidPlayerAnswerYes(storedResults))
                {
                    ReduceAndPlay = dda.InstanceIdentifier;
                }
                Target = dda.Target;
            }
            else if (ReduceAndPlay.HasValue && ReduceAndPlay.Value == dda.InstanceIdentifier)
            {
                IEnumerator coroutine2 = base.GameController.ReduceDamage(dda, powerNumerals[0], null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (!base.GameController.PretendMode)
            {
                if (ReduceAndPlay.HasValue && ReduceAndPlay.Value == dda.InstanceIdentifier)
                {
                    trigger = AddTrigger<DealDamageAction>((DealDamageAction dd) => dd == dda, (DealDamageAction dd) => PlayIcework(httc, effect), TriggerType.PutIntoPlay, TriggerTiming.After, requireActionSuccess: false);
                }
                ReduceAndPlay = null;
            }
        }


        private IEnumerator PlayIcework(HeroTurnTakerController httc, StatusEffect effect)
        {
            //Console.WriteLine("Playing icework from Arctis' power");
            RemoveTrigger(trigger);
            IEnumerator coroutine = base.GameController.SelectAndPlayCardFromHand(httc, false, null, new LinqCardCriteria((Card c) => IsIcework(c), "", false, false, "icework", "iceworks"), true, cardSource: new CardSource(FindCardController(effect.CardSource)));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision)
            {
                return new CustomDecisionText(
                $"Do you want to reduce this damage by {reduceAmount} and put an icework into play?",
                $"{decision.DecisionMaker.Name} is deciding whether to reduce this damage by {reduceAmount} and put an icework into play.",
                $"Vote for whether to reduce this damage by {reduceAmount} and put an icework into play.",
                $"whether to reduce this damage by {reduceAmount} and put an icework into play."
                );
            }
            return null;

        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    //Select a target. Reduce the next damage dealt to that target by 2.
                    List<SelectCardDecision> results = new List<SelectCardDecision>();
                    coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText), results, false, false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidSelectCard(results))
                    {
                        Card selectedCard = GetSelectedCard(results);
                        ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(2);
                        effect.TargetCriteria.IsSpecificCard = selectedCard;
                        effect.NumberOfUses = 1;
                        coroutine = AddStatusEffect(effect);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    break;
                case 1:
                    //1 hero may use a power.
                    coroutine = base.GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
                case 2:
                    //Destroy an ongoing card.
                    coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c),"ongoing"), false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
            }
        }

        private bool IsIcework(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "icework");
        }

        public override IEnumerable<Card> FilterDecisionCardChoices(SelectCardDecision decision)
        {
            if (decision.SelectionType == SelectionType.ReduceNextDamageTaken && decision.Choices.Where((Card c) => IsHeroTarget(c)).Count() > 0)
            {
                return decision.Choices.Where((Card c) => !IsHeroTarget(c));
            }
            return null;
        }
    }
}

