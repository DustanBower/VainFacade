using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
	public class TheBaronessSpiderCharacterCardController: VillainCharacterCardController
    {
		public TheBaronessSpiderCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);

            base.SpecialStringMaker.ShowSpecialString(CardsInHandSpecialString).Condition = () => !this.Card.IsFlipped;
            base.SpecialStringMaker.ShowSpecialString(CardsInTrashSpecialString).Condition = () => !this.Card.IsFlipped;
            base.SpecialStringMaker.ShowSpecialString(CardsInPlaySpecialString).Condition = () => !this.Card.IsFlipped;

            base.SpecialStringMaker.ShowHighestHP(1, null, new LinqCardCriteria(IsHeroTarget, "", false, false, "hero target", "hero targets")).Condition = () => this.Card.IsFlipped;
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstSchemeThisTurn, "A scheme has already entered or left play this turn", "A scheme has not entered or left play this turn").Condition = () => this.Card.IsFlipped;
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(First5DamageThisTurn, $"{this.Card.Title} has already been dealt 5 damage this turn", $"{this.Card.Title} has not been dealt 5 damage this turn").Condition = () => this.Card.IsFlipped;
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDrawThisTurn, "A card has already been drawn this turn", "No cards have been drawn this turn").Condition = () => this.Card.IsFlipped;
        }

        private string CardsInHandSpecialString()
        {
            int max = base.Game.HeroTurnTakers.Select((HeroTurnTaker htt) => htt.NumberOfCardsInHand).Max();
            List<string> names = base.Game.HeroTurnTakers.Where((HeroTurnTaker htt) => htt.NumberOfCardsInHand == max).Select((HeroTurnTaker htt) => htt.Name).ToList();
            return $"{names.ToCommaList(true)} {(names.Count() == 1 ? "has" : "have")} {max} cards in their hand";
        }

        private string CardsInTrashSpecialString()
        {
            int max = base.Game.HeroTurnTakers.Select((HeroTurnTaker htt) => htt.Trash.NumberOfCards).Max();
            List<string> names = base.Game.HeroTurnTakers.Where((HeroTurnTaker htt) => htt.Trash.NumberOfCards == max).Select((HeroTurnTaker htt) => htt.Name).ToList();
            return $"{names.ToCommaList(true)} {(names.Count() == 1 ? "has" : "have")} {max} cards in their trash";
        }

        private string CardsInPlaySpecialString()
        {
            int max = base.Game.HeroTurnTakers.Select((HeroTurnTaker htt) => FindCardsWhere((Card c) => !c.IsCharacter && c.Owner == htt && c.IsInPlayAndHasGameText).Count()).Max();
            List<string> names = base.Game.HeroTurnTakers.Where((HeroTurnTaker htt) => FindCardsWhere((Card c) => !c.IsCharacter && c.Owner == htt && c.IsInPlayAndHasGameText).Count() == max).Select((HeroTurnTaker htt) => htt.Name).ToList();
            return $"{names.ToCommaList(true)} {(names.Count() == 1 ? "has" : "have")} {max} non-character cards in play";
        }

        protected const string VampirismIdentifier = "Vampirism";
        protected const string SchemeKeyword = "scheme";
        protected const string First5DamageThisTurn = "First5DamageThisTurn";
        protected const string FirstDrawThisTurn = "FirstDrawThisTurn";
        protected const string FirstSchemeThisTurn = "FirstSchemeThisTurn";

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "Vampirism is indestructible."
            if (card.Owner == base.TurnTaker && card.Identifier == VampirismIdentifier)
            {
                return true;
            }
            return false;
        }

        public override void AddSideTriggers()
        {
            AddDefeatedIfDestroyedTriggers();

            if (!this.Card.IsFlipped)
            {
                //Cards cannot be played or put into play from the villain deck.
                //CannotPlayCards((TurnTakerController ttc) => ttc.TurnTaker == this.TurnTaker && !this.Card.IsFlipped);

                //At the start of the villain turn, remove each villain web from the game and flip The Baroness's character cards if any of the following conditions are met:
                //Any player has 7 or more cards in their hand.
                //Any player has 7 or more cards in their trash.
                //Any player has 3 or more non-character cards in play.
                //There are no villain web targets in play.
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, FlipResponse, new TriggerType[] {TriggerType.RemoveFromGame, TriggerType.FlipCard}, FlipCriteria));

                //When a player would draw a card when they have 6 cards in hand, present a choice
                AddSideTrigger(AddTrigger<DrawCardAction>((DrawCardAction dc) => dc.HeroTurnTaker.NumberOfCardsInHand == 6,(DrawCardAction dc) => DoNothing(), TriggerType.Other, TriggerTiming.Before));

                //Advanced: Reduce damage dealt to webs by 1.
                if (base.Game.IsAdvanced)
                {
                    AddSideTrigger(AddReduceDamageTrigger((Card c) => base.GameController.DoesCardContainKeyword(c, "web"), 1));
                }
            }
            else
            {
                //Radiant damage dealt to villain targets is irreducible.
                AddSideTrigger(AddMakeDamageIrreducibleTrigger((DealDamageAction dd) => IsVillainTarget(dd.Target) && dd.DamageType == DamageType.Radiant));

                //The first time each turn a scheme enters or leaves play, play the top card of the villain deck.
                AddSideTrigger(AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cep) => !IsPropertyTrue(FirstSchemeThisTurn) && base.GameController.DoesCardContainKeyword(cep.CardEnteringPlay, SchemeKeyword), (CardEntersPlayAction cep) => SchemeResponse(), TriggerType.PlayCard, TriggerTiming.After));
                AddSideTrigger(AddTrigger<MoveCardAction>((MoveCardAction mc) => !IsPropertyTrue(FirstSchemeThisTurn) && base.GameController.DoesCardContainKeyword(mc.CardToMove, SchemeKeyword) && !mc.Destination.IsInPlay && mc.Origin.IsInPlay, (MoveCardAction mc) => SchemeResponse(), TriggerType.PlayCard, TriggerTiming.After));

                //The first time each turn The Baroness is dealt 5 or more damage at once, put a non-character hero card in play face-down in the villain play area.
                AddSideTrigger(AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(First5DamageThisTurn) && dd.Target == this.Card && dd.DidDealDamage && dd.Amount >= 5, GainBloodResponse, TriggerType.MoveCard, TriggerTiming.After));

                //The first time each turn each player draws a card, increase the next damage dealt by The Baroness by 1.
                AddSideTrigger(AddTrigger<DrawCardAction>((DrawCardAction dc) => !IsPropertyTrue(FirstDrawThisTurn), FirstDrawResponse, TriggerType.CreateStatusEffect, TriggerTiming.After, isActionOptional: false));

                //At the end of the villain turn, play the top card of the villain deck.
                //Then, The Baroness deals the hero target with the highest HP H melee and H infernal damage.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnFlippedResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DealDamage }));

                //Advanced: Increase damage dealt by The Baroness by 1.
                if (base.Game.IsAdvanced)
                {
                    AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsSameCard(this.Card), 1));
                }
            }

            if (base.Game.IsChallenge)
            {
                //At the end of the villain turn, put the top card of each hero deck face down in the villain play area.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnChallengeResponse, TriggerType.MoveCard));
            }
        }

        private bool FlipCriteria(PhaseChangeAction pca)
        {
            //Any player has 7 or more cards in their hand.
            //Any player has 7 or more cards in their trash.
            //Any player has 3 or more non-character cards in play.
            //There are no villain web targets in play.

            bool flag = FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().NumberOfCardsInHand >= 7).Any();
            flag |= FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && tt.Trash.NumberOfCards >= 7).Any();
            flag |= FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && FindCardsWhere((Card c) => c.Owner == tt && c.IsInPlay && !c.IsCharacter).Count() >= 3).Any();
            flag |= !FindCardsWhere((Card c) => IsVillain(c) && base.GameController.DoesCardContainKeyword(c, "web") && c.IsTarget && c.IsInPlayAndHasGameText).Any();
            return flag;
        }

        private IEnumerator FlipResponse(PhaseChangeAction pca)
        {
            //remove each villain web from the game and flip The Baroness's character cards
            IEnumerator coroutine = base.GameController.BulkMoveCards(this.TurnTakerController, FindCardsWhere((Card c) => IsVillain(c) && base.GameController.DoesCardContainKeyword(c, "web")), this.TurnTaker.OutOfGame, cardSource: GetCardSource(), responsibleTurnTaker: this.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);

            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = FlipThisCharacterCardResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);

            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator EndOfTurnFlippedResponse(PhaseChangeAction pca)
        {
            //At the end of the villain turn, play the top card of the villain deck.
            //Then, The Baroness deals the hero target with the highest HP H melee and H infernal damage.
            IEnumerator coroutine = PlayTheTopCardOfTheVillainDeckWithMessageResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);

            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            DealDamageAction damage1 = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.Card), null, H, DamageType.Melee);
            DealDamageAction damage2 = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.Card), null, H, DamageType.Infernal);

            coroutine = DealMultipleInstancesOfDamageToHighestLowestHP(new DealDamageAction[] { damage1, damage2 }.ToList(), IsHeroTarget, HighestLowestHP.HighestHP);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);

            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator SchemeResponse()
        {
            SetCardProperty(FirstSchemeThisTurn,true);
            IEnumerator coroutine = PlayTheTopCardOfTheVillainDeckWithMessageResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);

            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator GainBloodResponse(DealDamageAction dd)
        {
            //The first time each turn The Baroness is dealt 5 or more damage at once, put a non-character hero card in play face-down in the villain play area.
            SetCardPropertyToTrueIfRealAction(First5DamageThisTurn);
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardFaceDownToVillainPlayArea, new LinqCardCriteria((Card c) => IsHero(c) && !c.IsCharacter && c.IsInPlayAndHasGameText && !c.IsOneShot), results, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);

            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Card selected = GetSelectedCard(results);
            if (selected != null)
            {
                //coroutine = base.GameController.MoveCard(base.TurnTakerController, selected, base.TurnTaker.PlayArea, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: false, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: true, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
                //if (base.UseUnityCoroutines)
                //{
                //    yield return base.GameController.StartCoroutine(coroutine);
                //}
                //else
                //{
                //    base.GameController.ExhaustCoroutine(coroutine);
                //}

                //Move the selected card to the villain play area
                coroutine = base.GameController.MoveCard(base.TurnTakerController, selected, base.TurnTaker.PlayArea, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, flipFaceDown: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                //Get list of cards in villain play area
                List<Card> list = GetOrderedCardsInLocation(TurnTaker.PlayArea).Where((Card c) => !c.IsCharacter).ToList();

                //Put the moved card last in the list of cards, so it goes to the end of the play area
                list.Remove(selected);
                list.Add(selected);
                list.ForEach(delegate (Card c)
                {
                    base.GameController.Game.AssignPlayCardIndex(c);
                });

                //Flip the card face-down after moving it, so that the UI refreshes and the card goes to the end of the play area
                coroutine = base.GameController.FlipCard(FindCardController(selected), cardSource: GetCardSource());
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

        private IEnumerable<Card> GetOrderedCardsInLocation(Location location)
        {
            return location.Cards.OrderBy((Card c) => c.PlayIndex);
        }

        private IEnumerator FirstDrawResponse(DrawCardAction dc)
        {
            SetCardProperty(FirstDrawThisTurn, true);
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
            effect.NumberOfUses = 1;
            effect.SourceCriteria.IsSpecificCard = this.Card;

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

        private IEnumerator EndOfTurnChallengeResponse(PhaseChangeAction pca)
        {
            //At the end of the villain turn, put the top card of each hero deck face down in the villain play area.
            IEnumerator coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame && tt.Deck.HasCards, "heroes with cards in their decks"), SelectionType.MoveCardFaceDownToVillainPlayArea, ChallengeMoveCards, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator ChallengeMoveCards(TurnTaker tt)
        {
            IEnumerator coroutine = base.GameController.MoveCard(base.TurnTakerController, tt.Deck.TopCard, base.TurnTaker.PlayArea, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: false, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: true, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator coroutine = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.ChangeMaximumHP(this.Card, this.Card.Definition.FlippedHitPoints.Value, true, GetCardSource());
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