using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class DialingInCardController : NodeUtilityCardController
    {
        public DialingInCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            switch (index)
            {
                case 0:
                    // "Reveal cards from the top of your deck until 2 Connections are revealed. Put 1 into your hand. Shuffle the rest into your deck."
                    int numRevealed = GetPowerNumeral(0, 2);
                    int numMoved = GetPowerNumeral(1, 1);
                    IEnumerator searchCoroutine = RevealCards_SelectSome_MoveThem_ReturnTheRest(DecisionMaker, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(ConnectionKeyword), numRevealed, numMoved, true, false, false, "Connection");
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(searchCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(searchCoroutine);
                    }
                    break;
                case 1:
                    // "Play a Connection."
                    IEnumerator playCoroutine = SelectAndPlayCardFromHand(DecisionMaker, false, cardCriteria: isConnection);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                    break;
            }
            yield break;
        }
    }
}
