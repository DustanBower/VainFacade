using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class EldrenwoodVillageCardController : VillainCharacterCardController
    {
        public EldrenwoodVillageCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.ActivatesEffects);
            AddThisCardControllerToList(CardControllerListType.ModifiesKeywords);
            // Back side: how many Small Werewolf cards in play?
            SpecialStringMaker.ShowNumberOfCardsInPlay(smallWerewolf).Condition = () => base.Card.IsFlipped;
            // Back side: who is the non-Werewolf with the lowest HP?
            SpecialStringMaker.ShowLowestHP(1, () => 1, nonWerewolf).Condition = () => base.Card.IsFlipped;
            // Back side: how many Clever Werewolf cards in play?
            SpecialStringMaker.ShowNumberOfCardsInPlay(cleverWerewolf).Condition = () => base.Card.IsFlipped;
            // Back side: who is the non-Werewolf with the second highest HP?
            SpecialStringMaker.ShowHighestHP(2, () => 1, nonWerewolf).Condition = () => base.Card.IsFlipped;
            // Back side: how many Common Werewolf cards in play?
            SpecialStringMaker.ShowNumberOfCardsInPlay(commonWerewolf).Condition = () => base.Card.IsFlipped;
            // Back side: who is the non-Werewolf with the second lowest HP?
            SpecialStringMaker.ShowLowestHP(2, () => 1, nonWerewolf).Condition = () => base.Card.IsFlipped;
        }

        protected const string HowlsKey = "HowlsEffectKey";
        protected const string QuaintKey = "QuaintEffectKey";

        protected const string AfflictedKeyword = "afflicted";
        protected const string WerewolfKeyword = "werewolf";
        protected const string TriggerKeyword = "trigger";
        protected const string SmallKeyword = "small";
        protected const string CleverKeyword = "clever";
        protected const string CommonKeyword = "common";

        protected LinqCardCriteria afflictedTarget = new LinqCardCriteria((Card c) => c.IsTarget && c.DoKeywordsContain(AfflictedKeyword), "Afflicted");
        protected LinqCardCriteria nonWerewolf = new LinqCardCriteria((Card c) => c.IsTarget && !c.DoKeywordsContain(WerewolfKeyword), "non-Werewolf");
        protected LinqCardCriteria smallWerewolf = new LinqCardCriteria((Card c) => c.DoKeywordsContain(SmallKeyword) && c.DoKeywordsContain(WerewolfKeyword), "Small Werewolf");
        protected LinqCardCriteria cleverWerewolf = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CleverKeyword) && c.DoKeywordsContain(WerewolfKeyword), "Clever Werewolf");
        protected LinqCardCriteria commonWerewolf = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CommonKeyword) && c.DoKeywordsContain(WerewolfKeyword), "Common Werewolf");
        protected LinqCardCriteria triggerInPlay = new LinqCardCriteria((Card c) => c.DoKeywordsContain(TriggerKeyword) && c.IsInPlayAndHasGameText, "Trigger");

        public override bool? AskIfActivatesEffect(TurnTakerController turnTakerController, string effectKey)
        {
            bool? result = null;
            if (!base.Card.IsFlipped)
            {
                // If "Quaint Country Town" is up, then QuaintKey effects are active
                if (turnTakerController == base.TurnTakerController && effectKey == QuaintKey)
                {
                    result = true;
                }
            }
            else
            {
                // If "Howls in the Distance" is up, then HowlsKey effects are active
                if (turnTakerController == base.TurnTakerController && effectKey == HowlsKey)
                {
                    result = true;
                }
            }
            return result;
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "When a Trigger enters play, reveal cards from the top of the environment deck until an Afflicted card is revealed. Put it into play. Shuffle the remaining cards back into the environment deck. Then flip this card."
                AddSideTrigger(AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.DoKeywordsContain(TriggerKeyword), FindAfflictedFlipResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.FlipCard }, TriggerTiming.After));
            }
            else
            {
                // Back side:
                // "Reduce damage dealt to Werewolves by 2."
                AddSideTrigger(AddReduceDamageTrigger((Card c) => c.DoKeywordsContain(WerewolfKeyword), 2));
                // "At the start of the environment turn, if there are no Triggers in play, flip this card."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, FlipThisCharacterCardResponse, TriggerType.FlipCard, (PhaseChangeAction pca) => FindCardsWhere(triggerInPlay, visibleToCard: GetCardSource()).Count() <= 0));
                // "At the end of the environment turn, each Small Werewolf deals the non-Werewolf with the lowest HP {H - 2} irreducible melee damage."
                // "At the end of the environment turn, each Clever Werewolf deals the non-Werewolf with the second highest HP {H + 1} melee damage."
                // "At the end of the environment turn, each Common Werewolf deals the non-Werewolf with the second lowest HP {H} melee damage."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SmallCleverCommonDamageResponse, TriggerType.DealDamage));
            }
        }

        public override bool AskIfCardContainsKeyword(Card card, string keyword, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
        {
            // Back side: "Afflicted targets are Werewolves"
            if (base.Card.IsFlipped && keyword == WerewolfKeyword && card.IsTarget && card.DoKeywordsContain(AfflictedKeyword) && card.BattleZone == base.CardWithoutReplacements.BattleZone)
            {
                return true;
            }
            return base.AskIfCardContainsKeyword(card, keyword, evenIfUnderCard, evenIfFaceDown);
        }

        public override IEnumerable<string> AskForCardAdditionalKeywords(Card card)
        {
            // Back side: "Afflicted targets are Werewolves"
            if (base.Card.IsFlipped && card.IsTarget && card.DoKeywordsContain(AfflictedKeyword) && card.BattleZone == base.CardWithoutReplacements.BattleZone)
            {
                return new string[1] { WerewolfKeyword };
            }
            return base.AskForCardAdditionalKeywords(card);
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator baseCoroutine = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(baseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(baseCoroutine);
            }
            if (base.Card.IsFlipped)
            {
                // When flipped to Howls in the Distance, add Werewolf keyword where appropriate
                List<Card> affectedCards = FindCardsWhere(afflictedTarget).ToList();
                IEnumerator addCoroutine = base.GameController.ModifyKeywords(WerewolfKeyword, true, affectedCards, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(addCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(addCoroutine);
                }
            }
            else
            {
                // When flipped to Quaint Country Town, remove Werewolf keyword where added
                List<Card> affectedCards = FindCardsWhere(afflictedTarget).ToList();
                IEnumerator removeCoroutine = base.GameController.ModifyKeywords(WerewolfKeyword, false, affectedCards, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(removeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(removeCoroutine);
                }

                // Front side: "When this card flips to this side, shuffle the environment trash into the the environment deck."
                IEnumerator shuffleCoroutine = base.GameController.ShuffleTrashIntoDeck(base.TurnTakerController, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
        }

        public IEnumerator FindAfflictedFlipResponse(CardEntersPlayAction cepa)
        {
            // "... reveal cards from the top of the environment deck until an Afflicted card is revealed. Put it into play. Shuffle the remaining cards back into the environment deck."
            List<Card> revealedCards = new List<Card>();
            IEnumerator findCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, false, true, false, new LinqCardCriteria((Card c) => c.DoKeywordsContain(AfflictedKeyword), "Afflicted"), 1, storedPlayResults: revealedCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            List<Location> places = new List<Location>();
            places.Add(base.TurnTaker.Revealed);
            IEnumerator cleanupCoroutine = CleanupCardsAtLocations(places, base.TurnTaker.Deck, cardsInList: revealedCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanupCoroutine);
            }
            // "Then flip this card."
            IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource(), allowBackToFront: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(flipCoroutine);
            }
        }

        private IEnumerator SmallCleverCommonDamageResponse(PhaseChangeAction pca)
        {
            // "... each Small Werewolf deals the non-Werewolf with the lowest HP {H - 2} irreducible melee damage."
            IEnumerator smallCoroutine = MultipleDamageSourcesDealDamage(smallWerewolf, TargetType.LowestHP, 1, nonWerewolf, H - 2, DamageType.Melee, isIrreducible: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(smallCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(smallCoroutine);
            }
            // "... each Clever Werewolf deals the non-Werewolf with the second highest HP {H + 1} melee damage."
            IEnumerator cleverCoroutine = MultipleDamageSourcesDealDamage(cleverWerewolf, TargetType.HighestHP, 2, nonWerewolf, H + 1, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleverCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleverCoroutine);
            }
            // "... each Common Werewolf deals the non-Werewolf with the second lowest HP {H} melee damage."
            IEnumerator commonCoroutine = MultipleDamageSourcesDealDamage(commonWerewolf, TargetType.LowestHP, 2, nonWerewolf, H, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(commonCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(commonCoroutine);
            }
        }
    }
}
