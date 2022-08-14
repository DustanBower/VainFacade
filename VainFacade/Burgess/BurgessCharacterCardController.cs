using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class BurgessCharacterCardController : HeroCharacterCardController
    {
        public BurgessCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If active: show number of Clue cards in Burgess's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain(ClueKeyword), "Clue")).Condition = () => !base.Card.IsFlipped;
        }

        protected const string ClueKeyword = "clue";

        public override IEnumerator UsePower(int index = 0)
        {
            // "Discard the top two cards of your deck."
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(base.TurnTakerController, 2, storedResults: results, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Put any Clues discarded this way into play or into your hand."
            List<Card> discardedClues = results.SelectMany<MoveCardAction, Card>((MoveCardAction mca) => mca.CardToMove.ToEnumerable()).Where((Card c) => c.DoKeywordsContain(ClueKeyword)).ToList();
            MoveCardDestination[] playOrHand = new MoveCardDestination[2] { new MoveCardDestination(base.TurnTaker.PlayArea), new MoveCardDestination(base.HeroTurnTaker.Hand) };
            foreach (Card c in discardedClues)
            {
                IEnumerator moveCoroutine = base.GameController.SelectLocationAndMoveCard(base.HeroTurnTakerController, c, playOrHand, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            // "Draw a card."
            IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may draw a card."
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCard(base.HeroTurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        private IEnumerator UseIncapOption2()
        {
            // "Increase projectile damage dealt by hero targets by 1 until the start of your next turn."
            IncreaseDamageStatusEffect aim = new IncreaseDamageStatusEffect(1);
            aim.SourceCriteria.IsHero = true;
            aim.SourceCriteria.IsTarget = true;
            aim.DamageTypeCriteria.AddType(DamageType.Projectile);
            aim.UntilStartOfNextTurn(base.TurnTaker);
            IEnumerator aimCoroutine = AddStatusEffect(aim);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(aimCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(aimCoroutine);
            }
        }

        private IEnumerator UseIncapOption3()
        {
            // "One hero may use a power now."
            IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
        }
    }
}
