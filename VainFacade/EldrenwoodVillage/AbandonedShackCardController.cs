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
    public class AbandonedShackCardController : CardController
    {
        public AbandonedShackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, each player may shuffle their hand into their deck and draw 2 cards. Reduce damage dealt to heroes that do so by 2 until the start of the next environment turn."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SelectPlayersResponse, new TriggerType[] { TriggerType.ShuffleCardIntoDeck, TriggerType.DrawCard, TriggerType.CreateStatusEffect });
            // "At the start of the environment turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        private IEnumerator SelectPlayersResponse(PhaseChangeAction pca)
        {
            // "... each player may shuffle their hand into their deck and draw 2 cards. Reduce damage dealt to heroes that do so by 2 until the start of the next environment turn."
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame), SelectionType.Custom, ShuffleHandDrawReduceDamageResponse, requiredDecisions: 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator ShuffleHandDrawReduceDamageResponse(TurnTaker tt)
        {
            // "... shuffle their hand into their deck..."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleCardsIntoLocation(GameController.FindHeroTurnTakerController(tt.ToHero()), tt.ToHero().Hand.Cards, tt.Deck, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            // "... and draw 2 cards."
            IEnumerator drawCoroutine = DrawCards(GameController.FindHeroTurnTakerController(tt.ToHero()), 2);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Reduce damage dealt to heroes that do so by 2 until the start of the next environment turn."
            ReduceDamageStatusEffect wall = new ReduceDamageStatusEffect(2);
            wall.TargetCriteria.IsHeroCharacterCard = true;
            wall.TargetCriteria.OwnedBy = tt;
            if (tt.HasMultipleCharacterCards)
            {
                wall.TargetCriteria.OutputString = tt.Name + "'s hero character cards";
                if (tt.DeckDefinition.IsPlural)
                {
                    wall.TargetCriteria.OutputString = tt.Name + "' hero character cards";
                }
            }
            else
            {
                wall.TargetCriteria.OutputString = tt.CharacterCard.Title;
            }
            wall.UntilStartOfNextTurn(base.TurnTaker);
            IEnumerator statusCoroutine = AddStatusEffect(wall);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a player to shuffle their hand into their deck and draw 2 cards", "choosing a player to shuffle their hand into their deck and draw 2 cards", "Vote for a player to shuffle their hand into their deck and draw 2 cards", "player to shuffle their hand into their deck and draw 2 cards");
        }
    }
}
