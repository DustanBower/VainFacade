using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class GlyphCardUtilities:CardController
	{
		public GlyphCardUtilities(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			//SpecialString s = base.SpecialStringMaker.ShowSpecialString(() => $"This card is in {this.Card.Location.OwnerName}'s play area.");
            //s.Condition = () => this.Card.Location.IsPlayArea && this.Card.IsFaceDownNonCharacter;
            //s.ShowWhileIncapacitated = true;
        }

        public string ShowLocationsOfFaceDownCards(Func<Card, bool> condition, string description, bool showNumber)
        {
            List<TurnTaker> turnTakers = FindTurnTakersWhere((TurnTaker tt) => tt.PlayArea.Cards.Any(condition)).ToList();
            if (turnTakers.Any())
            {
                string text = "";
                if (showNumber)
                {
                    text = turnTakers.Select((TurnTaker tt) => $"{tt.Name} ({tt.PlayArea.Cards.Where(condition).Count()})").ToCommaList();
                }
                else
                {
                    text = turnTakers.Select((TurnTaker tt) => tt.Name).ToCommaList();
                }
                return $"{description} are in: {text}.";
            }
            return $"There are no {description} in play.";
        }

        public enum CustomDecisionMode
        {
            Might,
            Fate,
            Insight,
            Death,
            Destruction,
            Default,
            MakeNextDamageIrreducible,
            PreventNextDamage,
            PlayOrDiscardTopCard
        };

        public CustomDecisionMode decisionMode;

        public bool IsRitual(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "ritual", false, false);
        }

        public bool IsRelic(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "relic", false, false);
        }

        public bool IsGlyphFaceDownCard(Card c)
		{
			return c.Owner == this.TurnTaker && c.IsInPlay && c.IsFaceDownNonCharacter;
        }

		public bool IsMight(Card c)
		{
			return base.GameController.DoesCardContainKeyword(c, "might", false, true);
		}

		public bool IsFaceDownMight(Card c)
		{
			return IsGlyphFaceDownCard(c) && IsMight(c);
		}

        public bool IsInsight(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "insight", false, true);
        }

        public bool IsFaceDownInsight(Card c)
        {
            return IsGlyphFaceDownCard(c) && IsInsight(c);
        }

        public bool IsFate(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "fate", false, true);
        }

        public bool IsFaceDownFate(Card c)
        {
            return IsGlyphFaceDownCard(c) && IsFate(c);
        }

        public bool IsDeath(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "death", false, true);
        }

        public bool IsFaceDownDeath(Card c)
        {
            return IsGlyphFaceDownCard(c) && IsDeath(c);
        }

        public bool IsDestruction(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "destruction", false, true);
        }

        public bool IsFaceDownDestruction(Card c)
        {
            return IsGlyphFaceDownCard(c) && IsDestruction(c);
        }

        public int NumberOfFaceDownCardsInPlayArea(TurnTaker tt)
        {
            return tt.PlayArea.Cards.Where((Card c) => IsGlyphFaceDownCard(c)).Count();
        }

        public int NumberOfFaceDownCardsInPlayArea(Location L)
        {
            return L.Cards.Where((Card c) => IsGlyphFaceDownCard(c)).Count();
        }

        public IEnumerator SelectAndDestroyCardAndReturnLocation(HeroTurnTakerController hero, LinqCardCriteria cardCriteria, bool optional, List<DestroyCardAction> storedResultsAction = null, List<Location> storedResultsLocation = null, CustomDecisionMode mode = CustomDecisionMode.Default, Card responsibleCard = null, CardSource cardSource = null)
        {
            //Copy of GameController.SelectAndDestroyCard, but that also returns the location of the card before it was destroyed
            IEnumerator coroutine;
            BattleZone battleZone = null;
            if (cardSource != null && !cardCriteria.IgnoreBattleZone)
            {
                battleZone = cardSource.BattleZone;
            }
            IEnumerable<Card> enumerable = FindCardsWhere((Card c) => c.IsInPlay && !FindCardController(c).IsBeingDestroyed && !c.IsOneShot && cardCriteria.Criteria(c), realCardsOnly: true, null);
            if (enumerable.Any())
            {
                IEnumerable<Card> source = enumerable.Where((Card c) => !base.GameController.IsCardIndestructible(c));
                IEnumerable<Card> source2 = enumerable.Where((Card c) => base.GameController.IsCardVisibleToCardSource(c, cardSource));
                if (source.Any() && source2.Any())
                {
                    SelectCardDecision selectCardDecision = new SelectCardDecision(base.GameController, hero, SelectionType.DestroyCard, enumerable, optional, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: false, actionCanBeCancelled: true, cardSource);
                    selectCardDecision.BattleZone = battleZone;


                    //IEnumerator coroutine = base.GameController.SelectCardAndDoAction(selectCardDecision, delegate (SelectCardDecision d)
                    //{
                    //    GameController gameController = base.GameController;
                    //    HeroTurnTakerController hero2 = hero;
                    //    Card selectedCard = d.SelectedCard;
                    //    List<DestroyCardAction> storedResults = storedResultsAction;
                    //    Card responsibleCard2 = responsibleCard;
                    //    CardSource cardSource2 = cardSource;
                    //    return gameController.DestroyCard(hero2, selectedCard, optional: false, storedResults, null, null, null, responsibleCard2, null, null, null, cardSource2);
                    //});
                    //if (UseUnityCoroutines)
                    //{
                    //    yield return base.GameController.StartCoroutine(coroutine);
                    //}
                    //else
                    //{
                    //    base.GameController.ExhaustCoroutine(coroutine);
                    //}

                    List<TurnTaker> turnTakers = enumerable.Select((Card c) => c.Location.OwnerTurnTaker).Distinct().ToList();

                    List<SelectTurnTakerDecision> selectTTResults = new List<SelectTurnTakerDecision>();

                    if (turnTakers.Count() > 1)
                    {
                        decisionMode = mode;
                        coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, selectTTResults, optional, additionalCriteria: (TurnTaker tt) => turnTakers.Contains(tt), cardSource: GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }

                    if (DidSelectTurnTaker(selectTTResults) || turnTakers.Count() == 1)
                    {
                        Location playArea = turnTakers.FirstOrDefault().PlayArea;
                        if (turnTakers.Count() > 1)
                        {
                            playArea = GetSelectedTurnTaker(selectTTResults).PlayArea;
                        }
                        IEnumerable<Card> enumerable2 = enumerable.Where((Card c) => c.Location.HighestRecursiveLocation == playArea);

                        List<SelectCardDecision> selectionResults = new List<SelectCardDecision>();
                        decisionMode = mode;
                        coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, enumerable2, selectionResults, optional, cardSource: cardSource);
                        if (UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        if (DidSelectCard(selectionResults))
                        {
                            Card selected = GetSelectedCard(selectionResults);

                            storedResultsLocation.Add(selected.Location.HighestRecursiveLocation);

                            coroutine = base.GameController.DestroyCard(hero, selected, false, storedResultsAction, null, null, null, responsibleCard, null, null, null, cardSource);
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
                }
                else
                {
                    string message = (source.Any() ? $"All {cardCriteria.GetDescription()} in play cannot be destroyed by {cardSource.Card.Title}." : $"All {cardCriteria.GetDescription()} in play are indestructible.");
                    IEnumerator coroutine2 = base.GameController.SendMessageAction(message, Priority.High, cardSource, null, showCardSource: true);
                    if (UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine2);
                    }
                }
            }
            else
            {
                string text = cardCriteria.GetDescription();
                if (string.IsNullOrEmpty(text))
                {
                    text = "applicable";
                }
                string text2 = "There are no " + text + " in play for " + cardSource.CardController.CardWithoutReplacements.Title + " to destroy.";
                if (hero != null && hero.TurnTaker.Identifier == "Guise")
                {
                    text2 += " [i]What a waste![/i]";
                }
                IEnumerator coroutine3 = base.GameController.SendMessageAction(text2, Priority.High, cardSource);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            CustomDecisionText result = null;

            string keyword = "";
            if (decisionMode == CustomDecisionMode.Might)
            {
                keyword = "might ";
            }
            else if (decisionMode == CustomDecisionMode.Fate)
            {
                keyword = "fate ";
            }
            else if (decisionMode == CustomDecisionMode.Insight)
            {
                keyword = "insight ";
            }
            else if (decisionMode == CustomDecisionMode.Death)
            {
                keyword = "death ";
            }
            else if (decisionMode == CustomDecisionMode.Destruction)
            {
                keyword = "destruction ";
            }

            if (decision is SelectTurnTakerDecision)
            {
                string text = $"a play area to destroy a {keyword}card.";
                result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
            }
            else if (decision is SelectCardDecision && ((SelectCardDecision)decision).Choices.Any())
            {
                string playArea = ((SelectCardDecision)decision).Choices.FirstOrDefault().Location.GetFriendlyName();
                result = new CustomDecisionText(
                $"Destroy a {keyword}card in {playArea}",
                $"The other heroes are choosing a {keyword}card to destroy in {playArea}",
                $"Vote for a {keyword}card to destroy in {playArea}",
                $"a {keyword}card to destroy in {playArea}"
                );
            }
            
            
            return result;
        }

        public override MoveCardDestination GetDeckDestination()
        {
            return new MoveCardDestination(this.TurnTaker.Deck);
        }

        public override MoveCardDestination GetTrashDestination()
        {
            return new MoveCardDestination(this.TurnTaker.Trash);
        }
    }
}

