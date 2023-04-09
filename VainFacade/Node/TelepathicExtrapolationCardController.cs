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
    public class TelepathicExtrapolationCardController : CardController
    {
        public TelepathicExtrapolationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {NodeCharacter} would deal 0 or more damage to a target, you may select a keyword. ..."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.IsSameCard(base.CharacterCard) && dda.CanDealDamage && !dda.IsPretend, GuessResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.ModifyDamageAmount }, TriggerTiming.Before);
        }

        private IEnumerator GuessResponse(DealDamageAction dda)
        {
            Location deck = GetNativeDeck(dda.Target);
            // "... you may select a keyword."
            IOrderedEnumerable<string> keywords = from s in deck.Cards.SelectMany((Card c) => base.GameController.GetAllKeywords(c)).Distinct() orderby s select s;
            string autofail = "Another keyword - always fails to match";
            keywords = keywords.Concat(autofail.ToEnumerable()).OrderBy((string s) => s);
            List<SelectWordDecision> choice = new List<SelectWordDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectWord(DecisionMaker, keywords, SelectionType.SelectKeyword, choice, true, associatedCards: dda.Target.ToEnumerable(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectWord(choice))
            {
                // "If you do, reveal and replace the top card of that deck."
                List<Card> revealed = new List<Card>();
                IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, deck, 1, revealed, revealedCardDisplay: RevealedCardDisplay.Message, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }
                List<Location> locations = new List<Location>();
                locations.Add(deck.OwnerTurnTaker.Revealed);
                IEnumerator replaceCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, locations, deck, cardsInList: revealed);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(replaceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(replaceCoroutine);
                }

                if (revealed.FirstOrDefault() != null && GetSelectedWord(choice) != autofail && revealed.FirstOrDefault().DoKeywordsContain(GetSelectedWord(choice)))
                {
                    // "If the revealed card shares the selected keyword, increase that damage by 2."
                    IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 2, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(increaseCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(increaseCoroutine);
                    }
                }
                else
                {
                    // "Otherwise, reduce that damage by 1."
                    IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, null, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(reduceCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(reduceCoroutine);
                    }
                }
            }
        }
    }
}
