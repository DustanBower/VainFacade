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
    public class VelkinorCardController : TheMidnightBazaarUtilityCardController
    {
        public VelkinorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show which active hero character cards, if any, are in the same play area as this card
            SpecialStringMaker.ShowListOfCardsAtLocation(base.Card.Location.HighestRecursiveLocation, new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.IsActive, "active hero character")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, if there is no active hero in this play area, 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card to their play area."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && !base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.IsActive && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation)).Any(), SelectPlayerResponse, TriggerType.MoveCard);
        }

        private IEnumerator SelectPlayerResponse(PhaseChangeAction pca)
        {
            // "... 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card to their play area."
            List<bool> cardsMoved = new List<bool>();
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, null, GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && tt.ToHero().HasCardsInHand && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.MoveCard, isOptional: true, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => GetSwordResponse(tt));
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
                IEnumerator pickupCoroutine = base.GameController.MoveCard(base.GameController.FindTurnTakerController(tt), base.Card, tt.PlayArea, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: tt, evenIfIndestructible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(pickupCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(pickupCoroutine);
                }
            }
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int damageAmt = GetPowerNumeral(1, 5);
            int damageIncrease = GetPowerNumeral(2, 1);
            if (base.Card.Location.Cards.Any((Card c) => c.IsHeroCharacterCard && c.IsActive))
            {
                List<SelectCardDecision> selection = new List<SelectCardDecision>();
                HeroTurnTakerController httc = DecisionMaker;
                if (base.Card.Location.OwnerTurnTaker.IsHero)
                    httc = base.GameController.FindTurnTakerController(base.Card.Location.OwnerTurnTaker).ToHero();
                IEnumerator findCoroutine = base.GameController.SelectCardAndStoreResults(httc, SelectionType.HeroToUsePower, new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.IsActive && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation), selection, false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                SelectCardDecision choice = selection.FirstOrDefault();
                if (choice != null && choice.SelectedCard != null)
                {
                    Card heroUsingPower = choice.SelectedCard;
                    // "Your hero deals 1 target 5 irreducible melee damage, ..."
                    IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(httc, new DamageSource(base.GameController, heroUsingPower), (Card c) => damageAmt, DamageType.Melee, () => numTargets, false, numTargets, isIrreducible: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                    // "... then increases damage dealt to your hero by 1 until the start of your next turn."
                    IncreaseDamageStatusEffect aggro = new IncreaseDamageStatusEffect(damageIncrease);
                    aggro.TargetCriteria.IsSpecificCard = heroUsingPower;
                    aggro.UntilStartOfNextTurn(heroUsingPower.Owner);
                    aggro.UntilTargetLeavesPlay(heroUsingPower);
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
            yield break;
        }
    }
}
