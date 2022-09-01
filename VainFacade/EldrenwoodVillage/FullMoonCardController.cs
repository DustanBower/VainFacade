using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class FullMoonCardController : EldrenwoodUtilityCardController
    {
        public FullMoonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (card == base.Card)
            {
                // "This card is indestructible while there is at least 1 card under it."
                return base.Card.UnderLocation.HasCards;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the environment turn, discard 1 card from under this card. Then each Werewolf regains 1 HP. Then destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardHealDestructResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.GainHP, TriggerType.DestroySelf });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, put the top card of 3 hero decks under this card."
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame && tt.Deck.HasCards, "heroes with cards in their decks"), SelectionType.MoveCardToUnderCard, (TurnTaker tt) => base.GameController.MoveCard(base.TurnTakerController, tt.Deck.TopCard, base.Card.UnderLocation, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), 3, requiredDecisions: 3, allowAutoDecide: true, numberOfCards: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        IEnumerator DiscardHealDestructResponse(PhaseChangeAction pca)
        {
            // "... discard 1 card from under this card."
            List<SelectCardDecision> selected = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardToTrash, (from c in FindCardsWhere((Card c) => c.Location == base.Card.UnderLocation) orderby c.Owner.Name select c), selected, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectCardDecision choice = selected.Where((SelectCardDecision scd) => scd.Completed && scd.SelectedCard != null).FirstOrDefault();
            if (choice != null)
            {
                MoveCardDestination trashDestination = FindCardController(choice.SelectedCard).GetTrashDestination();
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, choice.SelectedCard, trashDestination.Location, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            // "Then each Werewolf regains 1 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => base.GameController.DoesCardContainKeyword(c, WerewolfKeyword), 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "Then destroy this card."
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
}
