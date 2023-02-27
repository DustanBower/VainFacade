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
            // Show list of Connections in play
            SpecialStringMaker.ShowListOfCardsInPlay(isConnection);
        }

        public override IEnumerator Play()
        {
            // "You may play a card."
            IEnumerator freePlayCoroutine = SelectAndPlayCardFromHand(DecisionMaker, associateCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(freePlayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(freePlayCoroutine);
            }
            // "Return any number of Connections in play to your hand."
            List<MoveCardAction> storedMoves = new List<MoveCardAction>();
            IEnumerator returnCoroutine = base.GameController.SelectAndReturnCards(DecisionMaker, null, isConnection, true, false, false, 0, storedMoves, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            // "For each Connection returned this way, you may draw a card, then you may play a card."
            int numberMoved = GetNumberOfCardsMoved(storedMoves);
            for (int i = 0; i < numberMoved; i++)
            {
                IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker, optional: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
                IEnumerator playCoroutine = SelectAndPlayCardFromHand(DecisionMaker, associateCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            // "Increase the next damage dealt to {NodeCharacter} by X, where X is the number of hero cards played since this card entered play."
            PlayCardJournalEntry thisWasPlayed = base.GameController.Game.Journal.QueryJournalEntries((PlayCardJournalEntry e) => e.CardPlayed == base.Card).LastOrDefault();
            if (thisWasPlayed != null)
            {
                int? thisIndex = base.GameController.Game.Journal.GetEntryIndex(thisWasPlayed);
                if (thisIndex.HasValue)
                {
                    int numberHeroCardsPlayedSince = (from pcje in base.Journal.PlayCardEntries() where pcje.CardPlayed.IsHero && base.GameController.Game.Journal.GetEntryIndex(pcje).HasValue && base.GameController.Game.Journal.GetEntryIndex(pcje).Value > thisIndex.Value select pcje).Count();
                    IncreaseDamageStatusEffect vulnerability = new IncreaseDamageStatusEffect(numberHeroCardsPlayedSince);
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
                }
            }
        }
    }
}
