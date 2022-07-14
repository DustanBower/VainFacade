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
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && IsThreen(dda.DamageSource.Card));
            // "When a Threen would deal damage, 1 player may put 2 cards from their hand or 1 hero card from play under [i]The Empty Well[/i] to redirect that damage to a non-Threen target."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && IsThreen(dda.DamageSource.Card) && IsEmptyWellInPlay(), MoveCardsToRedirectResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.RedirectDamage }, TriggerTiming.Before);
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && IsThreen(dda.DamageSource.Card) && !IsEmptyWellInPlay(), EmptyWellNotInPlayResponse, TriggerType.ShowMessage, TriggerTiming.Before);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, play the top card of the environment deck."
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator MoveCardsToRedirectResponse(DealDamageAction dda)
        {
            // "... 1 player may put 2 cards from their hand or 1 hero non-character card from play and not under [i]The Empty Well[/i] under [i]The Empty Well[/i] to redirect that damage to a non-Threen target."
            List<bool> cardsMoved = new List<bool>();
            List<DealDamageAction> damageList = new List<DealDamageAction>();
            damageList.Add(dda);
            currentMode = CustomMode.PlayerToDropCards;
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, DecisionMaker, GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && (tt.ToHero().HasCardsInHand || tt.ToHero().HasDestroyableCards) && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.Custom, isOptional: true, gameAction: dda, dealDamageInfo: damageList, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => ChooseSourceAndMoveCardsToRedirect(tt, cardsMoved, dda));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            yield break;
        }

        private IEnumerator ChooseSourceAndMoveCardsToRedirect(TurnTaker tt, List<bool> cardsMoved, DealDamageAction dda)
        {
            // "... 1 player may put 2 cards from their hand or 1 hero non-character card from play and not under [i]The Empty Well[/i] under [i]The Empty Well[/i]..."
            List<Function> options = new List<Function>();
            options.Add(new Function(GameController.FindTurnTakerController(tt).ToHero(), "Move 2 cards from hand", SelectionType.MoveCard, () => DropCardsFromHand(tt, 2, false, true, cardsMoved, GetCardSource()), tt.ToHero().Hand.Cards.Count() > 1));
            options.Add(new Function(GameController.FindTurnTakerController(tt).ToHero(), "Move 1 card from play", SelectionType.MoveCard, () => DropCardFromPlay(cardsMoved), GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHero && !c.IsCharacter && c.IsInPlay && c.Location != FindCard(EmptyWellIdentifier).UnderLocation), visibleToCard: GetCardSource()).Any()));
            SelectFunctionDecision select = new SelectFunctionDecision(GameController, FindTurnTakerController(tt).ToHero(), options, true, gameAction: dda, "There are no non-character hero cards in play, and " + tt.Name + " does not have two cards in hand to put under [i]The Empty Well.[/i]", cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(select);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
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

        private IEnumerator DropCardFromPlay(List<bool> cardsMoved)
        {
            // The players choose a hero card in play to move under The Empty Well
            if (FindCard(EmptyWellIdentifier).IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(FindCard(EmptyWellIdentifier), GetCardSource()))
            {
                if (cardsMoved == null)
                {
                    cardsMoved = new List<bool>();
                }
                List<SelectCardDecision> choices = new List<SelectCardDecision>();
                currentMode = CustomMode.CardToDrop;
                // Have the players select and move a card
                IEnumerator moveCoroutine = base.GameController.SelectAndMoveCard(DecisionMaker, (Card c) => c.IsHero && !c.IsCharacter && c.IsInPlay && c.Location != FindCard(EmptyWellIdentifier).UnderLocation, FindCard(EmptyWellIdentifier).UnderLocation, optional: true, playIfMovingToPlayArea: false, storedResults: choices, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
                // Note whether a card was moved
                foreach (SelectCardDecision choice in choices)
                {
                    if (choice != null && choice.SelectedCard != null)
                    {
                        if (choice.SelectedCard.Location == FindCard(EmptyWellIdentifier).UnderLocation)
                        {
                            cardsMoved.Add(true);
                        }
                        else
                        {
                            cardsMoved.Add(false);
                        }
                    }
                }
            }
            yield break;
        }
    }
}
