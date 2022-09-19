using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class ObstacleToProgressCardController : CardController
    {
        public ObstacleToProgressCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            AllowFastCoroutinesDuringPretend = false;
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible while a target is next to it."
            if (card == base.Card)
            {
                return base.Card.Location.IsNextToCard && GetCardThisCardIsNextTo().IsTarget;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        protected const string FirstDamage = "FirstDamageEachTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn each villain card would deal damage, redirect that damage to the target next to this card. Any player may discard 1 card to redirect that damage to one of their targets instead."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsVillain && !IsPropertyTrue(GeneratePerTargetKey(FirstDamage, dda.DamageSource.Card)), RedirectDamageResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            ResetFlagsAfterLeavesPlay(FirstDamage);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to the hero with the highest HP."
            List<Card> found = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHeroCharacterCard, found, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card target = found.FirstOrDefault();
            if (target != null)
            {
                storedResults.Add(new MoveCardDestination(target.NextToLocation));
            }
        }

        private IEnumerator RedirectDamageResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(GeneratePerTargetKey(FirstDamage, dda.DamageSource.Card));
            // "... redirect that damage to the target next to this card."
            IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, GetCardThisCardIsNextTo(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            // "Any player may discard 1 card to redirect that damage to one of their targets instead."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            List<SelectTurnTakerDecision> players = new List<SelectTurnTakerDecision>();
            SelectionType displayType = SelectionType.DiscardCard;
            if (!dda.IsRedirectable)
            {
                displayType = SelectionType.DiscardCardsNoRedirect;
            }
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalSelectHero: true, optionalDiscardCard: true, storedResultsTurnTaker: players, storedResultsDiscard: discards, gameAction: dda, selectionType: displayType, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            IEnumerator nextCoroutine = null;
            if (DidDiscardCards(discards))
            {
                Log.Debug("ObstacleToProgressCardController.RedirectDamageResponse: card was discarded");
                if (dda.IsRedirectable)
                {
                    HeroTurnTakerController httc = discards.FirstOrDefault().HeroTurnTakerController;
                    CardSource heroCharacter;
                    if (httc.HasMultipleCharacterCards)
                    {
                        heroCharacter = FindCardController(httc.CharacterCards.First()).GetCardSource();
                    }
                    else
                    {
                        heroCharacter = httc.CharacterCardController.GetCardSource();
                    }
                    nextCoroutine = base.GameController.SelectTargetAndRedirectDamage(httc, (Card c) => c.IsTarget && c.Owner == httc.TurnTaker, dda, cardSource: GetCardSource());
                }
                else
                {
                    nextCoroutine = base.GameController.SendMessageAction(discards.First().HeroTurnTakerController.Name + " discarded a card, but the damage can't be redirected.", Priority.High, GetCardSource(), showCardSource: true);
                }
            }
            else
            {
                Log.Debug("ObstacleToProgressCardController.RedirectDamageResponse: no card was discarded");
                if (DidSelectTurnTaker(players))
                {
                    TurnTaker selected = players.FirstOrDefault((SelectTurnTakerDecision sttd) => DidSelectTurnTaker(sttd.ToEnumerable())).SelectedTurnTaker;
                    nextCoroutine = base.GameController.SendMessageAction(selected.Name + " did not discard a card, so the damage is not redirected.", Priority.High, GetCardSource(), showCardSource: true);
                }
            }
            if (nextCoroutine != null)
            {
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(nextCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(nextCoroutine);
                }
            }
        }
    }
}
