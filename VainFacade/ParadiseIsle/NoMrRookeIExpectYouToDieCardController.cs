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
    public class NoMrRookeIExpectYouToDieCardController : ParadiseIsleUtilityCardController
    {
        public NoMrRookeIExpectYouToDieCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show Conspirator with highest HP
            SpecialStringMaker.ShowHighestHP(cardCriteria: isConspirator).Condition = () => FindCardsWhere(isConspiratorInPlay).Count() > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Conspirators in play.").Condition = () => FindCardsWhere(isConspiratorInPlay).Count() <= 0;
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
            // Show number of cards in environment trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, showInEffectsList: () => base.Card.IsInPlayAndHasGameText);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the environment turn, the Conspirator with the highest HP deals the hero target with the highest HP X energy damage, where X = the number of cards in the environment trash. If no damage was dealt this way, discard the top card of the environment deck. If a Conspirator is discarded this way, put it into play."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDiscardPlayResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        private IEnumerator DamageDiscardPlayResponse(PhaseChangeAction pca)
        {
            // "... the Conspirator with the highest HP deals the hero target with the highest HP X energy damage, where X = the number of cards in the environment trash."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            if (FindCardsWhere(isConspiratorInPlay, GetCardSource()).Count() > 0)
            {
                List<Card> highestResults = new List<Card>();
                IEnumerator findConspiratorCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.DoKeywordsContain(ConspiratorKeyword), highestResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findConspiratorCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findConspiratorCoroutine);
                }
                Card highestConspirator = highestResults.FirstOrDefault();
                if (highestConspirator == null)
                {
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are no Conspirators in play to deal damage.", Priority.Medium, GetCardSource(), showCardSource: true);
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
                    IEnumerator damageCoroutine = DealDamageToHighestHP(highestConspirator, 1, (Card c) => IsHeroTarget(c), (Card c) => base.TurnTaker.Trash.NumberOfCards, DamageType.Energy, storedResults: damageResults);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                }
            }
            // "If no damage was dealt this way, discard the top card of the environment deck."
            if (!DidDealDamage(damageResults))
            {
                List<MoveCardAction> discardResults = new List<MoveCardAction>();
                IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(base.TurnTakerController, 1, storedResults: discardResults, showMessage: true, responsibleTurnTaker: base.TurnTaker);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                // "If a Conspirator is discarded this way, put it into play."
                foreach (MoveCardAction action in discardResults)
                {
                    if (action.CardToMove.DoKeywordsContain(ConspiratorKeyword))
                    {
                        IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, action.CardToMove, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(putCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(putCoroutine);
                        }
                    }
                }
            }
        }
    }
}
