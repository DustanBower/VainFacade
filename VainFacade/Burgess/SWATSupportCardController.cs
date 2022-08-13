using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class SWATSupportCardController : BurgessUtilityCardController
    {
        public SWATSupportCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether this card has shot a target this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(TakenShotThisTurn, base.Card.Title + " has already fired their weapon to support " + base.TurnTaker.Name + " or another Backup this turn.", base.Card.Title + " has not yet fired their weapon to support " + base.TurnTaker.Name + " or another Backup this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string TakenShotThisTurn = "TakenShotThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to this card by 1."
            AddReduceDamageTrigger((Card c) => c == base.Card, 1);
            // "Once per turn when {BurgessCharacter} or a Backup deals a target damage, this card may deal that target 3 projectile damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(TakenShotThisTurn) && dda.DidDealDamage && dda.Target.IsInPlayAndHasGameText && dda.DamageSource.IsCard && (dda.DamageSource.IsSameCard(base.CharacterCard) || dda.DamageSource.Card.DoKeywordsContain(BackupKeyword)), AskToTakeShotResponse, TriggerType.DealDamage, TriggerTiming.After);
        }

        private IEnumerator AskToTakeShotResponse(DealDamageAction dda)
        {
            // "... this card may deal that target 3 projectile damage."
            Card target = dda.Target;
            DealDamageAction shot = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), target, 3, DamageType.Projectile);
            YesNoDecision choice = new YesNoDecision(base.GameController, base.HeroTurnTakerController, SelectionType.DealDamage, gameAction: shot, associatedCards: target.ToEnumerable(), cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choice))
            {
                SetCardPropertyToTrueIfRealAction(TakenShotThisTurn);
                IEnumerator damageCoroutine = DealDamage(base.Card, target, 3, DamageType.Projectile, cardSource: GetCardSource());
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

        public override IEnumerator Play()
        {
            // "When this card enters play, discard up to 5 cards. Set this card's HP to twice the number of cards discarded."
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 5, optional: false, requiredDecisions: 0, storedResults: results, allowAutoDecide: true, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            int startingHP = 2 * GetNumberOfCardsDiscarded(results);
            IEnumerator hpCoroutine = base.GameController.SetHP(base.Card, startingHP, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(hpCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(hpCoroutine);
            }
            // Do we need to specify "destroy this card if it has 0 HP"? We'll see...
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int buffAmt = GetPowerNumeral(0, 1);
            // "Discard a card."
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 1, requiredDecisions: 1, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Increase damage dealt by {BurgessCharacter} and Backups by 1 until the start of your next turn."
            IncreaseDamageStatusEffect buffBurgess = new IncreaseDamageStatusEffect(buffAmt);
            buffBurgess.SourceCriteria.IsSpecificCard = base.CharacterCard;
            buffBurgess.CardDestroyedExpiryCriteria.Card = base.CharacterCard;
            buffBurgess.UntilStartOfNextTurn(base.TurnTaker);
            IncreaseDamageStatusEffect buffBackups = new IncreaseDamageStatusEffect(buffAmt);
            buffBackups.SourceCriteria.HasAnyOfTheseKeywords = new string[1] { BackupKeyword }.ToList();
            buffBackups.UntilStartOfNextTurn(base.TurnTaker);
            IEnumerator burgessCoroutine = AddStatusEffect(buffBurgess);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(burgessCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(burgessCoroutine);
            }
            IEnumerator backupsCoroutine = AddStatusEffect(buffBackups);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(backupsCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(backupsCoroutine);
            }
        }
    }
}
