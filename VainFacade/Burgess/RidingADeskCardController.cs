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
    public class RidingADeskCardController : CardController
    {
        public RidingADeskCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If not in play, show whether Hot Pursuit is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(PursuitIdentifier).Condition = () => !base.Card.IsInPlayAndHasGameText;
            // If not in play, show whether To Serve and Protect is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(ProtectIdentifier).Condition = () => !base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string PursuitIdentifier = "HotPursuit";
        protected readonly string ProtectIdentifier = "ToServeAndProtect";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "{BurgessCharacter} is immune to damage."
            AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard);
            // "You may not play cards or use powers."
            CannotPlayCards((TurnTakerController ttc) => ttc == base.TurnTakerController);
            CannotUsePowers((TurnTakerController ttc) => ttc == base.TurnTakerController);
            // "When a card would cause you to draw a card, prevent that draw."
            AddTrigger((DrawCardAction dca) => dca.HeroTurnTaker == base.HeroTurnTaker && dca.CardSource != null && dca.CardSource.Card != null, PreventDrawResponse, TriggerType.CancelAction, TriggerTiming.Before);
            // "At the start of your turn, {BurgessCharacter} regains 2 HP, then destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealDestructResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DestroySelf });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy [i]Hot Pursuit[/i] and [i]To Serve and Protect[/i]."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.Owner == base.TurnTaker && (c.Identifier == PursuitIdentifier || c.Identifier == ProtectIdentifier), "of Hot Pursuit or To Serve and Protect", false, true, "copy", "copies"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Then {BurgessCharacter} regains 1 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }

        private IEnumerator PreventDrawResponse(DrawCardAction dca)
        {
            // "... prevent that draw."
            IEnumerator cancelCoroutine = base.GameController.CancelAction(dca, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
        }

        private IEnumerator HealDestructResponse(PhaseChangeAction pca)
        {
            // "... {BurgessCharacter} regains 2 HP, ..."
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "... then destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
