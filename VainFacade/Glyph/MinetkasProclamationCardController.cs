using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class MinetkasProclamationCardController:GlyphCardUtilities
	{
		public MinetkasProclamationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownMight, "face-down might cards", true));
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownFate, "face-down fate cards", true)).Condition = () => !this.Card.IsInPlay;
            //base.SpecialStringMaker.ShowListOfCards(new LinqCardCriteria((Card c) => GeAffectedCards().Contains(c), "affected by this card", false, true));
        }

        public override IEnumerator Play()
        {
            //When this card enters play, destroy any number of your face-down fate cards. 
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            List<Location> locationResults = new List<Location>();
            IEnumerator coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownFate, "face-down fate"), true, destroyResults, locationResults, CustomDecisionMode.Fate, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            while (locationResults.FirstOrDefault() != null)
            {
                if (DidDestroyCard(destroyResults) && locationResults.FirstOrDefault() != null && locationResults.FirstOrDefault().IsPlayArea)
                {
                    //For each card destroyed this way, increase or reduce damage dealt by a target in that play area by 1 until this card leaves play.
                    Location loc = locationResults.FirstOrDefault();
                    coroutine = DestroyResponse(loc);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }

                destroyResults = new List<DestroyCardAction>();
                locationResults = new List<Location>();
                coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownFate, "face-down fate"), true, destroyResults, locationResults, CustomDecisionMode.Fate, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }

        private IEnumerator DestroyResponse(Location loc)
        {
            List<SelectCardDecision> cardResults = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ModifyDamageAmount, new LinqCardCriteria((Card c) => c.Location.HighestRecursiveLocation == loc && c.IsInPlayAndHasGameText && c.IsTarget), cardResults, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectCard(cardResults))
            {
                Card selected = GetSelectedCard(cardResults);
                IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, $"Increase damage dealt by {selected.Title}", SelectionType.IncreaseDamage,() => IncreaseResponse(selected)),
                new Function(base.HeroTurnTakerController, $"Reduce damage dealt by {selected.Title}", SelectionType.ReduceDamageDealt,() => ReduceResponse(selected))
                };

                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
                IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(choose);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(choose);
                }
            }
        }

        private IEnumerator IncreaseResponse(Card card)
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
            effect.SourceCriteria.IsSpecificCard = card;
            effect.UntilCardLeavesPlay(this.Card);
            effect.UntilTargetLeavesPlay(card);
            IEnumerator coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator ReduceResponse(Card card)
        {
            ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(1);
            effect.SourceCriteria.IsSpecificCard = card;
            effect.UntilCardLeavesPlay(this.Card);
            effect.UntilTargetLeavesPlay(card);
            IEnumerator coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddTriggers()
        {
            //At the start of your turn, destroy 1 of your face - down might cards in the play area of a target affected this way or destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, TriggerType.DestroyCard );
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsFaceDownMight(c) && GetAffectedPlayAreas().Contains(c.Location.OwnerTurnTaker), "face-down might"), true, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidDestroyCard(results))
            {
                coroutine = DestroyThisCardResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }

        private List<TurnTaker> GetAffectedPlayAreas()
        {
            IEnumerable<IncreaseDamageStatusEffect> IncreaseEffects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is IncreaseDamageStatusEffect && ((IncreaseDamageStatusEffect)sec.StatusEffect).CardSource == this.Card).Select((StatusEffectController sec) => (IncreaseDamageStatusEffect)sec.StatusEffect);
            IEnumerable<ReduceDamageStatusEffect> ReduceEffects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is ReduceDamageStatusEffect && ((ReduceDamageStatusEffect)sec.StatusEffect).CardSource == this.Card).Select((StatusEffectController sec) => (ReduceDamageStatusEffect)sec.StatusEffect);

            IEnumerable<Card> IncreaseCards = IncreaseEffects.Select((IncreaseDamageStatusEffect e) => e.SourceCriteria.IsSpecificCard);
            IEnumerable<Card> ReduceCards = ReduceEffects.Select((ReduceDamageStatusEffect e) => e.SourceCriteria.IsSpecificCard);

            List<Card> affected = new List<Card>();
            affected.AddRange(IncreaseCards);
            affected.AddRange(ReduceCards);
            return affected.Select((Card c) => c.Location.OwnerTurnTaker).Distinct().ToList();
        }

        private List<Card> GeAffectedCards()
        {
            IEnumerable<IncreaseDamageStatusEffect> IncreaseEffects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is IncreaseDamageStatusEffect && ((IncreaseDamageStatusEffect)sec.StatusEffect).CardSource == this.Card).Select((StatusEffectController sec) => (IncreaseDamageStatusEffect)sec.StatusEffect);
            IEnumerable<ReduceDamageStatusEffect> ReduceEffects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is ReduceDamageStatusEffect && ((ReduceDamageStatusEffect)sec.StatusEffect).CardSource == this.Card).Select((StatusEffectController sec) => (ReduceDamageStatusEffect)sec.StatusEffect);

            IEnumerable<Card> IncreaseCards = IncreaseEffects.Select((IncreaseDamageStatusEffect e) => e.SourceCriteria.IsSpecificCard);
            IEnumerable<Card> ReduceCards = ReduceEffects.Select((ReduceDamageStatusEffect e) => e.SourceCriteria.IsSpecificCard);

            List<Card> affected = new List<Card>();
            affected.AddRange(IncreaseCards);
            affected.AddRange(ReduceCards);
            return affected;
        }
    }
}

