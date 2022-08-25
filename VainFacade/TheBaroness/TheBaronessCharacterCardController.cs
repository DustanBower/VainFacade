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
    internal class TheBaronessCharacterCardController : VillainCharacterCardController
    {
        public TheBaronessCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);

            // Both sides: show how much damage The Baroness has been dealt this turn
            SpecialStringMaker.ShowDamageTaken(base.Card, showInEffectsList: () => true, thisTurn: true);
            // Both sides: show whether The Baroness has dealt damage to a hero target this turn
            SpecialStringMaker.ShowIfElseSpecialString(DidHitHeroTargetThisTurn, () => base.Card.Title + " has already dealt a hero target damage this turn.", () => base.Card.Title + " has not dealt a hero target damage this turn.");

            // Front side: show 2 heroes with lowest HP
            SpecialStringMaker.ShowHeroCharacterCardWithLowestHP(ranking: 1, numberOfTargets: 2).Condition = () => !base.Card.IsFlipped;
            // Front side: show hero with highest HP
            SpecialStringMaker.ShowHeroCharacterCardWithHighestHP(ranking: 1, numberOfTargets: 1).Condition = () => !base.Card.IsFlipped;

            // Back side: show whether a hero card has been put face-down in the villain play area this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstBloodThisTurn, "A hero card has already been put face-down in the villain play area this turn.", "No hero cards have been put face-down in the villain play area this turn.").Condition = () => base.Card.IsFlipped;
            // Back side: show number of villain Schemes in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(SchemeKeyword), "villain Scheme")).Condition = () => base.Card.IsFlipped;
            // Back side: show hero target with lowest HP
            SpecialStringMaker.ShowHeroTargetWithLowestHP(ranking: 1, numberOfTargets: 1).Condition = () => base.Card.IsFlipped;

            // Challenge: show whether a Scheme has entered play this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstSchemeThisTurn, "A Scheme card has already entered play this turn.", "No Scheme cards have entered play this turn.").Condition = () => base.IsGameChallenge;
        }

        protected const string VampirismIdentifier = "Vampirism";
        protected const string SchemeKeyword = "scheme";
        protected const string FirstBloodThisTurn = "FirstBloodThisTurn";
        protected const string PlayedBonusThisTurn = "PlayedBonusThisTurn";
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

        private int NumVillainSchemesInPlay()
        {
            return FindCardsWhere(new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(SchemeKeyword) && c.IsInPlayAndHasGameText, "villain Scheme"), visibleToCard: GetCardSource()).Count();
        }

        private bool DidHitHeroTargetThisTurn()
        {
            IEnumerable<DealDamageJournalEntry> list = Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.SourceCard != null && ddje.SourceCard == base.Card && ddje.TargetCard != null && ddje.TargetCard.IsHero && ddje.Amount > 0);
            return list.Any();
        }

        private int DamageTakenThisTurn()
        {
            IEnumerable<DealDamageJournalEntry> list = Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.TargetCard == base.Card);
            return (from e in list select e.Amount).Sum();
        }

        public override void AddTriggers()
        {
            if (base.IsGameChallenge)
            {
                // "The first time a Scheme enters play each turn, play the top card of the villain deck."
                AddTrigger((CardEntersPlayAction cepa) => !HasBeenSetToTrueThisTurn(FirstSchemeThisTurn) && cepa.CardEnteringPlay.DoKeywordsContain(SchemeKeyword), PlayCardForSchemeResponse, TriggerType.PlayCard, TriggerTiming.After);
            }
            base.AddTriggers();
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Increase radiant damage dealt to {TheBaroness} by 1."
                base.AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageType == DamageType.Radiant && dd.Target == base.Card, 1));
                // "When {TheBaroness} is dealt 6 or more damage in a turn, play the top card of the villain deck."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(PlayedBonusThisTurn) && dda.Target == base.Card && dda.DidDealDamage && DamageTakenThisTurn() > 5, PlayCardForDamageResponse, TriggerType.PlayCard, TriggerTiming.After));

                // "When a hero card destroys a villain Scheme, {TheBaroness} deals the 2 heroes with the lowest HP {H - 2} melee damage each."
                base.AddSideTrigger(AddTrigger((DestroyCardAction dca) => dca.CardToDestroy.Card.IsVillain && dca.CardToDestroy.Card.DoKeywordsContain(SchemeKeyword) && dca.WasCardDestroyed && dca.CardSource != null && dca.CardSource.Card.IsHero, HitTwoLowestResponse, TriggerType.DealDamage, TriggerTiming.After));
                // "When there are no villain Schemes in play, flip {TheBaroness}'s character card."
                base.AddSideTrigger(AddTrigger((GameAction a) => a.CardSource != null && NumVillainSchemesInPlay() == 0, (GameAction a) => base.GameController.FlipCard(this, cardSource: GetCardSource()), TriggerType.FlipCard, TriggerTiming.After));
                base.AddSideTrigger(AddTrigger((PhaseChangeAction pca) => NumVillainSchemesInPlay() == 0, (PhaseChangeAction pca) => base.GameController.FlipCard(this, cardSource: GetCardSource()), TriggerType.FlipCard, TriggerTiming.After));
                // "At the end of the villain turn, {TheBaroness} deals the hero with the highest HP {H - 1} melee damage. Then, unless {TheBaroness} dealt a hero target damage this turn, destroy {H} hero Ongoing and/or Equipment cards."
                base.AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HitHighestOrDestroyResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard }));
                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // "Increase damage dealt by {TheBaroness} by 1."
                    base.AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card == base.Card, (DealDamageAction dda) => 1));
                }
            }
            else
            {
                // Back side:
                // "Increase radiant damage dealt to {TheBaroness} by 1."
                base.AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageType == DamageType.Radiant && dd.Target == base.Card, 1));
                // "When {TheBaroness} is dealt 6 or more damage in a turn, play the top card of the villain deck."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(PlayedBonusThisTurn) && dda.Target == base.Card && dda.DidDealDamage && DamageTakenThisTurn() > 5, PlayCardForDamageResponse, TriggerType.PlayCard, TriggerTiming.After));

                // "The first time each turn a hero card is put face-down into the villain play area, put the top card of its associated deck face-down in the villain play area."
                AddSideTrigger(AddTrigger((MoveCardAction mca) => !HasBeenSetToTrueThisTurn(FirstBloodThisTurn) && mca.CardToMove.IsFaceDownNonCharacter && mca.CardToMove.IsHero && mca.Destination.IsPlayAreaOf(base.TurnTaker), FirstBloodTakenResponse, TriggerType.MoveCard, TriggerTiming.After));
                // "At the end of the villain turn, discard the top {H + 2} cards of the villain deck. Put any Schemes discarded this way into play."
                // "Then if there is at least 1 villain Scheme in play, flip {TheBaroness}'s character card."
                // "Then {TheBaroness} deals the hero target with the lowest HP {H - 2} melee damage. Unless {TheBaroness} dealt a hero target damage this turn, destroy {H} hero Ongoing and/or Equipment cards."
                base.AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardSchemeFlipHitLowestOrDestroyResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay, TriggerType.FlipCard, TriggerType.DealDamage, TriggerType.DestroyCard }));
                if (base.IsGameAdvanced)
                {
                    // Back side, Advanced:
                    // "At the end of the villain turn, {TheBaroness} regains {H - 2} HP."
                    base.AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.GainHP(base.Card, H - 2, cardSource: GetCardSource()), TriggerType.GainHP));
                }
            }
            AddDefeatedIfDestroyedTriggers();
        }

        private IEnumerator PlayCardForSchemeResponse(CardEntersPlayAction cepa)
        {
            // "... play the top card of the villain deck."
            SetCardPropertyToTrueIfRealAction(FirstSchemeThisTurn);
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(cepa);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator PlayCardForDamageResponse(DealDamageAction dda)
        {
            // "... play the top card of the villain deck."
            SetCardPropertyToTrueIfRealAction(PlayedBonusThisTurn);
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(dda);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator HitTwoLowestResponse(DestroyCardAction dca)
        {
            // "... {TheBaroness} deals the 2 heroes with the lowest HP {H - 2} melee damage each."
            IEnumerator damageCoroutine = DealDamageToLowestHP(base.Card, 1, (Card c) => c.IsHeroCharacterCard, (Card c) => H - 2, DamageType.Melee, numberOfTargets: 2);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }

        private IEnumerator FirstBloodTakenResponse(MoveCardAction mca)
        {
            // "... put the top card of its associated deck face-down in the villain play area."
            SetCardPropertyToTrueIfRealAction(FirstBloodThisTurn);
            if (mca.CardToMove != null)
            {
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, GetNativeDeck(mca.CardToMove).TopCard, base.TurnTaker.PlayArea, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, flipFaceDown: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
        }

        private IEnumerator HitHighestOrDestroyResponse(PhaseChangeAction pca)
        {
            // "... {TheBaroness} deals the hero with the highest HP {H - 1} melee damage."
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => c.IsHeroCharacterCard, (Card c) => H - 1, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Then, unless {TheBaroness} dealt a hero target damage this turn, destroy {H} hero Ongoing and/or Equipment cards."
            if (!DidHitHeroTargetThisTurn())
            {
                LinqCardCriteria heroOngEqp = new LinqCardCriteria((Card c) => c.IsHero && (c.IsOngoing || IsEquipment(c)) && c.IsInPlayAndHasGameText, "hero Ongoing or Equipment");
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, heroOngEqp, H, requiredDecisions: H, allowAutoDecide: H >= base.GameController.FindCardsWhere(heroOngEqp, visibleToCard: GetCardSource()).Count(), responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator DiscardSchemeFlipHitLowestOrDestroyResponse(PhaseChangeAction pca)
        {
            // "... discard the top {H + 2} cards of the villain deck. Put any Schemes discarded this way into play."
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(base.TurnTakerController, H + 2, storedResults: results, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            foreach (MoveCardAction mca in results)
            {
                if (mca.CardToMove.DoKeywordsContain(SchemeKeyword))
                {
                    IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, mca.CardToMove, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(putCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(putCoroutine);
                    }
                }
            }
            // "Then if there is at least 1 villain Scheme in play, flip {TheBaroness}'s character card."
            if (NumVillainSchemesInPlay() >= 1)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
                yield break;
            }
            // "Then {TheBaroness} deals the hero target with the lowest HP {H - 2} melee damage."
            IEnumerator damageCoroutine = DealDamageToLowestHP(base.Card, 1, (Card c) => c.IsHero && c.IsTarget, (Card c) => H - 2, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Unless {TheBaroness} dealt a hero target damage this turn, destroy {H} hero Ongoing and/or Equipment cards."
            if (!DidHitHeroTargetThisTurn())
            {
                LinqCardCriteria heroOngEqp = new LinqCardCriteria((Card c) => c.IsHero && (c.IsOngoing || IsEquipment(c)) && c.IsInPlayAndHasGameText, "hero Ongoing or Equipment");
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, heroOngEqp, H, requiredDecisions: H, allowAutoDecide: H >= base.GameController.FindCardsWhere(heroOngEqp, visibleToCard: GetCardSource()).Count(), responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }
    }
}
