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
    public class BleedingEdgeCardController : NodeUtilityCardController
    {
        public BleedingEdgeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Connected character cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsCharacter && IsConnected(c), "Connected character"));
            // Show list of Connections in play
            SpecialStringMaker.ShowListOfCardsInPlay(isConnection);
        }

        private bool isSelectingPlayArea = false;

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (isSelectingPlayArea)
            {
                return new CustomDecisionText("Select a play area to play a Connection in", base.TurnTaker.Name + " is selecting a play area to play a Connection in", "Vote for a play area for " + base.TurnTaker.Name + " to play a Connection in", "play area to play a Connection in");
            }
            else
            {
                return new CustomDecisionText("Do you want to increase the next damage dealt to " + base.CharacterCard.Title + " by 1?", base.TurnTaker.Name + " is deciding whether to increase the next damage dealt to " + base.CharacterCard.Title + " by 1", "Vote for whether to increase the next damage dealt to " + base.CharacterCard.Title + " by 1", "whether to increase the next damage dealt to " + base.CharacterCard.Title + " by 1");
            }
        }

        public override IEnumerator Play()
        {
            IEnumerator startCoroutine = Continue();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(startCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(startCoroutine);
            }
            yield break;
        }

        public IEnumerator Continue()
        {
            // "You may play a Connection in a [i]Connected[/i] character's play area."
            HeroTurnTaker player = base.TurnTaker.ToHero();
            if (player != null && player.Hand.Cards.Where((Card c) => isConnector.Criteria(c)).Count() > 0)
            {
                List<Card> connectedChars = FindCardsWhere(new LinqCardCriteria((Card c) => IsConnected(c) && c.IsCharacter)).ToList();
                List<Location> connectedLocations = (from c in connectedChars where c.Location.HighestRecursiveLocation.IsPlayArea select c.Location.HighestRecursiveLocation).Distinct().ToList();
                if (connectedLocations.Count > 0)
                {
                    // Choose a Connected play area to play a Connection in
                    List<SelectLocationDecision> choices = new List<SelectLocationDecision>();
                    List<LocationChoice> options = new List<LocationChoice>();
                    foreach(Location l in connectedLocations)
                    {
                        options.Add(new LocationChoice(l));
                    }
                    isSelectingPlayArea = true;
                    IEnumerator locateCoroutine = base.GameController.SelectLocation(DecisionMaker, options, SelectionType.Custom, choices, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(locateCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(locateCoroutine);
                    }
                    if (DidSelectLocation(choices))
                    {
                        Location chosenDest = GetSelectedLocation(choices);

                        // Choose a Connection from hand and play it in chosenDest
                        List<Card> connectOptions = player.Hand.Cards.Where((Card c) => isConnector.Criteria(c)).ToList();
                        IEnumerator selectConnectionHandCoroutine = base.GameController.SelectCardAndDoAction(new SelectCardDecision(base.GameController, DecisionMaker, SelectionType.PlayCard, connectOptions, cardSource: GetCardSource()), (SelectCardDecision scd) => base.GameController.PlayCard(base.TurnTakerController, scd.SelectedCard, overridePlayLocation: chosenDest, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()));
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(selectConnectionHandCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(selectConnectionHandCoroutine);
                        }
                    }
                }
                else
                {
                    IEnumerator noLocationsCoroutine = base.GameController.SendMessageAction("There are no play areas with Connected characters to play a Connection in.", Priority.High, GetCardSource(), showCardSource: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(noLocationsCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(noLocationsCoroutine);
                    }
                }
            }
            else
            {
                IEnumerator noConnectionsHandCoroutine = base.GameController.SendMessageAction(base.TurnTaker.Name + " has no Connection cards in their hand to play.", Priority.High, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(noConnectionsHandCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(noConnectionsHandCoroutine);
                }
            }
            // "You may return any number of Connections to your hand."
            List<MoveCardAction> returned = new List<MoveCardAction>();
            IEnumerator returnCoroutine = base.GameController.SelectAndReturnCards(DecisionMaker, null, isConnectionInPlay, true, false, true, 0, returned, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            // "Draw a card for each Connection returned this way."
            int numberMoved = GetNumberOfCardsMoved(returned);
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, numberMoved, false, false);
            if (numberMoved <= 0)
            {
                drawCoroutine = base.GameController.SendMessageAction("No Connections were returned, so " + base.TurnTaker.Name + " does not draw any cards.", Priority.Medium, cardSource: GetCardSource(), showCardSource: true);
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "You may increase the next damage dealt to {NodeCharacter} by 1."
            isSelectingPlayArea = false;
            YesNoDecision damageChoice = new YesNoDecision(base.GameController, DecisionMaker, SelectionType.Custom, associatedCards: base.CharacterCard.ToEnumerable(), cardSource: GetCardSource());
            IEnumerator increaseChoiceCoroutine = base.GameController.MakeDecisionAction(damageChoice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseChoiceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseChoiceCoroutine);
            }
            if (DidPlayerAnswerYes(damageChoice))
            {
                IncreaseDamageStatusEffect vulnerability = new IncreaseDamageStatusEffect(1);
                vulnerability.TargetCriteria.IsSpecificCard = base.CharacterCard;
                vulnerability.UntilTargetLeavesPlay(base.CharacterCard);
                vulnerability.NumberOfUses = 1;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(vulnerability, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
                // "If you do, repeat the entire game text of this card."
                IEnumerator repeatCoroutine = Continue();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(repeatCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(repeatCoroutine);
                }
            }
            yield break;
        }
    }
}
