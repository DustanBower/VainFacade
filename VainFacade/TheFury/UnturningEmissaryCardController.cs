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
    public class UnturningEmissaryCardController : TheFuryUtilityCardController
    {
        public UnturningEmissaryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "{TheFuryCharacter} is indestructible."
            if (card == base.CharacterCard)
            {
                return true;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {TheFuryCharacter} is dealt damage, discard a card or destroy this card. Then increase the next damage dealt to {TheFuryCharacter} by 1."
            AddTrigger((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.CharacterCard, DiscardDestroyDebuffResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroySelf, TriggerType.CreateStatusEffect }, TriggerTiming.After);
            // "At the end of your turn, if {TheFuryCharacter} has 1 or more HP, you may draw a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DrawCard(base.TurnTaker.ToHero()), TriggerType.DrawCard, additionalCriteria: (PhaseChangeAction pca) => base.CharacterCard.IsTarget && base.CharacterCard.HitPoints >= 1);
            // [At the end of your turn], "If you did not play a card this turn, discard a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => SelectAndDiscardCards(DecisionMaker, 1, requiredDecisions: 1, responsibleTurnTaker: base.TurnTaker), TriggerType.DiscardCard, additionalCriteria: (PhaseChangeAction pca) => !base.Game.Journal.PlayCardEntriesThisTurn().Where((PlayCardJournalEntry pcje) => pcje.ResponsibleTurnTaker == base.TurnTaker).Any());
        }

        private IEnumerator DiscardDestroyDebuffResponse(DealDamageAction dda)
        {
            // "... discard a card or destroy this card."
            IEnumerable<Function> options = new Function[2]
            {
                new Function(DecisionMaker, "Discard a card", SelectionType.DiscardCard, () => SelectAndDiscardCards(DecisionMaker, 1, requiredDecisions: 1, responsibleTurnTaker: base.TurnTaker), onlyDisplayIfTrue: base.TurnTaker.ToHero().HasCardsInHand, forcedActionMessage: base.Card.Title + " cannot be destroyed, so " + base.TurnTaker.Name + " must discard a card."),
                new Function(DecisionMaker, "Destroy " + base.Card.Title, SelectionType.DestroySelf, () => DestroyThisCardResponse(dda), forcedActionMessage: base.TurnTaker.Name + " cannot discard any cards, so they must destroy " + base.Card.Title + ".")
            };
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, false, noSelectableFunctionMessage: base.TurnTaker.Name + " cannot discard any cards or destroy " + base.Card.Title + ".", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            // "Then increase the next damage dealt to {TheFuryCharacter} by 1."
            IEnumerator debuffCoroutine = IncreaseNextDamageTo(base.CharacterCard, 1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(debuffCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(debuffCoroutine);
            }
        }
    }
}
