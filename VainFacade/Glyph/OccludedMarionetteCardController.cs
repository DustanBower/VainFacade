using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Controller.OblivAeon;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class OccludedMarionetteCardController:GlyphCardUtilities
	{
		public OccludedMarionetteCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownMight, "face-down might cards", false));
		}

        public override IEnumerator Play()
        {
            //You may put a non-indestructible, non-character card on top of its deck.
            //Based on Abduct and Abandon
            var scd = new SelectCardDecision(GameController, DecisionMaker, SelectionType.MoveCardOnDeck, GameController.GetAllCards(), isOptional: true,
                additionalCriteria: c => !base.GameController.IsCardIndestructible(c) && c.IsInPlay && !c.IsCharacter && !c.IsOneShot && GameController.IsCardVisibleToCardSource(c, GetCardSource()) && (FindCardController(c) is MissionCardController ? c.IsFlipped : true),
                cardSource: GetCardSource());
            var coroutine = GameController.SelectCardAndDoAction(scd, SelectCardResponse);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }


            //You may destroy 1 of your face-down might cards to put the top card of of a deck in that card's play area into play or shuffle that deck.
            //If a card entered play this way, cards cannot be played from that deck until the start of your next turn.
            List<DestroyCardAction> destroyMight = new List<DestroyCardAction>();
            List<Location> locationResults = new List<Location>();
            coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownMight, "face-down might"), true, destroyMight, locationResults, CustomDecisionMode.Might, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        

            if (DidDestroyCard(destroyMight) && locationResults.FirstOrDefault() != null)
            {
                Location loc = locationResults.FirstOrDefault();
                List<SelectLocationDecision> locResults = new List<SelectLocationDecision>();
                coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.MakeDecision, (Location L) => L.OwnerTurnTaker == loc.OwnerTurnTaker && base.GameController.IsLocationVisibleToSource(L, GetCardSource()), locResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidSelectDeck(locResults))
                {
                    Location deck = GetSelectedLocation(locResults);
                    List<Card> playResults = new List<Card>();
                    
                    IEnumerable<Function> functionChoices = new Function[2]
                    {
                    new Function(base.HeroTurnTakerController, $"Put the top card of {deck.GetFriendlyName()} into play", SelectionType.PutIntoPlay, () => base.GameController.PlayTopCardOfLocation(this.TurnTakerController, deck, playedCards: playResults, responsibleTurnTaker: this.TurnTaker,isPutIntoPlay: true, cardSource:GetCardSource())),
                    new Function(base.HeroTurnTakerController, $"Shuffle {deck.GetFriendlyName()}", SelectionType.ShuffleDeck, () => base.GameController.ShuffleLocation(deck,null, GetCardSource()))
                    };

                    SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, null, GetCardSource());
                    IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(choose);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(choose);
                    }

                    if (playResults.Count() > 0)
                    {
                        CannotPlayCardsStatusEffect effect = new CannotPlayCardsStatusEffect();
                        effect.CardCriteria.IsAtLocation = deck;
                        effect.UntilStartOfNextTurn(this.TurnTaker);
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

        private IEnumerator SelectCardResponse(SelectCardDecision scd)
        {
            if (scd.SelectedCard != null)
            {
                var card = scd.SelectedCard;
                Location destination = GetNativeDeck(card);
                //Location destination = card.NativeDeck is null || card.NativeDeck.OwnerTurnTaker != card.Owner ? card.Owner.Deck : card.NativeDeck;

                IEnumerator coroutine = GameController.MoveCard(DecisionMaker, card, destination,
                                    showMessage: true,
                                    decisionSources: new IDecision[] { scd },
                                    evenIfIndestructible: false,
                                    cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }
    }
}

