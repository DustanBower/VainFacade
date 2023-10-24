using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace VainFacadePlaytest.Arctis
{
	public class IceworkWithAbilityCardController:IceworkCardController
	{
		public IceworkWithAbilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SetupIcework();
            base.SpecialStringMaker.ShowIfElseSpecialString(CanActivateIceEffect,() => this.TurnTaker.Name + " has not used a {ice} effect this turn",() => this.TurnTaker.Name + " has already used a {ice} effect this turn");
            //base.SpecialStringMaker.ShowListOfCardsInPlay(FirstAbilityCriteria);
            //if (FirstAbilityCriteria.GetDescription() != SecondAbilityCriteria.GetDescription())
            //{
            //    base.SpecialStringMaker.ShowListOfCardsInPlay(SecondAbilityCriteria);
            //}
        }

        protected string FirstAbilityKey;

        //protected LinqCardCriteria FirstAbilityCriteria;

        protected string SecondAbilityKey;

        //protected LinqCardCriteria SecondAbilityCriteria;

        protected bool AllowSameCardTwice;

        protected bool AllowLessThanTwoActivations;

        protected void SetupIcework()
        {
            FirstAbilityKey = "{ice}";
            //FirstAbilityCriteria = null;
            SecondAbilityKey = "{ice}";
            //SecondAbilityCriteria = null;
            AllowSameCardTwice = false;
            AllowLessThanTwoActivations = true;
        }

        public override IEnumerator ActivateAbilityEx(CardDefinition.ActivatableAbilityDefinition definition)
        {
            IEnumerator enumerator = null;
            if (definition.Name == "{ice}")
            {
                enumerator = ActivateIce();
            }
            if (enumerator != null)
            {
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(enumerator);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(enumerator);
                }
            }
        }

        public virtual IEnumerator ActivateIce()
        {
            yield return null;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //If you have activated no {ice} effects this turn, activate up to two {ice} effects.
            //Based on Drake's Pipes
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();
            IEnumerator coroutine;
            if (CanActivateIceEffect())
            {
                AddInhibitorException((GameAction a) => a is MessageAction);
                coroutine = base.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, FirstAbilityKey, null, storedResults, AllowLessThanTwoActivations, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                RemoveInhibitorException();
                bool flag = false;
                if ((!AllowSameCardTwice && AllowLessThanTwoActivations && storedResults.Count() == 0) || (storedResults.Count() > 0 && storedResults.First().SelectedAbility == null))
                {
                    flag = true;
                }
                if (flag)
                {
                    yield break;
                }
                LinqCardCriteria cardCriteria = null;
                if (!AllowSameCardTwice && storedResults.Count() > 0)
                {
                    cardCriteria = new LinqCardCriteria((Card c) => c != storedResults.First().SelectedAbility.CardController.Card);
                }
                AddInhibitorException((GameAction a) => a is MessageAction);
                coroutine = base.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, SecondAbilityKey, cardCriteria, storedResults, AllowLessThanTwoActivations, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                RemoveInhibitorException();
            }
            else
            {
                coroutine = CannotUseIceMessage();
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

        private bool CanActivateIceEffect()
        {
            //StatusEffectController effectController = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is CannotUseIceStatusEffect).FirstOrDefault();
            bool flag = base.GameController.Game.Journal.QueryJournalEntries((ActivateAbilityJournalEntry e) => e.ActivatingTurnTaker == this.TurnTaker && e.AbilityKey == "{ice}").Where(base.GameController.Game.Journal.ThisTurn<ActivateAbilityJournalEntry>()).Any();

            return !flag;
        }

        private IEnumerator CannotUseIceMessage()
        {
            return base.GameController.SendMessageAction(this.TurnTaker.Name + " has already used a {ice} effect this turn",Priority.Low,GetCardSource());
        }
    }
}

