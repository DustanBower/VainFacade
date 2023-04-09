using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class ExplosiveMacguffinCardController : CardController
    {
        public ExplosiveMacguffinCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero card is discarded on an environment or villain turn, the target with the lowest HP from that turn's associated deck deals each other target in its play area 1 projectile damage, then deals itself 4 irreducible fire damage. Then destroy this card."
            AddTrigger((MoveCardAction mca) => mca.IsDiscard && IsHero(mca.CardToMove) && (GameController.ActiveTurnTaker.IsEnvironment || GameController.ActiveTurnTaker.IsVillain), ExplodeResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard }, TriggerTiming.After);
        }

        private IEnumerator ExplodeResponse(MoveCardAction mca)
        {
            // "... the target with the lowest HP from that turn's associated deck..."
            List<Card> lowestTargets = new List<Card>();
            List<Location> associatedDecks = GameController.ActiveTurnTaker.Decks;
            foreach(Location deck in associatedDecks)
            {
                List<Card> lowestFromDeck = new List<Card>();
                IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => c.NativeDeck == deck, lowestFromDeck, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                lowestTargets.Add(lowestFromDeck.FirstOrDefault());
            }
            List<SelectTargetDecision> cardsChosen = new List<SelectTargetDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectTargetAndStoreResults(DecisionMaker, lowestTargets, cardsChosen, selectionType: SelectionType.DealDamage, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectTargetDecision choice = cardsChosen.FirstOrDefault((SelectTargetDecision d) => d.Completed);
            if (choice != null)
            {
                Card selectedCard = choice.SelectedCard;
                // "... deals each other target in its play area 1 projectile damage, ..."
                Location playArea = selectedCard.Location.HighestRecursiveLocation;
                IEnumerator projectileCoroutine = DealDamage(selectedCard, (Card c) => c.IsAtLocationRecursive(playArea) && c != selectedCard, 1, DamageType.Projectile);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(projectileCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(projectileCoroutine);
                }
                // "... then deals itself 4 irreducible fire damage."
                IEnumerator fireCoroutine = DealDamage(selectedCard, selectedCard, 4, DamageType.Fire, isIrreducible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(fireCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(fireCoroutine);
                }
            }
            // "Then destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(mca);
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
