using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class WrittenFutureCardController:GlyphCardUtilities
	{
		public WrittenFutureCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownDeath, "face-down death cards", true));
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownDestruction, "face-down destruction cards", true));
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownFate, "face-down fate cards", true));
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownInsight, "face-down insight cards", true));
        }

        public override IEnumerator Play()
        {
            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            List<Location> locationResults = new List<Location>();
            IEnumerator coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsGlyphFaceDownCard, "face-down"), true, destroyResults, locationResults, CustomDecisionMode.Default, cardSource: GetCardSource());
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
                    Location loc = locationResults.FirstOrDefault();
                    Card destroyed = GetDestroyedCards(destroyResults).FirstOrDefault();
                    if (IsDeath(destroyed))
                    {
                        coroutine = DeathResponse(loc);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    if (IsDestruction(destroyed))
                    {
                        coroutine = DestructionResponse(loc);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    if (IsFate(destroyed))
                    {
                        coroutine = FateResponse(loc);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    if (IsInsight(destroyed))
                    {
                        coroutine = InsightResponse(loc);
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

                destroyResults = new List<DestroyCardAction>();
                locationResults = new List<Location>();
                coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsGlyphFaceDownCard, "your face-down"), true, destroyResults, locationResults, CustomDecisionMode.Default, cardSource: GetCardSource());
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

        private IEnumerator DeathResponse(Location L)
        {
            //1 target in that play area is destroyed if it has 3 or fewer HP.
            SelectCardDecision decision = new SelectCardDecision(base.GameController, DecisionMaker, SelectionType.DestroyCard, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L && c.HitPoints <= 3), false, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardAndDoAction(decision, (SelectCardDecision d) => base.GameController.DestroyCard(DecisionMaker, d.SelectedCard, cardSource:GetCardSource()));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DestructionResponse(Location L)
        {
            //1 target in that play area deals itself 2 psychic damage.
            SelectCardDecision decision = new SelectCardDecision(base.GameController, DecisionMaker, SelectionType.DealDamageSelf, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L), false, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardAndDoAction(decision, (SelectCardDecision d) => DealDamage(d.SelectedCard, d.SelectedCard, 2, DamageType.Psychic, cardSource:GetCardSource()));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator FateResponse(Location L)
        {
            //1 target in that play area reduces its next damage dealt by 2.
            decisionMode = CustomDecisionMode.MakeNextDamageIrreducible;
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceDamageDealt, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L), results, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectCard(results))
            {
                Card selected = GetSelectedCard(results);
                ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(2);
                effect.SourceCriteria.IsSpecificCard = selected;
                effect.NumberOfUses = 1;
                effect.UntilTargetLeavesPlay(selected);
                coroutine = AddStatusEffect(effect);
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

        private IEnumerator InsightResponse(Location L)
        {
            //1 target in that play area increases its next damage dealt by 2
            decisionMode = CustomDecisionMode.PreventNextDamage;
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.IncreaseNextDamage, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L), results, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectCard(results))
            {
                Card selected = GetSelectedCard(results);
                IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(2);
                effect.SourceCriteria.IsSpecificCard = selected;
                effect.NumberOfUses = 1;
                effect.UntilTargetLeavesPlay(selected);
                coroutine = AddStatusEffect(effect);
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
    }
}

