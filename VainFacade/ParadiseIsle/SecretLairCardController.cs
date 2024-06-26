﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class SecretLairCardController : AreaCardController
    {
        public SecretLairCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Conspirator cards in the environment deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, isConspirator);
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
            // If in play: show whether this card has seen an environment card enter play this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstEnvCard, "An environment card has already entered play this turn since {0} entered play.", "No environment cards have entered play this turn since {0} entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private readonly string FirstEnvCard = "FirstEnvironmentCardPlayedThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn an environment card enters play, if it is a Conspirator, it deals the hero target with the highest HP {H - 1} projectile damage. Otherwise, play the top card of the environment deck, then 1 player may draw a card."
            AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsEnvironment && !HasBeenSetToTrueThisTurn(FirstEnvCard), ShootOrPlayDrawResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard, TriggerType.DrawCard }, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstEnvCard), TriggerType.Hidden);
        }

        private IEnumerator ShootOrPlayDrawResponse(CardEntersPlayAction cepa)
        {
            SetCardPropertyToTrueIfRealAction(FirstEnvCard);
            // "... if it is a Conspirator, it deals the hero target with the highest HP {H - 1} projectile damage."
            if (cepa.CardEnteringPlay.DoKeywordsContain(ConspiratorKeyword))
            {
                IEnumerator damageCoroutine = DealDamageToHighestHP(cepa.CardEnteringPlay, 1, (Card c) => IsHeroTarget(c), (Card c) => H - 1, DamageType.Projectile);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            else
            {
                // "Otherwise, play the top card of the environment deck, ..."
                IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(cepa);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                // "... then 1 player may draw a card."
                IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
        }
    }
}
