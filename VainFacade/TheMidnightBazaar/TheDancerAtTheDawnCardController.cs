using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public class TheDancerAtTheDawnCardController : ThreenCardController
    {
        public TheDancerAtTheDawnCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show the number of Threen in the environment deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsThreen(c), "Threen"));
            // Show the number of Unbindings in the environment deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsUnbinding(c), "Unbinding"));
            // Show the Threen with the highest HP
            SpecialStringMaker.ShowHighestHP(1, cardCriteria: ThreenInPlay);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, discard the top 3 cards of the environment deck. Put any Threens or Unbindings discarded this way into play. Then set the HP of each Threen to the HP of the Threen with the highest HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, DiscardPlayHealResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay, TriggerType.GainHP });
            // ...
        }

        private IEnumerator DiscardPlayHealResponse(PhaseChangeAction pca)
        {
            // "... discard the top 3 cards of the environment deck. Put any Threens or Unbindings discarded this way into play."
            List<MoveCardAction> discards = new List<MoveCardAction>();
            IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(base.TurnTakerController, 3, storedResults: discards, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Put any Threens or Unbindings discarded this way into play."
            foreach (MoveCardAction move in discards)
            {
                if (IsThreen(move.CardToMove) || IsUnbinding(move.CardToMove))
                {
                    IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, move.CardToMove, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, actionSource: move, cardSource: GetCardSource());
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
            // "Then set the HP of each Threen to the HP of the Threen with the highest HP."
            List<Card> storedResults = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsThreen(c), storedResults, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (storedResults.Any())
            {
                Card highest = storedResults.FirstOrDefault();
                if (highest != null)
                {
                    if (highest.HitPoints > 0)
                    {
                        int targetHP = highest.HitPoints.Value;
                        List<Card> threen = FindCardsWhere(ThreenInPlay).ToList();
                        foreach(Card c in threen)
                        {
                            IEnumerator setCoroutine = base.GameController.SetHP(c, targetHP, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(setCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(setCoroutine);
                            }
                        }
                    }
                }
            }
            yield break;
        }
    }
}
