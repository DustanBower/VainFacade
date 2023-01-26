using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class BrimstoneDownCardController : CardController
    {
        public BrimstoneDownCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show which decks have had cards revealed from them by Ember's cards and/or powers this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => NumberOfDecksRevealedFromThisTurn() > 0, () => base.TurnTaker.Name + "'s cards and powers have revealed cards from " + NamesOfDecksRevealedFromThisTurn().ToCommaList(useWordAnd: true) + " this turn.", () => base.TurnTaker.Name + "'s cards and powers have not revealed cards from any decks this turn.");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When you use a power, {EmberCharacter} may deal 1 target 1 fire damage."
            AddTrigger((UsePowerAction upa) => upa.HeroUsingPower == base.TurnTakerController.ToHero(), BurnOneResponse, TriggerType.DealDamage, TriggerTiming.After);
            // "At the end of each turn, {EmberCharacter} deals up to X targets 1 fire damage each, where X is the number of decks that have had cards revealed by your cards or powers this turn."
            AddEndOfTurnTrigger((TurnTaker tt) => base.GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource()), BurnXResponse, TriggerType.DealDamage);
        }

        private IEnumerator BurnOneResponse(UsePowerAction upa)
        {
            // "... {EmberCharacter} may deal 1 target 1 fire damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 1, DamageType.Fire, 1, false, 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }

        private IEnumerator BurnXResponse(PhaseChangeAction pca)
        {
            // "... {EmberCharacter} deals up to X targets 1 fire damage each, where X is the number of decks that have had cards revealed by your cards or powers this turn."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 1, DamageType.Fire, NumberOfDecksRevealedFromThisTurn(), false, 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }

        private List<Location> DecksRevealedFromThisTurn()
        {
            return (from e in base.GameController.Game.Journal.MoveCardEntriesThisTurn() where e.ToLocation.IsRevealed && e.CardSource.Owner == base.TurnTaker && e.FromLocation.IsDeck && base.GameController.IsLocationVisibleToSource(e.FromLocation, GetCardSource()) select e.FromLocation).Distinct().ToList();
        }

        private List<string> NamesOfDecksRevealedFromThisTurn()
        {
            return (from d in DecksRevealedFromThisTurn() select d.GetFriendlyName()).ToList();
        }

        private int NumberOfDecksRevealedFromThisTurn()
        {
            return DecksRevealedFromThisTurn().Count();
        }
    }
}
