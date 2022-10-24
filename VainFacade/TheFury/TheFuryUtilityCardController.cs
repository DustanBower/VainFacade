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
    public class TheFuryUtilityCardController : CardController
    {
        public TheFuryUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        protected const string CoincidenceKeyword = "coincidence";

        public LinqCardCriteria IsCoincidence = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CoincidenceKeyword), "Coincidence");

        public IEnumerator IncreaseNextDamageTo(Card target, int amount, CardSource cardSource)
        {
            IncreaseDamageStatusEffect debuff = new IncreaseDamageStatusEffect(amount);
            debuff.TargetCriteria.IsSpecificCard = target;
            debuff.NumberOfUses = 1;
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(debuff, true, cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }

        public IEnumerator SelectTargetAndIncreaseNextDamageTo(LinqCardCriteria criteria, int amount, bool optional, CardSource cardSource, List<SelectCardDecision> choices = null)
        {
            if (choices == null)
            {
                choices = new List<SelectCardDecision>();
            }
            IEnumerator chooseCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.SelectTargetNoDamage, criteria, choices, optional, cardSource: cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (choices.Count > 0)
            {
                Card selectedCard = choices.First().SelectedCard;
                if (selectedCard != null)
                {
                    IEnumerator increaseCoroutine = IncreaseNextDamageTo(selectedCard, amount, cardSource);
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

        public IEnumerator ReduceNextDamageTo(Card target, int amount, CardSource cardSource)
        {
            ReduceDamageStatusEffect buff = new ReduceDamageStatusEffect(amount);
            buff.TargetCriteria.IsSpecificCard = target;
            buff.NumberOfUses = 1;
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(buff, true, cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }

        public IEnumerator IncreaseNextDamageBy(Card target, int amount, CardSource cardSource)
        {
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(amount);
            buff.SourceCriteria.IsSpecificCard = target;
            buff.NumberOfUses = 1;
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(buff, true, cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }

        public IEnumerator MakeIndestructibleThisTurn(Card selected, CardSource cardSource)
        {
            MakeIndestructibleStatusEffect protection = new MakeIndestructibleStatusEffect();
            protection.CardsToMakeIndestructible.IsSpecificCard = selected;
            protection.UntilThisTurnIsOver(base.Game);
            IEnumerator protectCoroutine = base.GameController.AddStatusEffect(protection, true, cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(protectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(protectCoroutine);
            }
        }
    }
}
