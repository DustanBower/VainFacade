using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class ManWithAPlanCardController : CarnavalUtilityCardController
    {
        public ManWithAPlanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Trap cards in Carnaval's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, TrapCard);
            // Show list of Trap cards in Carnaval's hand
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.ToHero().Hand, TrapCard);
        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the top of your deck until 2 Traps are revealed. Put them into your hand. Shuffle the other revealed cards into your deck."
            IEnumerator revealCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, false, false, true, TrapCard, 2, revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "You may play a Trap."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(DecisionMaker, cardCriteria: TrapCard);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
