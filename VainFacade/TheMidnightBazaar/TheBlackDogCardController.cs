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
    public class TheBlackDogCardController : TheMidnightBazaarUtilityCardController
    {
        public TheBlackDogCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the environment turn, this card deals each non-environment or Threen target 0 irreducible infernal damage 3 times."
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, TripleBlastResponse, TriggerType.DealDamage);
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

        public override bool AskIfActionCanBePerformed(GameAction gameAction)
        {
            // "Damage dealt by this card can only be increased by environment cards."
            if (gameAction is IncreaseDamageAction)
            {
                IncreaseDamageAction buff = (IncreaseDamageAction)gameAction;
                //bool? flag = gameAction.DoesFirstCardAffectSecondCard((Card c) => !c.IsEnvironment, (Card c) => c == base.Card);
                if (buff.DealDamageAction.DamageSource != null && buff.DealDamageAction.DamageSource.IsCard && buff.DealDamageAction.DamageSource.Card == base.Card && buff.CardSource != null && buff.CardSource.Card != null && !buff.CardSource.Card.IsEnvironment)
                {
                    return false;
                }
            }
            return base.AskIfActionCanBePerformed(gameAction);
        }

        private IEnumerator TripleBlastResponse(PhaseChangeAction pca)
        {
            // "... this card deals each non-environment or Threen target 0 irreducible infernal damage 3 times."
            List<DealDamageAction> attacks = new List<DealDamageAction>();
            for (int i = 0; i < 3; i++)
            {
                attacks.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, 0, DamageType.Infernal, isIrreducible: true));
            }
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamage(attacks, (Card c) => c.IsTarget && (!c.IsEnvironmentTarget || IsThreen(c)));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
