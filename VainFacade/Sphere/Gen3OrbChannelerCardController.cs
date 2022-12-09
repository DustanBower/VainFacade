using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class Gen3OrbChannelerCardController : SphereUtilityCardController
    {
        public Gen3OrbChannelerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Emanation cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(SphereUtilityCardController.isEmanation);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Return an Emanation from in play to your hand."
            int numTargets = GetPowerNumeral(0, 1);
            int energyAmt = GetPowerNumeral(1, 3);
            List<MoveCardAction> storedResults = new List<MoveCardAction>();
            List<MoveCardDestination> destinations = new List<MoveCardDestination>();
            destinations.Add(new MoveCardDestination(base.HeroTurnTaker.Hand));
            IEnumerator moveCoroutine = base.GameController.SelectAndReturnCards(base.HeroTurnTakerController, 1, isEmanation, true, false, false, 1, storedResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            if (DidMoveCard(storedResults))
            {
                // "If you do, {Sphere} deals 1 target 3 energy damage."
                IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), energyAmt, DamageType.Energy, numTargets, false, numTargets, cardSource: GetCardSource());
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
}
