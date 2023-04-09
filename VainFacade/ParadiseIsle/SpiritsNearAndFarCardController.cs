using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class SpiritsNearAndFarCardController : CardController
    {
        public SpiritsNearAndFarCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddAsPowerContributor();
            // Show number of cards in each hero trash
            SpecialStringMaker.ShowNumberOfCardsAtLocations(() => from httc in base.GameController.FindHeroTurnTakerControllers()
                                                                  where !httc.IsIncapacitatedOrOutOfGame
                                                                  select httc.TurnTaker.Trash);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, you may move this card next to a hero."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, MoveResponse, TriggerType.MoveCard);
        }

        private IEnumerator MoveResponse(PhaseChangeAction pca)
        {
            // "... you may move this card next to a hero."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardNextToCard, new LinqCardCriteria((Card c) => IsHeroCharacterCard(c), "hero character"), choices, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Card chosen = GetSelectedCard(choices);
            if (chosen != null)
            {
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, chosen.NextToLocation, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, doesNotEnterPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            // "That hero gains the following power:"
            if (cardController.HeroTurnTakerController != null && IsHeroCharacterCard(cardController.Card) && IsHero(cardController.Card.Owner) && !cardController.Card.Owner.IsIncapacitatedOrOutOfGame && ! cardController.Card.IsFlipped && cardController.Card == GetCardThisCardIsNextTo())
            {
                Power drinking = new Power(cardController.HeroTurnTakerController, cardController, "Put a card from your trash into your hand. Destroy [i]" + base.Card.Title + "[/i].", ReturnDestructResponse(cardController.HeroTurnTakerController), 0, null, GetCardSource());
                return new Power[] { drinking };
            }
            return null;
        }

        public IEnumerator ReturnDestructResponse(HeroTurnTakerController httc)
        {
            // "Put a card from your trash into your hand."
            IEnumerator returnCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(httc, httc.TurnTaker.Trash, new LinqCardCriteria((Card c) => true), (new MoveCardDestination(httc.HeroTurnTaker.Hand)).ToEnumerable(), responsibleTurnTaker: httc.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            // "Destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(null);
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
