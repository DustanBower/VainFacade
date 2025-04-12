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
    public class RedEyedRonaldCardController : TheMidnightBazaarUtilityCardController
    {
        public RedEyedRonaldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether The Blinded Queen is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(BlindedQueenIdentifier);
            // Show the hero target with the highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1, 1);
            // Show whether The Empty Well is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(EmptyWellIdentifier);
        }

        private ITrigger redirectDamageTrigger;
        private const string didMoveToRedirect = "DidMoveToRedirect";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card would deal damage, if [i]The Blinded Queen[/i] is in play, play the top card of the environment deck instead."
            AddCannotDealDamageTrigger((Card c) => c == this.Card && IsBlindedQueenInPlay());

            //At the end of the environment turn, increase the next damage dealt by this card by 2.
            // "Then this card deals the hero target with the highest HP {H} melee damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, DamageWithRedirectResponse, TriggerType.DealDamage);
            // "If a player puts a card from their hand under [i]The Empty Well[/i], redirect that damage to a target other than this card."
            redirectDamageTrigger = AddTrigger((DealDamageAction dda) => EndOfTurnDamageCriteria(dda), MovedRedirectResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
        }

        private bool EndOfTurnDamageCriteria(DealDamageAction dda)
        {
            return dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.Card && dda.OriginalAmount == H + 2 && dda.OriginalDamageType == DamageType.Melee && dda.CardSource.Card == base.Card && Journal.GetCardPropertiesBoolean(base.Card, didMoveToRedirect) == true;
        }

        private IEnumerator DamageWithRedirectResponse(PhaseChangeAction pca)
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(2);
            effect.SourceCriteria.IsSpecificCard = this.Card;
            effect.NumberOfUses = 1;
            effect.TargetLeavesPlayExpiryCriteria.Card = this.Card;
            IEnumerator coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            // "... this card deals the hero target with the highest HP {H} melee damage. If a player puts a card from their hand under [i]The Empty Well[/i], redirect that damage to a target other than this card."
            List<Card> highest = new List<Card>();
            DealDamageAction earlyPreview = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, H, DamageType.Melee);
            IEnumerable<Card> cardsInHand = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Location.IsHand), visibleToCard: GetCardSource());
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsHeroTarget(c), highest, gameAction: earlyPreview, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highestHeroTarget = highest.FirstOrDefault();
            if (highestHeroTarget != null)
            {
                DealDamageAction targetPreview = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), highestHeroTarget, H + 2, DamageType.Melee);

                if (cardsInHand.Any() && IsEmptyWellInPlay() && !IsBlindedQueenInPlay())
                {
                    TurnTaker chosen = null;
                    List<bool> cardsMoved = new List<bool>();
                    // Choose a player to move cards
                    List<SelectTurnTakerDecision> selection = new List<SelectTurnTakerDecision>();
                    currentMode = CustomMode.PlayerToDropCard;
                    //Log.Debug("RedEyedRonaldCardController.DamageWithRedirectResponse: calling SelectTurnTaker");
                    IEnumerator selectCoroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, selection, optional: true, additionalCriteria: (TurnTaker tt) => IsHero(tt) && tt.ToHero().HasCardsInHand, dealDamageInfo: targetPreview.ToEnumerable(), cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(selectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(selectCoroutine);
                    }
                    if (DidSelectTurnTaker(selection))
                    {
                        chosen = selection.FirstOrDefault().SelectedTurnTaker;
                    }
                    // The chosen player may move a card
                    //Log.Debug("RedEyedRonaldCardController.DamageWithRedirectResponse: calling DropCardsFromHand");
                    IEnumerator moveCoroutine = DropCardsFromHand(chosen, 1, false, true, cardsMoved, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                    int cardsDropped = 0;
                    foreach (bool b in cardsMoved)
                    {
                        if (b)
                        {
                            cardsDropped++;
                        }
                    }
                    // If a card was moved, set the redirect flag
                    if (cardsDropped > 0 && IsRealAction())
                    {
                        Journal.RecordCardProperties(base.Card, didMoveToRedirect, true);
                    }
                }

                // Initiate damage
                IEnumerator damageCoroutine = DealDamage(base.Card, highestHeroTarget, H, DamageType.Melee, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
        }

        private IEnumerator MovedRedirectResponse(DealDamageAction dda)
        {
            // "... redirect that damage to a target other than this card."
            IEnumerator redirectCoroutine = base.GameController.SelectTargetAndRedirectDamage(DecisionMaker, (Card c) => c != base.Card, dda, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            if (IsRealAction())
            {
                SetCardProperty(didMoveToRedirect, false);
            }
        }
    }
}
