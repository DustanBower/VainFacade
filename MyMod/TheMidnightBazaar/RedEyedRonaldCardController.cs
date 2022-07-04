using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacade.TheMidnightBazaar
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
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card == base.Card && IsBlindedQueenInPlay(), PlayEnvironmentCardInsteadResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.PlayCard }, TriggerTiming.Before);
            // "At the end of the environment turn, this card deals the hero target with the highest HP {H + 2} melee damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, DamageWithRedirectResponse, TriggerType.DealDamage);
            // "If a player puts a card from their hand under [i]The Empty Well[/i], redirect that damage to a target other than this card."
            redirectDamageTrigger = AddTrigger((DealDamageAction dda) => EndOfTurnDamageCriteria(dda), MovedRedirectResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            // ...
        }

        private bool EndOfTurnDamageCriteria(DealDamageAction dda)
        {
            return dda.DamageSource != null && dda.DamageSource.Card == base.Card && dda.OriginalAmount == H + 2 && dda.OriginalDamageType == DamageType.Melee && dda.CardSource.Card == base.Card && Journal.GetCardPropertiesBoolean(base.Card, didMoveToRedirect) == true;
        }

        private IEnumerator PlayEnvironmentCardInsteadResponse(DealDamageAction dda)
        {
            // "... play the top card of the environment deck instead."
            IEnumerator cancelCoroutine = CancelAction(dda);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(dda);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }

        private IEnumerator DamageWithRedirectResponse(PhaseChangeAction pca)
        {
            // "... this card deals the hero target with the highest HP {H + 2} melee damage. If a player puts a card from their hand under [i]The Empty Well[/i], redirect that damage to a target other than this card."
            List<Card> highest = new List<Card>();
            DealDamageAction earlyPreview = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, H + 2, DamageType.Melee);
            IEnumerable<Card> cardsInHand = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Location.IsHand), visibleToCard: GetCardSource());
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHero, highest, gameAction: earlyPreview, cardSource: GetCardSource());
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
                if (cardsInHand.Any() && IsEmptyWellInPlay())
                {
                    List<YesNoCardDecision> chooseToMove = new List<YesNoCardDecision>();
                    HeroTurnTakerController httc = DecisionMaker;
                    Card firstInHand = cardsInHand.FirstOrDefault();
                    if (firstInHand != null && !cardsInHand.Any((Card c) => c.Owner != firstInHand.Owner))
                    {
                        httc = base.GameController.FindHeroTurnTakerController(firstInHand.Owner.ToHero());
                    }
                    IEnumerator decideCoroutine = base.GameController.MakeYesNoCardDecision(httc, SelectionType.Custom, base.Card, action: targetPreview, storedResults: chooseToMove, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(decideCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(decideCoroutine);
                    }
                    if (DidPlayerAnswerYes(chooseToMove))
                    {
                        List<bool> cardsMoved = new List<bool>();
                        TurnTaker chosen = null;
                        if (httc == DecisionMaker)
                        {
                            // Choose a player to move cards
                            List<SelectTurnTakerDecision> selection = new List<SelectTurnTakerDecision>();
                            IEnumerator selectCoroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, selection, additionalCriteria: (TurnTaker tt) => tt.IsHero && tt.ToHero().Hand.HasCards, cardSource: GetCardSource());
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
                        }
                        else
                        {
                            // Only one player has cards to move
                            chosen = httc.TurnTaker;
                        }
                        if (chosen != null)
                        {
                            // The chosen player may move a card
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
                    }

                    // Initiate damage
                    IEnumerator damageCoroutine = DealDamage(base.Card, highestHeroTarget, H + 2, DamageType.Melee, cardSource: GetCardSource());
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
            yield break;
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
            yield break;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Do you want to move a card from hand under [i]The Empty Well[/i] to redirect the damage?", "Should they move a card from hand under [i]The Empty Well[/i] to redirect the damage?", "Vote for whether to move a card from hand under [i]The Empty Well[/i] to redirect the damage", "moving a card from hand under [i]The Empty Well[/i] and redirecting damage", false);
        }
    }
}
