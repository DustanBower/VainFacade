using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class OpenLineCardController : ConnectionCardController
    {
        public OpenLineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override bool IsValidPlayArea(TurnTaker tt)
        {
            // "Play this card in another hero's play area."
            return tt.IsHero && tt != base.TurnTaker;
        }

        private bool ReplacingActionByNode = false;
        private bool ActionToReplaceIsDraw = false;

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string otherPlayers = "";
            string repeated = "whether ";
            if (ReplacingActionByNode)
            {
                otherPlayers += base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Name;
                repeated += base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Name;
            }
            else
            {
                otherPlayers += base.TurnTaker.Name;
                repeated += base.TurnTaker.Name;
            }
            string q = "Do you want to ";
            otherPlayers += " is deciding whether to ";
            repeated += " should ";
            if (ActionToReplaceIsDraw)
            {
                q += "draw";
                otherPlayers += "draw";
                repeated += "draw";
            }
            else
            {
                q += "play";
                otherPlayers += "play";
                repeated += "play";
            }
            q += " a card instead of ";
            repeated += " a card instead of ";
            otherPlayers += " a card instead of ";
            if (ReplacingActionByNode)
            {
                q += base.TurnTaker.Name + "?";
                otherPlayers += base.TurnTaker.Name;
                repeated += base.TurnTaker.Name;
            }
            else
            {
                q += base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Name + "?";
                otherPlayers += base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Name;
                repeated += base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Name;
            }
            string votingPlayers = "Vote for " + repeated;

            return new CustomDecisionText(q, otherPlayers, votingPlayers, repeated);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When you or that play area's owner would draw or play a card, the other may do so instead."
            AddTrigger((DrawCardAction dca) => base.TurnTaker.IsHero && base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.IsHero && (dca.HeroTurnTaker == base.TurnTaker.ToHero() || dca.HeroTurnTaker == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero()), OfferDrawExchangeResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.DrawCard }, TriggerTiming.Before);
            AddTrigger((PlayCardAction pca) => base.TurnTaker.IsHero && base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.IsHero && (pca.ResponsibleTurnTaker == base.TurnTaker || pca.ResponsibleTurnTaker == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker) && !pca.IsPutIntoPlay, OfferPlayExchangeResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.PlayCard }, TriggerTiming.Before);
        }

        private IEnumerator OfferDrawExchangeResponse(DrawCardAction dca)
        {
            ActionToReplaceIsDraw = true;
            HeroTurnTaker currentlyDrawing = dca.HeroTurnTaker;
            ReplacingActionByNode = false;
            HeroTurnTaker couldDrawInstead = base.TurnTaker.ToHero();
            if (currentlyDrawing == base.TurnTaker.ToHero())
            {
                ReplacingActionByNode = true;
                couldDrawInstead = base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero();
            }
            if (!ReplacingActionByNode || !couldDrawInstead.IsIncapacitatedOrOutOfGame)
            {
                YesNoDecision choice = new YesNoDecision(base.GameController, FindHeroTurnTakerController(couldDrawInstead), SelectionType.Custom, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (DidPlayerAnswerYes(choice))
                {
                    IEnumerator cancelCoroutine = CancelAction(dca);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(cancelCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(cancelCoroutine);
                    }
                    IEnumerator drawCoroutine = DrawCard(couldDrawInstead);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(drawCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(drawCoroutine);
                    }
                }
            }
        }

        private IEnumerator OfferPlayExchangeResponse(PlayCardAction pca)
        {
            ActionToReplaceIsDraw = false;
            HeroTurnTaker currentlyPlaying = pca.ResponsibleTurnTaker.ToHero();
            ReplacingActionByNode = false;
            HeroTurnTaker couldPlayInstead = base.TurnTaker.ToHero();
            if (currentlyPlaying == base.TurnTaker.ToHero())
            {
                ReplacingActionByNode = true;
                couldPlayInstead = base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero();
            }
            if (!ReplacingActionByNode || !couldPlayInstead.IsIncapacitatedOrOutOfGame)
            {
                YesNoDecision choice = new YesNoDecision(base.GameController, FindHeroTurnTakerController(couldPlayInstead), SelectionType.Custom, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (DidPlayerAnswerYes(choice))
                {
                    IEnumerator cancelCoroutine = CancelAction(pca);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(cancelCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(cancelCoroutine);
                    }
                    IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.GameController.FindHeroTurnTakerController(couldPlayInstead), optional: false, associateCardSource: true);
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

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> destination, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in another hero's play area."
            // Adapted from SergeantSteelTeam.MissionObjectiveCardController
            List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, storedResults, optional: false, allowAutoDecide: false, (TurnTaker tt) => tt.IsHero && tt != base.TurnTaker, null, null, checkExtraTurnTakersInstead: false, canBeCancelled: true, ignoreBattleZone: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SelectTurnTakerDecision selectTurnTakerDecision = storedResults.FirstOrDefault();
            if (selectTurnTakerDecision != null && selectTurnTakerDecision.SelectedTurnTaker != null && destination != null)
            {
                destination.Add(new MoveCardDestination(selectTurnTakerDecision.SelectedTurnTaker.PlayArea, toBottom: false, showMessage: true));
                yield break;
            }
            coroutine = base.GameController.SendMessageAction("No viable play locations. Putting this card in the trash", Priority.Low, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            destination.Add(new MoveCardDestination(base.TurnTaker.Trash, toBottom: false, showMessage: true));
        }
    }
}
