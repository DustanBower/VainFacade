using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class ReturnToSilenceCardController:GlyphCardUtilities
	{
		public ReturnToSilenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownMight, "face-down might cards", true));
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownFate, "face-down fate cards", true));
            AddThisCardControllerToList(CardControllerListType.ModifiesKeywords);
		}

        //public override void AddTriggers()
        //{
        //    AddTrigger<ExpireStatusEffectAction>((ExpireStatusEffectAction exp) => exp.StatusEffect is ReturnToSilenceStatusEffect, CleanUpKeywordsResponse, TriggerType.Hidden, TriggerTiming.After);
        //}

        public override IEnumerator Play()
        {
            //You may destroy 1 of your face-down might cards.
            List<DestroyCardAction> destroyMight = new List<DestroyCardAction>();
            List<Location> locationMight = new List<Location>();
            IEnumerator coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownMight, "face-down might"), true, destroyMight, locationMight, CustomDecisionMode.Might, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDestroyCard(destroyMight) && locationMight.FirstOrDefault() != null && locationMight.FirstOrDefault().IsPlayArea)
            {
                //If you do, each non-character non-target card in that card's play area gains the ongoing keyword this turn.
                Location loc = locationMight.FirstOrDefault();
                List<Card> affectedCards = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == loc && !c.IsCharacter && !c.IsTarget).ToList();
                coroutine = base.GameController.ModifyKeywords("ongoing", true, affectedCards, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                ReturnToSilenceStatusEffect effect = new ReturnToSilenceStatusEffect(this.Card, loc);
                effect.UntilThisTurnIsOver(base.Game);
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

            //Destroy any number of your face-down death cards.
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            List<Location> locationResults = new List<Location>();
            coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownFate, "face-down fate"), true, destroyResults, locationResults, CustomDecisionMode.Fate, cardSource: GetCardSource());
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
                    //For each card destroyed this way, destroy an environment, ongoing, or equipment card in that card's play area.
                    Location loc = locationResults.FirstOrDefault();
                    Console.WriteLine($"Destroy a card in {loc.GetFriendlyName()}");
                    coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.Location.HighestRecursiveLocation == loc && (IsOngoing(c) || c.IsEnvironment || IsEquipment(c)), "ongoing, environment, or equipment"), false, responsibleCard: this.Card, cardSource: GetCardSource());
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

        //This functionality had to be moved to Glyph's character card since this is a one-shot
        //private List<Location> GetAffectedPlayAreas()
        //{
        //    return base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is ReturnToSilenceStatusEffect).Select((StatusEffectController sec) => ((ReturnToSilenceStatusEffect)sec.StatusEffect).PlayArea).Distinct().ToList();
        //}

        //public override bool AskIfCardContainsKeyword(Card card, string keyword, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
        //{
        //    List<Location> affected = GetAffectedPlayAreas();
        //    if (affected != null && keyword == "ongoing" && !card.IsTarget && !card.IsCharacter && affected.Contains(card.Location.HighestRecursiveLocation))
        //    {
        //        return true;
        //    }
        //    return base.AskIfCardContainsKeyword(card, keyword, evenIfUnderCard, evenIfFaceDown);
        //}

        //public override IEnumerable<string> AskForCardAdditionalKeywords(Card card)
        //{
        //    List<Location> affected = GetAffectedPlayAreas();
        //    if (affected != null && !card.IsTarget && !card.IsCharacter && affected.Contains(card.Location.HighestRecursiveLocation))
        //    {
        //        return new string[1]
        //        {
        //        "ongoing"
        //        };
        //    }
        //    return base.AskForCardAdditionalKeywords(card);
        //}

        //private IEnumerator CleanUpKeywordsResponse(ExpireStatusEffectAction exp)
        //{
        //    Location playArea = ((ReturnToSilenceStatusEffect)exp.StatusEffect).PlayArea;
        //    List<Location> affectedAreas = GetAffectedPlayAreas();
        //    if (!affectedAreas.Contains(playArea))
        //    {
        //        List<Card> affectedCards = FindCardsWhere((Card c) => !c.IsTarget && !c.IsCharacter && c.Location.HighestRecursiveLocation == playArea && !c.IsOngoing).ToList();
        //        IEnumerator coroutine = base.GameController.ModifyKeywords("ongoing", addingOrRemoving: false, affectedCards, GetCardSource());
        //        if (base.UseUnityCoroutines)
        //        {
        //            yield return base.GameController.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            base.GameController.ExhaustCoroutine(coroutine);
        //        }
        //    }
        //}
    }
}

