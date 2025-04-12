using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public class TheBlindedQueenCardController : TheMidnightBazaarUtilityCardController
    {
        public TheBlindedQueenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // If in play: show whether a non-Hound target has already dealt damage to a target other than itself this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(FirstHostileDamageThisTurn), () => "A non-Hound target has already dealt damage to a target other than itself this turn.", () => "No non-Hound targets have dealt damage to targets other than themselves this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // Show any Hounds in the environment trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain(HoundKeyword), "Hound"));
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible to other cards."
            if (card == base.Card)
            {
                if (Journal.GetCardPropertiesBoolean(base.Card, IsShufflingSelf).HasValue)
                {
                    return !((bool)Journal.GetCardPropertiesBoolean(base.Card, IsShufflingSelf));
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        protected const string FirstHostileDamageThisTurn = "FirstHostileDamageThisTurn";
        protected const string IsShufflingSelf = "IsShufflingSelf";
        protected const string HoundKeyword = "hound";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn a non-Hound target deals damage to a target other than itself, increase the next damage dealt to the source of that damage by 2."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstHostileDamageThisTurn) && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsTarget && !base.GameController.DoesCardContainKeyword(dda.DamageSource.Card, HoundKeyword) && dda.Target != dda.DamageSource.Card, AggroResponse, TriggerType.AddStatusEffectToDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstHostileDamageThisTurn), TriggerType.Hidden);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(IsShufflingSelf), TriggerType.Hidden);
            // "At the start of the environment turn, play the top card of the environment deck, then shuffle this card and a Hound from the environment trash into the environment deck."
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, PlayAndShuffleResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.ShuffleCardIntoDeck });
        }

        private IEnumerator AggroResponse(DealDamageAction dda)
        {
            // "... increase the next damage dealt to the source of that damage by 2."
            if (!HasBeenSetToTrueThisTurn(FirstHostileDamageThisTurn))
            {
                SetCardPropertyToTrueIfRealAction(FirstHostileDamageThisTurn);
                IncreaseDamageStatusEffect aggro = new IncreaseDamageStatusEffect(2);
                aggro.TargetCriteria.IsSpecificCard = dda.DamageSource.Card;
                aggro.UntilTargetLeavesPlay(dda.DamageSource.Card);
                aggro.NumberOfUses = 1;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(aggro, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
        }

        private IEnumerator PlayAndShuffleResponse(PhaseChangeAction pca)
        {
            // "... play the top card of the environment deck, ..."
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(pca);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... then shuffle this card and a Hound from the environment trash into the environment deck."
            if (IsRealAction())
            {
                Journal.RecordCardProperties(base.Card, IsShufflingSelf, true);
            }
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.Deck, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            List<Card> found = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Location.IsEnvironment && c.IsInTrash && c.DoKeywordsContain(HoundKeyword), "in the environment trash", false, true, "Hound card", "Hound cards"), visibleToCard: GetCardSource()).ToList();
            if (found.Any())
            {
                // Move the Hound to the deck, then shuffle
                MoveCardDestination dest = new MoveCardDestination(FindEnvironment().TurnTaker.Deck);
                IEnumerator moveHoundCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(DecisionMaker, base.TurnTaker.Trash, new LinqCardCriteria((Card c) => base.GameController.DoesCardContainKeyword(c,HoundKeyword), "Hound"), dest.ToEnumerable(), shuffleAfterwards: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveHoundCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveHoundCoroutine);
                }
            }
            // Shuffle
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: GetCardSource());
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
}
