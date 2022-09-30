using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class DestroyersDirgeCardController : TheFuryUtilityCardController
    {
        public DestroyersDirgeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, IsCoincidence);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by {TheFuryCharacter} by 1 for each Coincidence in your trash."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard), (DealDamageAction dda) => FindCardsWhere(new LinqCardCriteria((Card c) => c.DoKeywordsContain(CoincidenceKeyword) && c.Location.HighestRecursiveLocation == base.TurnTaker.Trash), visibleToCard: GetCardSource()).Count());
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int amtPsychic = GetPowerNumeral(0, 1);
            int numTargets = GetPowerNumeral(1, 1);
            int xBase = GetPowerNumeral(2, 2);
            // "{TheFuryCharacter} is indestructible this turn."
            IEnumerator protectCoroutine = MakeIndestructibleThisTurn(base.CharacterCard, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(protectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(protectCoroutine);
            }
            // "She deals herself 1 psychic damage, ..."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator psychicCoroutine = DealDamage(base.Card, base.Card, amtPsychic, DamageType.Psychic, storedResults: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(psychicCoroutine);
            }
            int amtTaken = 0;
            if (DidDealDamage(damageResults))
            {
                foreach (DealDamageAction dda in damageResults)
                {
                    if (DidDealDamage(dda.ToEnumerable().ToList(), toSpecificTarget: base.Card, fromDamageSource: base.Card))
                    {
                        amtTaken += dda.Amount;
                    }
                }
            }
            int x = xBase + amtTaken;
            if (x > 0)
            {
                // "... then deals 1 target X irreducible melee damage that cannot be redirected..."
                ITrigger tempTrigger = AddMakeDamageNotRedirectableTrigger((DealDamageAction damage) => damage != null && damage.CardSource != null && damage.CardSource.Card == base.Card);
                IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), x, DamageType.Melee, numTargets, false, numTargets, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(meleeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(meleeCoroutine);
                }
                RemoveTrigger(tempTrigger);
                // "... and increases the next damage dealt to {TheFuryCharacter} by X, where X = 2 plus the damage she takes this way."
                IEnumerator increaseCoroutine = IncreaseNextDamageTo(base.CharacterCard, x, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(increaseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(increaseCoroutine);
                }
            }
        }
    }
}
