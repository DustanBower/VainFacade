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
    public class VelkinorCardController : TheMidnightBazaarUtilityCardController
    {
        public VelkinorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show which active hero character cards, if any, are in the same play area as this card
            SpecialStringMaker.ShowListOfCardsAtLocation(base.Card.Location.HighestRecursiveLocation, new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsActive, "active hero character")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();

            AddAsPowerContributor();

            //Increase damage dealt to targets next to this card by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.Target.GetAllNextToCards(false).Contains(this.Card), 1);

            // "At the end of the environment turn, if there is no active hero in this play area, 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card to their play area."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && IsEmptyWellInPlay(), SelectPlayerResponse, TriggerType.MoveCard);
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && !IsEmptyWellInPlay(), EmptyWellNotInPlayResponse, TriggerType.ShowMessage);
        }

        private IEnumerator SelectPlayerResponse(PhaseChangeAction pca)
        {
            // "... 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card next to their hero."
            List<bool> cardsMoved = new List<bool>();
            currentMode = CustomMode.PlayerToDropCard;
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, null, GameController.FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && tt.ToHero().HasCardsInHand && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.Custom, isOptional: true, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => GetSwordResponse(tt));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator GetSwordResponse(TurnTaker tt)
        {
            // "... 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card to their play area."
            List<bool> cardsMoved = new List<bool>();
            IEnumerator moveCoroutine = DropCardsFromHand(tt, 1, false, true, cardsMoved, GetCardSource());
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
                    cardsDropped++;
            }
            if (cardsDropped > 0)
            {
                List<Card> selectedHero = new List<Card>();
                IEnumerator coroutine = FindCharacterCard(tt, SelectionType.MoveCardNextToCard, selectedHero);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                Card selected = selectedHero.FirstOrDefault();

                if (selected != null)
                {
                    IEnumerator pickupCoroutine = base.GameController.MoveCard(base.GameController.FindTurnTakerController(tt), base.Card, selected.NextToLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: tt, evenIfIndestructible: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(pickupCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(pickupCoroutine);
                    }
                }
            }
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            if (cardController.Card == GetCardThisCardIsNextTo())
            {
                return new Power[1]
                {
                new Power(cardController.HeroTurnTakerController, cardController, "Your hero deals 1 target 5 irreducible melee damage.", DealDamageResponse(), 0, null, GetCardSource())
                };
            }
            return null;
        }

        private IEnumerator DealDamageResponse()
        {
            //Your hero deals 1 target 5 irreducible melee damage.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 5);

            Card wielder = GetCardThisCardIsNextTo();
            HeroTurnTakerController httc = FindHeroTurnTakerController(wielder.Owner.ToHero());
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(httc, new DamageSource(base.GameController, wielder), num2, DamageType.Melee, num1, false, num1, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string s = $"put a card from their hand under The Empty Well";

            return new CustomDecisionText(
            $"Select a player to {s}.",
            $"The players are selecting a player to {s}.",
            $"Vote for a player to {s}.",
            $"a player to {s}."
            );
        }
    }
}
