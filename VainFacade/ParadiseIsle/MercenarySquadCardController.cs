using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class MercenarySquadCardController : ParadiseIsleUtilityCardController
    {
        public MercenarySquadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show the non-Conspirator target with the second highest HP
            SpecialStringMaker.ShowHighestHP(2, cardCriteria: new LinqCardCriteria((Card c) => !c.DoKeywordsContain(ConspiratorKeyword), "non-Conspirator", singular: "target", plural: "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card would be dealt damage, discard a card beneath it. If you do, prevent that damage."
            AddTrigger((DealDamageAction dda) => dda.Target == base.Card && dda.Amount > 0 && base.Card.UnderLocation.Cards.Count() > 0, (DealDamageAction dda) => DiscardUnderCardToPreventDamage(base.Card, dda), new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.MoveCard }, TriggerTiming.Before);
            // "At the start of the environment turn, this card deals the non-Conspirator target with the second highest HP X projectile damage, where X = 2 plus the number of cards beneath this one."
            AddDealDamageAtStartOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => !c.DoKeywordsContain(ConspiratorKeyword), TargetType.HighestHP, 2, DamageType.Projectile, highestLowestRanking: 2, dynamicAmount: (Card c) => 2 + base.Card.UnderLocation.Cards.Count());
            // When this leaves play, move cards under it to its trash
            AddBeforeLeavesPlayActions(MoveCardsUnderThisCardToTrash);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, place the top {H} cards of the environment deck beneath it."
            IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.TurnTaker.Deck.GetTopCards(H), base.Card.UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        private IEnumerator MoveCardsUnderThisCardToTrash(GameAction ga)
        {
            IEnumerator coroutine = base.GameController.MoveCards(base.TurnTakerController, base.Card.UnderLocation.Cards, base.TurnTaker.Trash, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, null, showIndividualMessages: false, isDiscard: false, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public IEnumerator DiscardUnderCardToPreventDamage(Card card, DealDamageAction dd)
        {
            if (FindCardsWhere((Card c) => c.Location == card.UnderLocation).Any((Card c) => DoesAnyCardControllerMakeAnotherCardIndestructible(c) == null))
            {
                if (IsRealAction(dd))
                {
                    List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                    List<MoveCardAction> storedResultsMove = new List<MoveCardAction>();
                    IEnumerable<Card> cardList = from c in FindCardsWhere((Card c) => c.Location == card.UnderLocation)
                                                 orderby c.Owner.Name
                                                 select c;
                    IEnumerator coroutine = GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.DiscardCard, cardList, storedResults, optional: false, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: true);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    if (storedResults.Count() <= 0)
                    {
                        yield break;
                    }
                    SelectCardDecision selectCardDecision = storedResults.Where((SelectCardDecision d) => d.Completed && d.SelectedCard != null).FirstOrDefault();
                    if (selectCardDecision == null)
                    {
                        yield break;
                    }
                    MoveCardDestination trashDestination = FindCardController(selectCardDecision.SelectedCard).GetTrashDestination();
                    coroutine = GameController.MoveCard(TurnTakerController, selectCardDecision.SelectedCard, trashDestination.Location, trashDestination.ToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: true, storedResults.CastEnumerable<SelectCardDecision, IDecision>(), null, storedResultsMove, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: true, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    if (DidMoveCard(storedResultsMove))
                    {
                        coroutine = CancelAction(dd, showOutput: true, cancelFutureRelatedDecisions: true, null, isPreventEffect: true);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                }
                else
                {
                    IEnumerator coroutine2 = CancelAction(dd, showOutput: true, cancelFutureRelatedDecisions: true, null, isPreventEffect: true);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine2);
                    }
                }
            }
            else
            {
                IEnumerator coroutine3 = GameController.SendMessageAction("All cards under " + card.Title + " are indestructible and cannot be removed from play to prevent the damage!", Priority.Medium, GetCardSource(), null, showCardSource: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine3);
                }
            }
        }
    }
}
