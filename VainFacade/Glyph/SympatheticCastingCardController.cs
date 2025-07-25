using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class SympatheticCastingCardController:GlyphCardUtilities
	{
		public SympatheticCastingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsGlyphFaceDownCard, "your face-down cards", true));
		}

        public override IEnumerator UsePower(int index = 0)
        {
            //Select a play area. X on this card = the number of different visible keywords on your face-down cards in that area. {Glyph} deals herself and 1 target in that play area X irreducible infernal damage each, then {Glyph} deals that target X fire damage.
            int num = GetPowerNumeral(0, 1);

            List<SelectTurnTakerDecision> results = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectTurnTaker(results))
            {
                TurnTaker selected = GetSelectedTurnTaker(results);
                IEnumerable<Card> faceDownCards = FindCardsWhere((Card c) => IsGlyphFaceDownCard(c) && c.Location.HighestRecursiveLocation == selected.PlayArea);
                int X = 0;
                if (faceDownCards.Any(IsFaceDownMight))
                {
                    X++;
                }
                if (faceDownCards.Any(IsFaceDownInsight))
                {
                    X++;
                }
                if (faceDownCards.Any(IsFaceDownFate))
                {
                    X++;
                }
                if (faceDownCards.Any(IsFaceDownDestruction))
                {
                    X++;
                }
                if (faceDownCards.Any(IsFaceDownDeath))
                {
                    X++;
                }

                coroutine = DealDamage(this.CharacterCard, this.CharacterCard, X, DamageType.Infernal, true, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                List<DealDamageAction> damage = new List<DealDamageAction>();
                damage.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.CharacterCard), null, X, DamageType.Infernal, true));
                damage.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.CharacterCard), null, X, DamageType.Fire));
                coroutine = SelectTargetsAndDealMultipleInstancesOfDamage(damage, (Card c) => c.Location.HighestRecursiveLocation == selected.PlayArea, null, num, num);
                if (UseUnityCoroutines)
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
            string text = $"a play area.";
            CustomDecisionText result = new CustomDecisionText(
            $"Select {text}",
            $"The other heroes are choosing {text}",
            $"Vote for {text}",
            $"{text}"
            );
            return result;
        }
    }
}

