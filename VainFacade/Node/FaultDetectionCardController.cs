using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class FaultDetectionCardController : NodeUtilityCardController
    {
        public FaultDetectionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Connected targets
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsConnected(c) && c.IsTarget, "Connected targets", false, false, "target", "targets"));
        }

        public override IEnumerator Play()
        {
            // "{NodeCharacter} deals up to 3 [i]Connected[/i] targets 1 psychic damage and 1 psychic damage each."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction onePsychic = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Psychic);
            instances.Add(onePsychic);
            instances.Add(onePsychic);
            IEnumerator damageCoroutine = SelectTargetsAndDealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget && IsConnected(c), minNumberOfTargets: 0, maxNumberOfTargets: 3);
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
}
