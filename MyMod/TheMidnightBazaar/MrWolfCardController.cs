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
    public class MrWolfCardController : TheMidnightBazaarUtilityCardController
    {
        public MrWolfCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether The Blinded Queen is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(BlindedQueenIdentifier);
            // Show target with lowest HP other than this card
            SpecialStringMaker.ShowLowestHP(1, () => 1, cardCriteria: new LinqCardCriteria((Card c) => c != base.Card, "other than " + base.Card.Title, false, true, "target", "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card would deal damage, if [i]The Blinded Queen[/i] is in play, play the top card of the villain deck instead."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card == base.Card && dda.Amount > 0 && IsBlindedQueenInPlay(), PlayVillainCardInsteadResponse, new TriggerType[]{ TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.PlayCard}, TriggerTiming.Before);
            // "At the end of the environment turn, this card deals the target with the lowest HP other than this card {H + 1} melee damage."
            AddDealDamageAtEndOfTurnTrigger(TurnTaker, base.Card, (Card c) => c.IsTarget && c != base.Card, TargetType.LowestHP, H + 1, DamageType.Melee);
        }

        private IEnumerator PlayVillainCardInsteadResponse(DealDamageAction dda)
        {
            // "... play the top card of the villain deck instead."
            bool wouldDealDamage = dda.CanDealDamage && !dda.IsPretend;
            //Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: calling CancelAction");
            List<CancelAction> cancelActions = new List<CancelAction>();
            IEnumerator cancelCoroutine = CancelAction(dda, storedResults: cancelActions, isPreventEffect: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            /*Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: calling PlayTheTopCardOfTheVillainDeckWithMessageResponse");
            IEnumerator playCoroutine = base.PlayTheTopCardOfTheVillainDeckWithMessageResponse(cancelActions.FirstOrDefault());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }*/
            if (wouldDealDamage)
            {
                //Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: calling SendMessageAction");
                IEnumerator announceCoroutine = base.GameController.SendMessageAction(base.Card.Title + " plays the top card of the villain deck...", Priority.High, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(announceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(announceCoroutine);
                }
                List<SelectLocationDecision> choices = new List<SelectLocationDecision>();
                //Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: calling SelectADeck");
                IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.PlayTopCardOfVillainDeck, (Location l) => l.IsVillain, storedResults: choices, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                SelectLocationDecision playerChoice = choices.FirstOrDefault();
                if (playerChoice != null && playerChoice.SelectedLocation.Location != null)
                {
                    //Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: deck was selected");
                    Location chosenDeck = playerChoice.SelectedLocation.Location;
                    //Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: chosenDeck: " + chosenDeck.GetFriendlyName());
                    TurnTakerController ttc = FindTurnTakerController(chosenDeck.OwnerTurnTaker);
                    //Log.Debug("MrWolfCardController.PlayVillainCardInsteadResponse: calling PlayTopCardOfLocation");
                    IEnumerator playCoroutine = base.GameController.PlayTopCardOfLocation(ttc, chosenDeck, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
