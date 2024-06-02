using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Haste
{
	public class HasteUtilityCardController:CardController
	{
		public HasteUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
		{
		}

        public TokenPool SpeedPool => base.CharacterCard.FindTokenPool("SpeedPool");

		public TokenPool GetSpeedPool()
		{
			return HasteSpeedPoolUtility.GetSpeedPool(this);
		}

        public IEnumerator AddSpeedTokens(int amount)
		{
			return HasteSpeedPoolUtility.AddSpeedTokens(this, amount, GetCardSource());
        }

		public IEnumerator RemoveSpeedTokens(int amount, GameAction gameAction = null, bool optional = false, List<RemoveTokensFromPoolAction> storedResults = null, IEnumerable<Card> associatedCards = null, CardSource cardSource = null)
		{
            if (cardSource == null)
            {
                cardSource = GetCardSource();
            }
			return HasteSpeedPoolUtility.RemoveSpeedTokens(this, amount, gameAction, optional, storedResults, associatedCards, GetCardSource());
		}

		public IEnumerator RemoveAnyNumberOfSpeedTokens(List<int?> storedResults)
		{
            IEnumerator coroutine;
            if (HasteSpeedPoolUtility.GetSpeedPool(this) == null)
            {
                coroutine = HasteSpeedPoolUtility.SpeedPoolErrorMessage(this);
                if (this.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(coroutine);
                }
                yield break;
            }

            coroutine = RemoveAnyNumberOfTokensFromTokenPool(HasteSpeedPoolUtility.GetSpeedPool(this), storedResults);
            if (this.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(coroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(coroutine);
            }
        }

		public virtual string DecisionAction => "";

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
			if (decision is YesNoAmountDecision && ((YesNoAmountDecision)decision).Amount.HasValue)
			{
				int amount = ((YesNoAmountDecision)decision).Amount.Value;
                string amountString = amount == 1 ? "a" : amount.ToString();
				string name = decision.DecisionMaker.Name;
				string plural = amount == 1 ? "" : "s";
                return new CustomDecisionText(
                $"Remove {amountString} speed token{plural}{DecisionAction}?",
                $"{name} is deciding whether to remove {amountString} speed token{plural}{DecisionAction}",
                $"Vote on whether to remove {amountString} speed token{plural}{DecisionAction}",
                $"remove {amountString} speed token{plural}{DecisionAction}"
				);
            }
			return null;
        }

        public void ResetAllCardProperties(string key)
        {
            IEnumerable<CardPropertiesJournalEntry> source = from e in Journal.CardPropertiesEntries((CardPropertiesJournalEntry e) => e.Key == key) where e.Key == key select e;
            IEnumerable<Card> sourceCards = source.Where((CardPropertiesJournalEntry e) => e.BoolValue.HasValue).Select((CardPropertiesJournalEntry e) => e.Card).Distinct();
            List<Card> list = sourceCards.ToList();
            Console.WriteLine(key + " properties for " + list.Select((Card c) => c.Title).ToCommaList() + " are being reset by " + this.Card.Title);
            foreach (Card card in list)
            {
                Journal.RecordCardProperties(card, key, false);
            }
            Console.WriteLine(HasteSpeedPoolUtility.GetNumberOfCardPropertiesTrue(this, key).ToString() + " card properties are currently true");
        }
    }
}

