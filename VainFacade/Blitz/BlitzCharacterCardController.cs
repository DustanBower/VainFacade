using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    internal class BlitzCharacterCardController : VillainCharacterCardController
    {
        public BlitzCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Both sides: show number of villain Circuit cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(IsVillainCircuit, () => true);
            // If flipped: show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP().Condition = () => base.Card.IsFlipped;
        }

        protected const string CircuitKeyword = "circuit";
        protected const string PowerSourceIdentifier = "PowerSource";
        public LinqCardCriteria IsCircuit = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CircuitKeyword), "Circuit");
        public LinqCardCriteria IsVillainCircuit = new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(CircuitKeyword), "villain Circuit");
        public LinqCardCriteria IsVillainCircuitInPlay = new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(CircuitKeyword) && c.IsInPlayAndHasGameText, "villain Circuit", singular: "card in play", plural: "cards in play");

        public override void AddTriggers()
        {
            base.AddTriggers();
            if (base.IsGameChallenge)
            {
                // "At the end of the villain turn, discard the top card of the villain deck. If Power Source is discarded this way, put it into play."
                AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardPutPowerSourceResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
            }
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Lightning damage cannot have its type changed."
                AddSideTrigger(AddTrigger((ChangeDamageTypeAction cdta) => cdta.DealDamageAction.DamageType == DamageType.Lightning, (ChangeDamageTypeAction cdta) => CancelAction(cdta), TriggerType.CancelAction, TriggerTiming.Before));
                // "Lightning damage dealt to {BlitzCharacter} is irreducible."
                AddSideTrigger(AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target == base.Card));
                // "When a target other than {BlitzCharacter} would be dealt lightning damage by a source other than {BlitzCharacter}, 1 player discards a card or that damage is redirected to {BlitzCharacter}."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target != base.Card && (dda.DamageSource == null || dda.DamageSource.Card == null || dda.DamageSource.Card != base.Card), DiscardOrRedirectResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.RedirectDamage }, TriggerTiming.Before));
                // "When {BlitzCharacter} is dealt lightning damage, increase the next lightning damage dealt by {BlitzCharacter} by X minus 1, where X = the damage taken."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target == base.Card && dda.DidDealDamage, ChargeUpResponse, TriggerType.CreateStatusEffect, TriggerTiming.After));
                // "At the end of the villain turn, if there are no villain Circuit cards in play, flip this card. Otherwise, the environment deals {BlitzCharacter} 3 lightning damage."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, FlipOrZappedResponse, new TriggerType[] { TriggerType.FlipCard, TriggerType.DealDamage }));
                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // "Increase damage dealt by {BlitzCharacter} by 1."
                    AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.Card, 1));
                }
            }
            else
            {
                // Back side:
                // "Lightning damage cannot have its type changed."
                AddSideTrigger(AddTrigger((ChangeDamageTypeAction cdta) => cdta.DealDamageAction.DamageType == DamageType.Lightning, (ChangeDamageTypeAction cdta) => CancelAction(cdta), TriggerType.CancelAction, TriggerTiming.Before));
                // "Lightning damage dealt to {BlitzCharacter} is irreducible."
                AddSideTrigger(AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target == base.Card));
                // "When a target other than {BlitzCharacter} would be dealt lightning damage by a source other than {BlitzCharacter}, 1 player discards a card or that damage is redirected to {BlitzCharacter}."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target != base.Card && (dda.DamageSource == null || dda.DamageSource.Card == null || dda.DamageSource.Card != base.Card), DiscardOrRedirectResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.RedirectDamage }, TriggerTiming.Before));
                // "When {BlitzCharacter} is dealt lightning damage, increase the next lightning damage dealt by {BlitzCharacter} by X minus 1, where X = the damage taken."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target == base.Card && dda.DidDealDamage, ChargeUpResponse, TriggerType.CreateStatusEffect, TriggerTiming.After));
                // "At the end of the villain turn, if there is at least 1 villain Circuit in play, flip this card."
                // "At the end of the villain turn, the environment deals {BlitzCharacter} 3 lightning damage, then {BlitzCharacter} deals the hero target with the highest HP {H + 1} melee damage and plays the top card of the villain deck."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, FlipOrZappedTacklePlayResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard }));
                if (base.IsGameAdvanced)
                {
                    // Back side, Advanced:
                    // "At the end of the villain turn, {BlitzCharacter} deals the hero target with the highest HP {H - 1} melee damage."
                    AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => c.IsHero && c.IsTarget, TargetType.HighestHP, H - 1, DamageType.Melee));
                }
            }
            AddDefeatedIfDestroyedTriggers();
        }

        private IEnumerator DiscardOrRedirectResponse(DealDamageAction dda)
        {
            // "... 1 player discards a card or that damage is redirected to {BlitzCharacter}."
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            SelectionType attribute = SelectionType.DiscardCard;
            if (!dda.IsRedirectable)
            {
                attribute = SelectionType.DiscardCardsNoRedirect;
            }
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, true, true, storedResultsDiscard: results, gameAction: dda, selectionType: attribute, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (!DidDiscardCards(results))
            {
                IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(redirectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(redirectCoroutine);
                }
            }
        }

        private IEnumerator ChargeUpResponse(DealDamageAction dda)
        {
            // "... increase the next lightning damage dealt by {BlitzCharacter} by X minus 1, where X = the damage taken."
            IncreaseDamageStatusEffect charge = new IncreaseDamageStatusEffect(dda.Amount - 1);
            charge.SourceCriteria.IsSpecificCard = base.Card;
            charge.NumberOfUses = 1;
            charge.DamageTypeCriteria.AddType(DamageType.Lightning);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(charge, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }

        private IEnumerator FlipOrZappedResponse(PhaseChangeAction pca)
        {
            // "... if there are no villain Circuit cards in play, flip this card."
            if (FindCardsWhere(IsVillainCircuitInPlay, GetCardSource()).Count() <= 0)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            else
            {
                // "Otherwise, the environment deals {BlitzCharacter} 3 lightning damage."
                IEnumerator damageCoroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, FindEnvironment().TurnTaker), base.Card, 3, DamageType.Lightning, cardSource: GetCardSource());
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

        private IEnumerator FlipOrZappedTacklePlayResponse(PhaseChangeAction pca)
        {
            // "... if there is at least 1 villain Circuit in play, flip this card."
            if (FindCardsWhere(IsVillainCircuitInPlay, GetCardSource()).Count() >= 1)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            else
            {
                // "... the environment deals {BlitzCharacter} 3 lightning damage, ..."
                IEnumerator zapCoroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, FindEnvironment().TurnTaker), base.Card, 3, DamageType.Lightning, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(zapCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(zapCoroutine);
                }
                // "... then {BlitzCharacter} deals the hero target with the highest HP {H + 1} melee damage..."
                IEnumerator meleeCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => c.IsHero && c.IsTarget, (Card c) => H + 1, DamageType.Melee);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(meleeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(meleeCoroutine);
                }
                // "... and plays the top card of the villain deck."
                IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(pca);
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

        private IEnumerator DiscardPutPowerSourceResponse(PhaseChangeAction pca)
        {
            // "... discard the top card of the villain deck."
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, results, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If Power Source is discarded this way, put it into play."
            MoveCardAction result = results.FirstOrDefault();
            if (result != null && result.CardToMove != null && result.CardToMove.Identifier == PowerSourceIdentifier)
            {
                IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, result.CardToMove, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
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
