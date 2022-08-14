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
    public class HotPursuitCardController : CardController
    {
        public HotPursuitCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If not in play: show whether Burgess has been dealt damage since his last turn, and if so, by whom
            SpecialStringMaker.ShowDamageTaken(base.CharacterCard, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlay, "targets", useCardsSuffix: false), showTotalAmountOfDamageTaken: false, sinceLastTurn: base.TurnTaker, showDamageDealers: true).Condition = () => !base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is not next to a target, destroy it."
            AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
            // "Redirect all damage dealt by that target to {BurgessCharacter}..."
            AddRedirectDamageTrigger((DealDamageAction dda) => GetCardThisCardIsNextTo() != null && dda.DamageSource.IsCard && dda.DamageSource.Card == GetCardThisCardIsNextTo(), () => base.CharacterCard);
            // "... and increase that damage by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => GetCardThisCardIsNextTo() != null && dda.DamageSource.IsCard && dda.DamageSource.Card == GetCardThisCardIsNextTo(), 1);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to a target that has dealt {BurgessCharacter} damage since the end of your last turn."
            IEnumerator selectCoroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => HasTargetDealtDamageToBurgessSinceLastTurn(c), "targets that dealt " + base.CharacterCard.Title + " damage since the end of his last turn"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerable<Card> GetTargetsThatDealtDamageToBurgessSinceHisLastTurn()
        {
            return (from e in base.GameController.Game.Journal.DealDamageEntriesToTargetSinceLastTurn(base.CharacterCard, base.TurnTaker) where e.SourceCard != null && HasTargetDealtDamageToBurgessSinceLastTurn(e.SourceCard) select e.SourceCard).Distinct();
        }

        private bool HasTargetDealtDamageToBurgessSinceLastTurn(Card target)
        {
            if (target.IsTarget)
            {
                DealDamageJournalEntry ddje = (from d in base.GameController.Game.Journal.DealDamageEntriesFromTargetToTargetSinceLastTurn(target, base.CharacterCard, base.TurnTaker) where d.Amount > 0 select d).LastOrDefault();
                if (ddje != null && target.IsInPlayAndHasGameText)
                {
                    int? entryIndex = base.GameController.Game.Journal.GetEntryIndex(ddje);
                    PlayCardJournalEntry pcje = (from c in base.GameController.Game.Journal.PlayCardEntries() where c.CardPlayed == target select c).LastOrDefault();
                    if (pcje == null)
                    {
                        return true;
                    }
                    if (entryIndex > base.GameController.Game.Journal.GetEntryIndex(pcje))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int damageAmt = GetPowerNumeral(0, 3);
            // "{BurgessCharacter} deals the target next to this card 3 projectile damage or destroys this card."
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, base.CharacterCard.Title + " deals " + GetCardThisCardIsNextTo().Title + " " + damageAmt.ToString() + " projectile damage", SelectionType.DealDamage, () => DealDamage(base.CharacterCard, GetCardThisCardIsNextTo(), damageAmt, DamageType.Projectile, cardSource: GetCardSource()), forcedActionMessage: base.Card.Title + " cannot be destroyed, so " + base.CharacterCard.Title + " will deal projectile damage."));
            options.Add(new Function(base.HeroTurnTakerController, "Destroy " + base.Card.Title, SelectionType.DestroySelf, () => DestroyThisCardResponse(null), forcedActionMessage: base.CharacterCard.Title + " cannot deal damage to " + GetCardThisCardIsNextTo().Title + ", so " + base.Card.Title + " will be destroyed."));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, false, noSelectableFunctionMessage: base.CharacterCard.Title + " cannot deal damage to " + GetCardThisCardIsNextTo().Title + ", and " + base.Card.Title + " cannot be destroyed.", associatedCards: GetCardThisCardIsNextTo().ToEnumerable(), cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
