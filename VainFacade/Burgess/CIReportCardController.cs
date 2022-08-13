using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class CIReportCardController : BurgessUtilityCardController
    {
        public CIReportCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Clue cards in Burgess's trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain(ClueKeyword), "Clue"));
            // Show number of Clue cards in Burgess's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain(ClueKeyword), "Clue"));
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, draw 2 cards."
            IEnumerator drawCoroutine = DrawCards(base.HeroTurnTakerController, 2);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            switch (index)
            {
                case 0:
                    {
                        // "Put a Clue into play from your trash."
                        MoveCardDestination[] intoPlay = new MoveCardDestination[1] { new MoveCardDestination(base.TurnTaker.PlayArea) };
                        IEnumerator putCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, base.TurnTaker.Trash, ClueCard, intoPlay, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(putCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(putCoroutine);
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
                        break;
                    }
                case 1:
                    {
                        // "Reveal cards from the top of your deck until a Clue is revealed. Put it into your hand or into play. Discard the remaining cards."
                        IEnumerator revealCoroutine = RevealCards_SelectSome_MoveThem_DiscardTheRest(base.HeroTurnTakerController, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(ClueKeyword), 1, 1, true, true, true, "Clue", responsibleTurnTaker: base.TurnTaker);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(revealCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(revealCoroutine);
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
                        break;
                    }
            }
        }
    }
}
