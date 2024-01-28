using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class UnspokenDecreeCardController:GlyphCardUtilities
	{ 
		public UnspokenDecreeCardController(Card card, TurnTakerController turnTakerController)
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
                coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsGlyphFaceDownCard, "face-down"), true, destroyResults, locationResults, CustomDecisionMode.Default, cardSource: GetCardSource());
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
            //1 target in that play area is put on top its deck, if not a character.
            SelectCardDecision decision = new SelectCardDecision(base.GameController, DecisionMaker, SelectionType.MoveCardOnDeck, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L && !c.IsCharacter), false, cardSource:GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardAndDoAction(decision, (SelectCardDecision d) => base.GameController.MoveCard(this.TurnTakerController, d.SelectedCard, d.SelectedCard.NativeDeck, cardSource: GetCardSource()));
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
            //1 target in that play area Deals a target 2 melee damage.
            SelectCardDecision decision = new SelectCardDecision(base.GameController, DecisionMaker, SelectionType.DealDamage, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L), false, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardAndDoAction(decision, (SelectCardDecision d) => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, d.SelectedCard), 2, DamageType.Melee, 1, false, 1, cardSource:GetCardSource()));
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
            //1 target in that play area Makes its next damage dealt irreducible.
            decisionMode = CustomDecisionMode.MakeNextDamageIrreducible;
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom,FindCardsWhere( (Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L), results, false, cardSource: GetCardSource());
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
                MakeDamageIrreducibleStatusEffect effect = new MakeDamageIrreducibleStatusEffect();
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
            //1 target in that play area Prevents the next damage it would deal.
            decisionMode = CustomDecisionMode.PreventNextDamage;
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == L), results, false, cardSource: GetCardSource());
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
                CannotDealDamageStatusEffect effect = new CannotDealDamageStatusEffect();
                effect.SourceCriteria.IsSpecificCard = selected;
                effect.NumberOfUses = 1;
                effect.IsPreventEffect = true;
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decisionMode == CustomDecisionMode.MakeNextDamageIrreducible)
            {
                string text = $"a target to prevent its next damage.";
                CustomDecisionText result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
                return result;
            }
            else if (decisionMode == CustomDecisionMode.PreventNextDamage)
            {
                string text = $"a target to make its next damage irreducible.";
                CustomDecisionText result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
                return result;
            }
            return base.GetCustomDecisionText(decision);
        }
    }
}

