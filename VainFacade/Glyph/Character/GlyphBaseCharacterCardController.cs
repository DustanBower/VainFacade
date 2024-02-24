using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
    public class GlyphBaseCharacterCardController : HeroCharacterCardController
    {
        public GlyphBaseCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => $"The top card of {this.TurnTaker.Name}'s deck is{GetTypeOfCard(this.TurnTaker.Deck.TopCard)} card.").Condition = () => this.TurnTaker.Deck.HasCards && GetTypeOfCard(this.TurnTaker.Deck.TopCard) != "";
            AddThisCardControllerToList(CardControllerListType.ModifiesKeywords);
            //AddThisCardControllerToList(CardControllerListType.EnteringGameCheck);
        }

        public override void AddStartOfGameTriggers()
        {
            //Move all cards from Glyph's subdecks into her main deck
            if (this.TurnTakerController is GlyphTurnTakerController && this.TurnTaker.SubDecks.Any((Location L) => L.Cards.Any()))
            {
                ((GlyphTurnTakerController)this.TurnTakerController).HandleSubDecks();
            }
        }

        //public override IEnumerator PerformEnteringGameResponse()
        //{
        //    //Move all cards from Glyph's subdecks into her main deck
        //    IEnumerator coroutine = base.GameController.BulkMoveCards(this.TurnTakerController, FindCardsWhere((Card c) => (c.Location.IsSubDeck || c.Location.IsHand) && c.Location.OwnerTurnTaker == this.TurnTaker), this.TurnTaker.Deck, cardSource: new CardSource(this.CharacterCardController));
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    coroutine = base.GameController.ShuffleLocation(this.TurnTaker.Deck, cardSource: new CardSource(this.CharacterCardController));
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    coroutine = base.GameController.BulkMoveCards(this.TurnTakerController, this.TurnTaker.Deck.GetTopCards(4), this.HeroTurnTaker.Hand, cardSource: new CardSource(this.CharacterCardController));
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }
        //}

        public enum CustomDecisionMode
        {
            SelectFirstTurnTakerToSwapCard,
            SelectSecondTurnTakerToSwapCard,
            SelectFirstCardToSwap,
            SelectSecondCardToSwap,
            PlayFaceDownInstead,
            PlayFaceDownFromPower
        };

        public CustomDecisionMode decisionMode;

        public string GetTypeOfCard(Card c)
        {
            if (base.GameController.DoesCardContainKeyword(c, "might", false, true))
            {
                return " a might";
            }
            else if (base.GameController.DoesCardContainKeyword(c, "fate", false, true))
            {
                return " a fate";
            }
            else if (base.GameController.DoesCardContainKeyword(c, "insight", false, true))
            {
                return " an insight";
            }
            else if (base.GameController.DoesCardContainKeyword(c, "death", false, true))
            {
                return " a death";
            }
            else if (base.GameController.DoesCardContainKeyword(c, "destruction", false, true))
            {
                return " a destruction";
            }
            else
            {
                return "";
            }
        }

        public override void AddTriggers()
        {
            //If any of Glyph's cards would end up in a subdeck trash, put it in her regular trash instead
            AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Destination.IsSubTrash && mc.Destination.OwnerTurnTaker == this.TurnTaker, PutInMainTrash, TriggerType.MoveCard , TriggerTiming.After);
            AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Destination.IsSubDeck && mc.Destination.OwnerTurnTaker == this.TurnTaker, PutInMainDeck,TriggerType.MoveCard , TriggerTiming.After);

            //When you would play one of your cards, you may play it face-down in any play area instead of resolving its normal effects.
            AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.ResponsibleTurnTaker == this.TurnTaker && !pca.IsPutIntoPlay && pca.CardToPlay.Owner == this.TurnTaker, PlayFaceDownInstead, TriggerType.MoveCard, TriggerTiming.Before);

            //Handle keyword change for Return to Silence
            AddTrigger<ExpireStatusEffectAction>((ExpireStatusEffectAction exp) => exp.StatusEffect is ReturnToSilenceStatusEffect, CleanUpKeywordsResponse, TriggerType.Hidden, TriggerTiming.After);

            //AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => base.GameController.MoveCard(DecisionMaker, this.HeroTurnTaker.Deck.TopCard, this.Game.TurnTakers.ToList()[3].Deck, cardSource: GetCardSource()), TriggerType.MoveCard);

            //Some cards that reveal and play cards from Glyph's deck do not clean up the revealed cards if they cannot be played (e.g. Mission Control). Move any cards left in Glyph's revealed cards to her trash.
            AddPhaseChangeTrigger((TurnTaker tt) => true, (Phase p) => true, (PhaseChangeAction pca) => this.TurnTaker.Revealed.Cards.Any(), (PhaseChangeAction pca) => CleanupCardsAtLocations(new Location[] { this.TurnTaker.Revealed }.ToList(), this.TurnTaker.Trash, isReturnedToOriginalLocation: false, isDiscard: false),new TriggerType[] {TriggerType.Hidden, TriggerType.MoveCard },TriggerTiming.Before);

            //If Santa Guise flips one of Glyph's limited cards when another copy is in play, it doesn't get moved to the trash.
            AddTrigger<FlipCardAction>((FlipCardAction fc) => FindCardController(fc.CardToFlip.Card) is GlyphLimitedCard && FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Title == fc.CardToFlip.Card.Title && c != fc.CardToFlip.Card).Any(), HandleLimited, TriggerType.MoveCard, TriggerTiming.After);
        }

        private IEnumerator PlayFaceDownInstead(PlayCardAction pca)
        {
            Card card = pca.CardToPlay;
            List<YesNoCardDecision> results = new List<YesNoCardDecision>();

            decisionMode = CustomDecisionMode.PlayFaceDownInstead;
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, this.Card, null, results, new Card[] { card }, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidPlayerAnswerYes(results))
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
                    coroutine = CancelAction(pca, false);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    coroutine = base.GameController.MoveCard(DecisionMaker, card, loc, playCardIfMovingToPlayArea: false, flipFaceDown: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    SetCardProperty("ABrushWithDeathTracking", true);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            CustomDecisionText result = null;
            string text;
            if (decisionMode == CustomDecisionMode.PlayFaceDownInstead)
            {
                result = new CustomDecisionText(
                $"Do you want to play this card face-down?",
                $"The other heroes are choosing whether to play this card face-down.",
                $"Vote for whether to play this card face-down.",
                $"whether to play this card face-down."
                );
            }
            else if (decisionMode == CustomDecisionMode.SelectFirstTurnTakerToSwapCard)
            {
                text = "the first play area to swap a card from.";
                result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
            }
            else if (decisionMode == CustomDecisionMode.SelectSecondTurnTakerToSwapCard)
            {
                text = "the second play area to swap a card from.";
                result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
            }
            else if (decisionMode == CustomDecisionMode.SelectFirstCardToSwap)
            {
                text = "the first card to swap.";
                result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
            }
            else if (decisionMode == CustomDecisionMode.SelectSecondCardToSwap)
            {
                text = "the second card to swap.";
                result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
            }
            else if (decisionMode == CustomDecisionMode.PlayFaceDownFromPower)
            {
                text = "a card to play face-down";
                result = new CustomDecisionText(
                $"Select {text}.",
                $"The other heroes are choosing {text}.",
                $"Vote for {text}.",
                $"{text}."
                );
            }
            return result;
        }

        private IEnumerator PutInMainTrash(MoveCardAction mc)
        {
            IEnumerator coroutine = base.GameController.MoveCard(this.TurnTakerController, mc.CardToMove, this.TurnTaker.Trash,cardSource: mc.CardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator PutInMainDeck(MoveCardAction mc)
        {
            bool ToBottom = mc.ToBottom;
            IEnumerator coroutine = base.GameController.MoveCard(this.TurnTakerController, mc.CardToMove, this.HeroTurnTaker.Deck, toBottom: ToBottom, cardSource: mc.CardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        //Handle keyword change from Return to Silence
        private List<Location> GetAffectedPlayAreas()
        {
            return base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is ReturnToSilenceStatusEffect).Select((StatusEffectController sec) => ((ReturnToSilenceStatusEffect)sec.StatusEffect).PlayArea).Distinct().ToList();
        }

        public override bool AskIfCardContainsKeyword(Card card, string keyword, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
        {
            List<Location> affected = GetAffectedPlayAreas();
            if (affected != null && keyword == "ongoing" && !card.IsTarget && !card.IsCharacter && affected.Contains(card.Location.HighestRecursiveLocation) && base.GameController.IsCardVisibleToCardSource(card,GetCardSource()) && !card.IsFaceDownNonCharacter && !card.IsUnderCard)
            {
                return true;
            }
            return base.AskIfCardContainsKeyword(card, keyword, evenIfUnderCard, evenIfFaceDown);
        }

        public override IEnumerable<string> AskForCardAdditionalKeywords(Card card)
        {
            List<Location> affected = GetAffectedPlayAreas();
            if (affected != null && !card.IsTarget && !card.IsCharacter && affected.Contains(card.Location.HighestRecursiveLocation) && base.GameController.IsCardVisibleToCardSource(card, GetCardSource()) && !card.IsFaceDownNonCharacter && !card.IsUnderCard)
            {
                return new string[1]
                {
                "ongoing"
                };
            }
            return base.AskForCardAdditionalKeywords(card);
        }

        private IEnumerator CleanUpKeywordsResponse(ExpireStatusEffectAction exp)
        {
            Location playArea = ((ReturnToSilenceStatusEffect)exp.StatusEffect).PlayArea;
            List<Location> affectedAreas = GetAffectedPlayAreas();
            if (!affectedAreas.Contains(playArea))
            {
                List<Card> affectedCards = FindCardsWhere((Card c) => !c.IsTarget && !c.IsCharacter && c.Location.HighestRecursiveLocation == playArea && !c.IsOngoing).ToList();
                IEnumerator coroutine = base.GameController.ModifyKeywords("ongoing", addingOrRemoving: false, affectedCards, GetCardSource());
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

        private IEnumerator HandleLimited(FlipCardAction fc)
        {
            IEnumerator coroutine = base.GameController.SendMessageAction($"{this.TurnTaker.Name} tried to play {fc.CardToFlip.Card.Title}, but it is a limited card that is already in play. Moving it to {this.TurnTaker.Trash.GetFriendlyName()}.",Priority.High,GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.MoveCard(this.TurnTakerController, fc.CardToFlip.Card, this.TurnTaker.Trash, cardSource: GetCardSource());
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