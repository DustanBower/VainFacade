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
    public class DisquietCardController : TheMidnightBazaarUtilityCardController
    {
        public DisquietCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible."
            return card == base.Card;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Damage dealt by Threens is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && IsThreen(dda.DamageSource.Card));

            //Increase sonic and psychic damage dealt by targets by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsTarget && (dd.DamageType == DamageType.Sonic || dd.DamageType == DamageType.Psychic), 1);

            // "When a Threen would deal damage, 1 player may put 2 cards from their hand or 1 hero card from play under [i]The Empty Well[/i] to redirect that damage to a non-Threen target."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && IsThreen(dda.DamageSource.Card) && IsEmptyWellInPlay(), MoveCardsToRedirectResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.RedirectDamage }, TriggerTiming.Before);
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && IsThreen(dda.DamageSource.Card) && !IsEmptyWellInPlay(), EmptyWellNotInPlayResponse, TriggerType.ShowMessage, TriggerTiming.Before);
        }

        private IEnumerator MoveCardsToRedirectResponse(DealDamageAction dda)
        {
            // "... 1 player may put 2 cards from their hand or 1 hero non-character card from play and not under [i]The Empty Well[/i] under [i]The Empty Well[/i] to redirect that damage to a non-Threen target."
            List<bool> cardsMoved = new List<bool>();
            List<DealDamageAction> damageList = new List<DealDamageAction>();
            damageList.Add(dda);
            currentMode = CustomMode.PlayerToDropCards;
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, DecisionMaker, GameController.FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && (tt.ToHero().HasCardsInHand || tt.ToHero().HasDestroyableCards) && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.Custom, isOptional: true, gameAction: dda, dealDamageInfo: damageList, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => ChooseSourceAndMoveCardsToRedirect(tt, cardsMoved, dda));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator ChooseSourceAndMoveCardsToRedirect(TurnTaker tt, List<bool> cardsMoved, DealDamageAction dda)
        {
            // "... 1 player may put 2 cards from their hand under [i]The Empty Well[/i]..."
            IEnumerator coroutine = DropCardsFromHand(tt, 2, false, true, cardsMoved, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            int cardsDropped = 0;
            foreach (bool b in cardsMoved)
            {
                if (b)
                    cardsDropped++;
            }

            if (cardsDropped > 0)
            {
                // "... to redirect that damage to a non-Threen target."
                IEnumerator redirectCoroutine = base.GameController.SelectTargetAndRedirectDamage(base.GameController.FindTurnTakerController(tt).ToHero(), (Card c) => c.IsTarget && !IsThreen(c) && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), dda, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(redirectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(redirectCoroutine);
                }
            }
            else
            {
                IEnumerator messageCoroutine = GameController.SendMessageAction("No cards were moved, so " + tt.Name + " did not redirect damage.", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }
    }
}
