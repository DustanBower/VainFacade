using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
    public class GlyphCharacterCardController : GlyphBaseCharacterCardController
    {
        public GlyphCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            //base.SpecialStringMaker.ShowListOfCardsAtLocations(() => this.TurnTaker.SubDecks, new LinqCardCriteria((Card c) => true), "Glyph's subdeck", "Glyph's subdecks");
            //base.SpecialStringMaker.ShowListOfCardsAtLocations(() => this.TurnTaker.SubTrashes, new LinqCardCriteria((Card c) => true), "Glyph's subtrash", "Glyph's subtrashes");
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Draw up to 2 cards. You may play a card face-down in any play area or swap 2 of your face-down cards.
            //I decided not to code the case where a player uses Applied Numerology to swap 3 cards.
            int num1 = GetPowerNumeral(0, 2);

            IEnumerator coroutine = DrawCards(this.HeroTurnTakerController, num1, false, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Play a card face-down in any play area.", SelectionType.PlayCard, PlayFaceDown, this.HeroTurnTaker.HasCardsInHand && base.GameController.CanPlayCards(this.TurnTakerController, GetCardSource())),
                new Function(base.HeroTurnTakerController, $"Swap 2 of your face-down cards", SelectionType.MoveCard,() => SwapFaceDownCards(), FindTurnTakersWhere((TurnTaker tt) => tt.PlayArea.Cards.Any((Card c) => c.Owner == this.TurnTaker && c.IsFaceDownNonCharacter)).Count() > 1)
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, $"{this.TurnTaker.Name} {(this.HeroTurnTaker.HasCardsInHand ? "cannot play cards" : "has no cards in hand")} and only has face-down cards in one play area.", null, GetCardSource());
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

        private IEnumerator PlayFaceDown()
        {
            List<SelectCardDecision> cardResults = new List<SelectCardDecision>();
            decisionMode = CustomDecisionMode.PlayFaceDownFromPower;
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, FindCardsWhere((Card c) => c.Location == this.HeroTurnTaker.Hand && c.Owner == this.TurnTaker && !c.IsMissionCard), cardResults, true, cardSource:GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Card card = GetSelectedCard(cardResults);
            if (card != null)
            {
                List<SelectTurnTakerDecision> locationResults = new List<SelectTurnTakerDecision>();
                coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, locationResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                Location loc = GetSelectedTurnTaker(locationResults).PlayArea;
                if (loc != null)
                {
                    coroutine = base.GameController.MoveCard(DecisionMaker, card, loc, playCardIfMovingToPlayArea: false, flipFaceDown: true, cardSource: GetCardSource());
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

        private IEnumerator SwapFaceDownCards()
        {
            IEnumerator coroutine;

            List<TurnTaker> ttWithCards = FindTurnTakersWhere((TurnTaker tt) => tt.PlayArea.Cards.Any((Card c) => c.Owner == this.TurnTaker && c.IsFaceDownNonCharacter)).ToList();

            if (ttWithCards.Count() > 1)
            {
                List<Card> selected = new List<Card>();

                List<SelectTurnTakerDecision> ttResults = new List<SelectTurnTakerDecision>();
                decisionMode = CustomDecisionMode.SelectFirstTurnTakerToSwapCard;
                coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, ttResults, additionalCriteria: (TurnTaker tt) => ttWithCards.Contains(tt), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                TurnTaker firstTT = GetSelectedTurnTaker(ttResults);

                if (firstTT != null)
                {
                    List<Card> faceDown = firstTT.PlayArea.Cards.Where((Card c) => c.Owner == this.TurnTaker && c.IsFaceDownNonCharacter).ToList();
                    if (faceDown.Count() > 1)
                    {
                        List<SelectCardDecision> results = new List<SelectCardDecision>();
                        decisionMode = CustomDecisionMode.SelectFirstCardToSwap;
                        coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, new LinqCardCriteria((Card c) => faceDown.Contains(c)), results, false, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        if (GetSelectedCard(results) != null)
                        {
                            selected.Add(GetSelectedCard(results));
                        }
                    }
                    else if (faceDown.Count() == 1)
                    {
                        coroutine = base.GameController.SendMessageAction($"{this.TurnTaker.Name} only has 1 face-down card in {firstTT.PlayArea.GetFriendlyName()}.", Priority.Low, GetCardSource(), faceDown);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        selected.Add(faceDown.FirstOrDefault());
                    }
                }


                List<SelectTurnTakerDecision> ttResults2 = new List<SelectTurnTakerDecision>();
                decisionMode = CustomDecisionMode.SelectSecondTurnTakerToSwapCard;
                coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, ttResults2, additionalCriteria: (TurnTaker tt) => ttWithCards.Contains(tt) && tt != firstTT, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                TurnTaker secondTT = GetSelectedTurnTaker(ttResults2);

                if (secondTT != null)
                {
                    List<Card> faceDown2 = secondTT.PlayArea.Cards.Where((Card c) => c.Owner == this.TurnTaker && c.IsFaceDownNonCharacter).ToList();
                    if (faceDown2.Count() > 1)
                    {
                        List<SelectCardDecision> results2 = new List<SelectCardDecision>();
                        decisionMode = CustomDecisionMode.SelectSecondCardToSwap;
                        coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, new LinqCardCriteria((Card c) => faceDown2.Contains(c)), results2, false, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        if (GetSelectedCard(results2) != null)
                        {
                            selected.Add(GetSelectedCard(results2));
                        }
                    }
                    else if (faceDown2.Count() == 1)
                    {
                        coroutine = base.GameController.SendMessageAction($"{this.TurnTaker.Name} only has 1 face-down card in {firstTT.PlayArea.GetFriendlyName()}.", Priority.Low, GetCardSource(), faceDown2);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        selected.Add(faceDown2.FirstOrDefault());
                    }
                }
                //List<SelectCardsDecision> results = new List<SelectCardsDecision>();
                //coroutine = base.GameController.SelectCardsAndStoreResults(DecisionMaker, SelectionType.MoveCard, (Card c) => c.Owner == this.TurnTaker && c.IsFaceDownNonCharacter && c.IsInPlay, 2, results, false, 2, cardSource: GetCardSource());
                //if (base.UseUnityCoroutines)
                //{
                //    yield return base.GameController.StartCoroutine(coroutine);
                //}
                //else
                //{
                //    base.GameController.ExhaustCoroutine(coroutine);
                //}

                //List<Card> selected = GetSelectedCards(results).ToList();
                if (selected.Count() > 1)
                {
                    Location loc1 = selected[0].Location;
                    Location loc2 = selected[1].Location;
                    //Log.Debug($"Glyph will swap {selected.Select((Card c) => c.Title).ToCommaList()} between {loc1.GetFriendlyName()} and {loc2.GetFriendlyName()}");
                    coroutine = base.GameController.SendMessageAction($"{this.TurnTaker.Name} swaps{GetTypeOfCard(selected[0])} card from {loc1.GetFriendlyName()} and{GetTypeOfCard(selected[1])} card from {loc2.GetFriendlyName()}.", Priority.Low, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    coroutine = base.GameController.MoveCard(this.TurnTakerController, selected[0], loc2, playCardIfMovingToPlayArea: false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    coroutine = base.GameController.MoveCard(this.TurnTakerController, selected[1], loc1, playCardIfMovingToPlayArea: false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                else if (selected.Count() == 1)
                {
                    coroutine = base.GameController.SendMessageAction("Only 1 card was selected. You cannot swap just 1 card!", Priority.Low, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                else
                {
                    coroutine = base.GameController.SendMessageAction("No cards were selected.", Priority.Low, GetCardSource());
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
            else
            {
                coroutine = base.GameController.SendMessageAction($"{this.TurnTaker.Name} only has face-down cards in one play area.",Priority.Low,GetCardSource());
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

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    //Reveal and replace the top card of a deck.
                    List<SelectLocationDecision> revealResults = new List<SelectLocationDecision>();
                    coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location L) => true, revealResults, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidSelectLocation(revealResults))
                    {
                        Location deck = GetSelectedLocation(revealResults);
                        coroutine = base.GameController.RevealAndReplaceCards(DecisionMaker, deck, 1, null, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }

                    //1 player may play a card.
                    coroutine = SelectHeroToPlayCard(DecisionMaker);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
                case 1:
                    //1 player draws a card or puts a relic, spell, ritual, rune, or glyph from their trash on top of their deck.
                    List<SelectTurnTakerDecision> results = new List<SelectTurnTakerDecision>();
                    coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MakeDecision, results,additionalCriteria: (TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    TurnTaker hero = GetSelectedTurnTaker(results);
                    if (hero != null)
                    {
                        HeroTurnTakerController httc = FindHeroTurnTakerController(hero.ToHero());
                        IEnumerable<Function> functionChoices = new Function[2]
                        {
                        new Function(httc, "Draw a card.", SelectionType.DrawCard, () => DrawCard(hero.ToHero()), forcedActionMessage: $"{hero.Name} has no relics, spells, rituals, runes, or glyphs in their trash."),
                        new Function(httc, "Move a relic, spell, ritual, rune, or glyph from your trash to the top of your deck.", SelectionType.MoveCard,() => Incap2Move(httc), hero.Trash.Cards.Any((Card c) => Incap2Criteria(c)))
                        };

                        SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, httc, functionChoices, true, null, $"{this.TurnTaker.Name} {(this.HeroTurnTaker.HasCardsInHand ? "cannot play cards" : "has no cards in hand")} and only has face-down cards in one play area.", null, GetCardSource());
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
                    break;
                case 2:
                    //Destroy an environment card.
                    coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
            }
        }

        private bool Incap2Criteria(Card c)
        {
            //relic, spell, ritual, rune, or glyph
            bool IsRelic = base.GameController.DoesCardContainKeyword(c, "relic");
            bool IsSpell = base.GameController.DoesCardContainKeyword(c, "spell");
            bool IsRitual = base.GameController.DoesCardContainKeyword(c, "ritual");
            bool IsRune = base.GameController.DoesCardContainKeyword(c, "rune");
            bool IsGlyph = base.GameController.DoesCardContainKeyword(c, "glyph");

            return IsRelic || IsSpell || IsRitual || IsRune || IsGlyph;
        }

        private IEnumerator Incap2Move(HeroTurnTakerController httc)
        {
            IEnumerator coroutine = base.GameController.SelectCardFromLocationAndMoveIt(httc, httc.TurnTaker.Trash, new LinqCardCriteria(Incap2Criteria, "relic, spell, ritual, rune, or glyph"), new MoveCardDestination[] {new MoveCardDestination( httc.HeroTurnTaker.Deck )});
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