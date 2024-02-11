using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace VainFacadePlaytest.Doomsayer
{
    public class DoomsayerCharacterCardController : VillainCharacterCardController
    {
        public DoomsayerCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            //AddThisCardControllerToList(CardControllerListType.ChangesVisibility);
        }

        public override void AddTriggers()
        {
            //When a villain character is destroyed, remove it from the game
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.WasCardDestroyed && IsVillain(dc.CardToDestroy.Card) && dc.CardToDestroy.Card.IsCharacter, (DestroyCardAction dc) => base.GameController.MoveCard(this.TurnTakerController, dc.CardToDestroy.Card, this.TurnTaker.OutOfGame, cardSource: GetCardSource()),TriggerType.RemoveFromGame, TriggerTiming.After);

            //When Doomsayer is destroyed, the heroes win
            AddDefeatedIfDestroyedTriggers();
            AddDefeatedIfMovedOutOfGameTriggers();

            //Reduce damage dealt to Doomsayer by 1 for each other villain target in play
            AddReduceDamageTrigger((DealDamageAction dd) => dd.Target == this.Card, (DealDamageAction dd) => FindCardsWhere((Card c) => IsVillainTarget(c) && c != this.Card && c.IsInPlayAndHasGameText).Count());

            //At the start of the villain turn, Doomsayer regains 13 HP
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => base.GameController.GainHP(this.Card, 13, cardSource: GetCardSource()), TriggerType.GainHP);

            base.AddTriggers();
        }

        public override void AddSideTriggers()
        {
            if (!this.CharacterCard.IsFlipped)
            {
                //At the end of the villain turn, shuffle all patterns from the villain trash into the vilain deck,
                //then reveal cards from the top of the villain deck until a pattern is revealed. Put it into play.
                //Shuffle the rest into the villain deck. Then flip this card.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.ShuffleCardIntoDeck, TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.FlipCard }));

                if (base.Game.IsAdvanced)
                {
                    //Advanced: At the start of the villain turn, play the top card of the villain deck
                    AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, PlayTheTopCardOfTheVillainDeckWithMessageResponse, TriggerType.PlayCard));
                }
            }
            else
            {
                //When a villain pattern is moved under Countless Words, flip this card.
                AddSideTrigger(AddTrigger<MoveCardAction>((MoveCardAction mc) => IsVillain(mc.CardToMove) && IsPattern(mc.CardToMove) && mc.Destination.IsUnderCard && mc.Destination.OwnerCard == countlessWords && mc.WasCardMoved, (MoveCardAction mc) => base.GameController.FlipCard(this, cardSource: GetCardSource()), TriggerType.FlipCard, TriggerTiming.After));

                //At the end of the villain turn, Doomsayer deals each hero target 1 infernal damage.
                //If no damage is dealt this way, play the top card of the environment deck.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnFlippedResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard }));

                if (base.Game.IsAdvanced)
                {
                    //Advanced: Increase damage dealt to hero targets by 1
                    AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dd) => IsHeroTarget(dd.Target), 1));
                }
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            //shuffle all patterns from the villain trash into the vilain deck,
            IEnumerator coroutine = base.GameController.ShuffleCardsIntoLocation(DecisionMaker, FindCardsWhere((Card c) => c.Location == this.TurnTaker.Trash && IsPattern(c)), this.TurnTaker.Deck, false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //then reveal cards from the top of the villain deck until a pattern is revealed. Put it into play.
            //Shuffle the rest into the villain deck.
            coroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(this.TurnTakerController, this.TurnTaker.Deck, false, true, false, new LinqCardCriteria((Card c) => IsPattern(c), "", false, false, "pattern", "patterns"), 1, shuffleSourceAfterwards: true, showMessage: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Then flip this card.
            coroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
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
            //At the end of the villain turn, Doomsayer deals each hero target 1 infernal damage.
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator coroutine = DealDamage(this.Card, (Card c) => IsHeroTarget(c), 1, DamageType.Infernal, storedResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If no damage is dealt this way, play the top card of the environment deck.
            if (!DidDealDamage(results))
            {
                coroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(null);
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

        //Villain patterns cannot be affected by non-villain cards
        //Based on Isolated Hero
        public override bool AskIfActionCanBePerformed(GameAction g)
        { 
                bool? flag = g.DoesFirstCardAffectSecondCard((Card c) => !IsVillain(c), (Card c) => IsVillain(c) && IsPattern(c));
                if ((flag.HasValue && flag.Value))
                {
                    return false;
                }

                return true;
        }

        //public override bool? AskIfCardIsVisibleToCardSource(Card card, CardSource cardSource)
        //{
        //    if (cardSource != null && IsHero(cardSource.Card) && IsVillain(card) && IsPattern(card))
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        private bool IsPattern(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "pattern");
        }

        private Card countlessWords => base.TurnTaker.GetCardByIdentifier("TheTalisman");
    }
}

