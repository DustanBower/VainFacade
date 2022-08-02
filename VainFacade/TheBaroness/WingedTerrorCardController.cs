using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class WingedTerrorCardController : BaronessUtilityCardController
    {
        public WingedTerrorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show if Cloud of Bats is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(BatsIdentifier);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {TheBaroness} by 1."
            AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);
            // "{TheBaroness} is immune to melee damage."
            AddImmuneToDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Melee && dda.Target == base.CharacterCard);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy Cloud of Bats."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            Card cloud = base.TurnTaker.FindCard(BatsIdentifier);
            if (cloud.IsInPlayAndHasGameText)
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, cloud, storedResults: results, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            // "If you do, {TheBaroness} regains {H} HP."
            if (DidDestroyCard(results))
            {
                IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, H, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
            }
        }
    }
}
