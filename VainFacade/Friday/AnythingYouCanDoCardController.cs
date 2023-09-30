using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class AnythingYouCanDoCardController:CardController
	{
		public AnythingYouCanDoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
		{
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstTimeHPGain, $"{this.Card.Title} has let {this.TurnTaker.Name} regain HP this turn", $"{this.Card.Title} has not yet let {this.TurnTaker.Name} regain HP this turn").Condition = () => this.Card.Location.IsNextToCard;
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(DrawUsed, $"{this.Card.Title} has let {this.TurnTaker.Name} draw a card this turn", $"{this.Card.Title} has not yet let {this.TurnTaker.Name} draw a card this turn").Condition = () => this.Card.Location.IsNextToCard;
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(PlayUsed, $"{this.Card.Title} has let {this.TurnTaker.Name} play a card this turn", $"{this.Card.Title} has not yet let {this.TurnTaker.Name} play a card this turn").Condition = () => this.Card.Location.IsNextToCard;
        }

        private string FirstTimeHPGain = "FirstTimeHPGain";

        private string DrawUsed = "DrawUsed";

        private string PlayUsed = "PlayUsed";

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            IEnumerator coroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsRealCard && c.IsCharacter && c.IsInPlayAndHasGameText && c != this.CharacterCard, "character"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private int GetNumberOfDrawsThisTurn(Card nextTo)
        {
            return base.GameController.Game.Journal.QueryJournalEntries((DrawCardJournalEntry e) => e.Hero == nextTo.Owner).Where(base.GameController.Game.Journal.ThisTurn<DrawCardJournalEntry>()).Count();
        }

        private int GetNumberOfCardsEnteredPlayThisTurn(Card nextTo)
        {
            return base.GameController.Game.Journal.QueryJournalEntries((CardEntersPlayJournalEntry e) => e.Card.NativeDeck == nextTo.NativeDeck).Where(base.GameController.Game.Journal.ThisTurn<CardEntersPlayJournalEntry>()).Count();
        }

        public override void AddTriggers()
        {
            //Increase lightning damage dealt to {Friday} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Lightning, (DealDamageAction dd) => 1);

            //The first time each turn that character regains HP, {Friday} regains 1 hp.
            AddTrigger<GainHPAction>((GainHPAction hp) => GetCardThisCardIsNextTo() != null && hp.HpGainer == GetCardThisCardIsNextTo() && hp.AmountActuallyGained > 0 && !IsPropertyTrue(FirstTimeHPGain), GainHPResponse, TriggerType.GainHP, TriggerTiming.After);

            //The second time each turn...
            //} a card is drawn from that character's deck, you may draw a card.
            AddTrigger<DrawCardAction>((DrawCardAction dc) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner.IsPlayer && dc.HeroTurnTaker == GetCardThisCardIsNextTo().Owner && GetNumberOfDrawsThisTurn(GetCardThisCardIsNextTo()) == 2 && !IsPropertyTrue(DrawUsed), DrawResponse, TriggerType.DrawCard, TriggerTiming.After);

            //} a card from that character's deck enters play, you may play a card.
            //Hopefully useing NativeDeck here makes it work correctly in OblivAeon mode
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cep) => GetCardThisCardIsNextTo() != null && cep.CardEnteringPlay.NativeDeck == GetCardThisCardIsNextTo().NativeDeck && GetNumberOfCardsEnteredPlayThisTurn(GetCardThisCardIsNextTo()) == 2 && !IsPropertyTrue(PlayUsed), PlayResponse, TriggerType.PlayCard, TriggerTiming.After);

            //If the character this is next to leaves play, move this card to their play area. This should prevent this card from
            //getting sucked out of existence when a Scion is destroyed.
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(true, false);

            AddAfterLeavesPlayAction(ResetFlags, TriggerType.Hidden);
        }

        private IEnumerator GainHPResponse(GainHPAction hp)
        {
            IEnumerator coroutine = base.GameController.GainHP(this.CharacterCard, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SetCardPropertyToTrueIfRealAction(FirstTimeHPGain);
        }

        private IEnumerator DrawResponse(DrawCardAction dc)
        {
            IEnumerator coroutine = DrawCard(this.HeroTurnTaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SetCardPropertyToTrueIfRealAction(DrawUsed);
        }

        private IEnumerator PlayResponse(CardEntersPlayAction cep)
        {
            IEnumerator coroutine = SelectAndPlayCardFromHand(DecisionMaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SetCardPropertyToTrueIfRealAction(PlayUsed);
        }

        private IEnumerator ResetFlags(GameAction ga)
        {
            IEnumerator coroutine = ResetFlagAfterLeavesPlay(FirstTimeHPGain);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = ResetFlagAfterLeavesPlay(DrawUsed);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = ResetFlagAfterLeavesPlay(PlayUsed);
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

