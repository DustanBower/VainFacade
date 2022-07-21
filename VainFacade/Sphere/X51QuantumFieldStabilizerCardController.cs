using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class X51QuantumFieldStabilizerCardController : SphereUtilityCardController
    {
        public X51QuantumFieldStabilizerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Emanations in Sphere's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, isEmanation);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Reveal cards from the top of your deck until an Emanation is revealed. Put it into your hand or into play. Discard the other revealed cards."
            IEnumerator moveCoroutine = RevealCards_SelectSome_MoveThem_DiscardTheRest(base.HeroTurnTakerController, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(emanationKeyword), 1, 1, true, true, true, emanationKeyword, base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield break;
        }
    }
}
