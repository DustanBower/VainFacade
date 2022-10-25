using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class EerieQuietCardController : CardController
    {
        public EerieQuietCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase all psychic and sonic damage by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Psychic, 1);
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Sonic, 1);
            // "At the end of the environment turn, each hero may deal themselves 1 psychic damage to put a card from their trash on top of their deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SelectToTakeDamageStackCardResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.MoveCard });
            // "At the start of the environment turn, play the top card of the villain deck, then destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayVillainCardSelfDestructResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroySelf });
        }

        private IEnumerator SelectToTakeDamageStackCardResponse(PhaseChangeAction pca)
        {
            // "... each hero may deal themselves 1 psychic damage to put a card from their trash on top of their deck."
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame && tt.Trash.HasCards, "heroes with cards in their trash"), SelectionType.Custom, TakeDamageToStackCardResponse, optional: false, requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator TakeDamageToStackCardResponse(TurnTaker tt)
        {
            // "... may deal themselves 1 psychic damage..."
            List<Card> targets = new List<Card>();
            if (tt.HasMultipleCharacterCards)
            {
                IEnumerator findCoroutine = FindCharacterCardToTakeDamage(tt, targets, null, 1, DamageType.Psychic);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
            }
            else
            {
                targets.Add(tt.CharacterCard);
            }
            Card hero = targets.FirstOrDefault();
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            if (hero != null)
            {
                IEnumerator damageCoroutine = DealDamage(hero, hero, 1, DamageType.Psychic, storedResults: damageResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            if (damageResults.Count > 0)
            {
                // "... to put a card from their trash on top of their deck."
                IEnumerator stackCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.GameController.FindHeroTurnTakerController(tt.ToHero()), tt.Trash, new LinqCardCriteria((Card c) => true), (new MoveCardDestination(tt.Deck)).ToEnumerable(), showOutput: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(stackCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(stackCoroutine);
                }
            }
        }

        private IEnumerator PlayVillainCardSelfDestructResponse(PhaseChangeAction pca)
        {
            // "... play the top card of the villain deck..."
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... then destroy this card."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a hero to deal themself damage and move a card", "deciding which hero will deal themself damage and move a card", "Vote for a hero to deal themself damage and move a card", "hero to deal themself damage and move a card");
        }
    }
}
