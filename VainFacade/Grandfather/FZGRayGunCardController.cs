using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class FZGRayGunCardController : GrandfatherUtilityCardController
    {
        public FZGRayGunCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, {Grandfather} deals the hero target with the highest HP {H + 1} irreducible energy damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero, (Card c) => H + 1, DamageType.Energy, isIrreducible: true), TriggerType.DealDamage);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, if {Grandfather} is Covert..."
            if (base.CharacterCard.DoKeywordsContain(CovertKeyword))
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.CharacterCard.Title + " is in his " + CovertKeyword + " mode, so he won't bring out the ray gun right away. Instead, he plays 2 cards from the villain deck and destroys it.", Priority.High, GetCardSource(), associatedCards: base.CharacterCard.ToEnumerable());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "... play the top 2 cards of the villain deck..."
                IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, numberOfCards: 2, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                // "... and destroy this card."
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
}
