using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class DreamsOfFaerieCardController : CardController
    {
        public DreamsOfFaerieCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Ongoing cards in Ember's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsOngoing, "Ongoing"));
        }

        public override IEnumerator Play()
        {
            // "Draw a card."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "{EmberCharacter} deals herself 2 psychic damage."
            IEnumerator psychicCoroutine = DealDamage(base.CharacterCard, base.CharacterCard, 2, DamageType.Psychic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(psychicCoroutine);
            }
            // "Reveal cards from the top of your deck until 3 Ongoings are revealed. Put 1 into your hand. Shuffle the other revealed cards back into your deck."
            IEnumerator revealCoroutine = RevealCards_SelectSome_MoveThem_ReturnTheRest(DecisionMaker, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.IsOngoing, 3, 1, true, false, false, "Ongoing");
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
        }
    }
}
