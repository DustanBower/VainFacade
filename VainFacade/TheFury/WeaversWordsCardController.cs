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
    public class WeaversWordsCardController : TheFuryUtilityCardController
    {
        public WeaversWordsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            switch(index)
            {
                case 0:
                    // "1 target regains 2 HP."
                    int numTargets = GetPowerNumeral(0, 1);
                    int amtGained = GetPowerNumeral(1, 2);
                    IEnumerator healCoroutine = base.GameController.SelectAndGainHP(DecisionMaker, amtGained, numberOfTargets: numTargets, requiredDecisions: numTargets, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(healCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(healCoroutine);
                    }
                    break;
                case 1:
                    // "Reduce the next damage dealt to {TheFuryCharacter} by 7."
                    int amtReduce = GetPowerNumeral(0, 7);
                    IEnumerator statusCoroutine = ReduceNextDamageTo(base.CharacterCard, amtReduce, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(statusCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(statusCoroutine);
                    }
                    // "Play the top card of the villain deck."
                    IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckWithMessageResponse(null);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                    break;
                case 2:
                    // "Shuffle a trash."
                    List<SelectLocationDecision> locationChoices = new List<SelectLocationDecision>();
                    IEnumerator chooseCoroutine = base.GameController.SelectATrash(DecisionMaker, SelectionType.ShuffleDeck, (Location l) => base.GameController.IsLocationVisibleToSource(l, GetCardSource()), locationChoices, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(chooseCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(chooseCoroutine);
                    }
                    if (DidSelectLocation(locationChoices))
                    {
                        Location chosen = GetSelectedLocation(locationChoices);
                        if (chosen != null)
                        {
                            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(chosen, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(shuffleCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(shuffleCoroutine);
                            }
                            // "If the top card of that trash is not a One-Shot, play it."
                            if (chosen.HasCards)
                            {
                                Card top = chosen.TopCard;
                                if (top.IsOneShot)
                                {
                                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("The top card of " + chosen.GetFriendlyName() + " is a One-Shot.", Priority.High, GetCardSource(), associatedCards: top.ToEnumerable());
                                    if (base.UseUnityCoroutines)
                                    {
                                        yield return base.GameController.StartCoroutine(messageCoroutine);
                                    }
                                    else
                                    {
                                        base.GameController.ExhaustCoroutine(messageCoroutine);
                                    }
                                }
                                else
                                {
                                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("The top card of " + chosen.GetFriendlyName() + " is not a One-Shot, so " + base.Card.Title + " plays it!", Priority.High, GetCardSource(), associatedCards: top.ToEnumerable());
                                    if (base.UseUnityCoroutines)
                                    {
                                        yield return base.GameController.StartCoroutine(messageCoroutine);
                                    }
                                    else
                                    {
                                        base.GameController.ExhaustCoroutine(messageCoroutine);
                                    }
                                    IEnumerator replayCoroutine = base.GameController.PlayCard(base.TurnTakerController, top, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                                    if (base.UseUnityCoroutines)
                                    {
                                        yield return base.GameController.StartCoroutine(replayCoroutine);
                                    }
                                    else
                                    {
                                        base.GameController.ExhaustCoroutine(replayCoroutine);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            yield break;
        }
    }
}
