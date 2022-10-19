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
    public class AbyssalGazeCardController : TheFuryUtilityCardController
    {
        public AbyssalGazeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.EnteringGameCheck);
            AddThisCardControllerToList(CardControllerListType.ReplacesCardSource);
            AddInhibitorException((GameAction ga) => ga is PlayCardAction && base.Card.Location.IsHand);
        }

        public override void AddStartOfGameTriggers()
        {
            base.AddStartOfGameTriggers();
            // "When this card would be revealed or enter your hand, put it into play."
            AddTrigger((DrawCardAction dca) => dca.CardToDraw == base.Card, PutIntoPlayInsteadResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.Hidden }, TriggerTiming.Before, outOfPlayTrigger: true);
            AddTrigger((MoveCardAction mca) => mca.CardToMove == base.Card && (mca.Destination == base.TurnTaker.ToHero().Hand || mca.Destination.IsRevealed), PutIntoPlayInsteadResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.Hidden }, TriggerTiming.Before, outOfPlayTrigger: true);
            // trigger on RevealCardsAction
            AddTrigger((RevealCardsAction rca) => rca.RevealedCards.Contains(base.Card), PutIntoPlayInsteadResponse, TriggerType.PutIntoPlay, TriggerTiming.Before, outOfPlayTrigger: true);
            // trigger on BulkMoveCardsAction?
            // Shinobi Assassin doesn't, Monster of Id doesn't
            // ...
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Damage dealt by {TheFuryCharacter} is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard);
            // "At the start of your turn, select up to 3 targets other than {TheFuryCharacter}. Increase the next damage dealt to each of the selected targets by 1. If none of the selected targets are hero character cards, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SelectIncreaseDestroyResponse, new TriggerType[] { TriggerType.CreateStatusEffect, TriggerType.DestroySelf });
        }

        private IEnumerator PutIntoPlayInsteadResponse(GameAction ga = null)
        {
            // "...  put it into play."
            string message = base.Card.Title + " puts itself into play.";
            if (ga != null)
            {
                if (ga is DrawCardAction)
                {
                    message = base.Card.Title + " would be drawn, so it puts itself into play instead.";
                }
                else if (ga is MoveCardAction mca)
                {
                    message = base.Card.Title + " would be moved to " + mca.Destination.GetFriendlyName() + ", so it puts itself into play instead.";
                }
                IEnumerator cancelCoroutine = base.GameController.CancelAction(ga);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
            }

            if (ga is RevealCardsAction)
            {
                RevealCardsAction revealed = ga as RevealCardsAction;
                revealed.RemoveCardFromRevealedCards(base.Card);
                if (revealed.RevealCardsUntil == null)
                {
                    List<Card> storedResults = new List<Card>();
                    IEnumerator coroutine = base.GameController.RevealCards(base.TurnTakerController, revealed.SearchLocation, 1, storedResults, revealed.FromBottom, RevealedCardDisplay.None, null, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    foreach (Card item in storedResults)
                    {
                        revealed.AddCardToRevealedCards(item);
                    }
                }
            }

            IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.High, GetCardSource(), null, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }

            IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, base.Card, isPutIntoPlay: true, null, optional: false, null, null, evenIfAlreadyInPlay: false, null, null, null, associateCardSource: false, fromBottom: false, canBeCancelled: true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        public override IEnumerator PerformEnteringGameResponse()
        {
            // At start of game, if this card is in hand, put it into play
            IEnumerator coroutine = base.PerformEnteringGameResponse();
            if (base.Card.Location.IsHand)
            {
                coroutine = PutIntoPlayInsteadResponse();
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator SelectIncreaseDestroyResponse(PhaseChangeAction pca)
        {
            // "... select up to 3 targets other than {TheFuryCharacter}. Increase the next damage dealt to each of the selected targets by 1."
            List<Card> selectedTargets = new List<Card>();
            for (int i = 0; i < 3; i++)
            {
                List<SelectCardDecision> choices = new List<SelectCardDecision>();
                IEnumerator selectCoroutine = SelectTargetAndIncreaseNextDamageTo(new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && !selectedTargets.Contains(c) && c != base.CharacterCard, "targets in play", false), 1, true, GetCardSource(), choices);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                Card selected = GetSelectedCard(choices);
                if (selected == null)
                {
                    break;
                }
                else
                {
                    selectedTargets.Add(selected);
                }
            }
            // "If none of the selected targets are hero character cards, destroy this card."
            bool selectedHero = false;
            foreach (Card selected in selectedTargets)
            {
                if (selected.IsHeroCharacterCard)
                {
                    selectedHero = true;
                }
            }
            if (!selectedHero)
            {
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
}
