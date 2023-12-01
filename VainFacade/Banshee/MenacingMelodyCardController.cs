using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class MenacingMelodyCardController:CardController
	{
		public MenacingMelodyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        int num1 = GetPowerNumeral(0, 0);
                        int num2 = GetPowerNumeral(1, 3);

                        //A target deals itself 0 irreducible infernal damage.
                        List<DealDamageAction> damageResults = new List<DealDamageAction>();
                        List<SelectCardDecision> decisionResults = new List<SelectCardDecision>();
                        coroutine = base.GameController.SelectTargetsToDealDamageToSelf(DecisionMaker, num1, DamageType.Infernal, 1, false, 1, true, storedResultsDamage: damageResults, storedResultsDecisions: decisionResults, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        //Then, if that target has 3 or fewer hp, destroy it and {Banshee} regains X hp, where X = that target's hp before it was destroyed.
                        Card target = GetSelectedCard(decisionResults);
                        if (target != null)
                        {
                            int hp = target.HitPoints.Value;

                            if (hp <= num2)
                            {
                                coroutine = base.GameController.DestroyCard(DecisionMaker, target, postDestroyAction: () => base.GameController.GainHP(this.CharacterCard, hp, cardSource: GetCardSource()), cardSource: GetCardSource());
                                if (base.UseUnityCoroutines)
                                {
                                    yield return base.GameController.StartCoroutine(coroutine);
                                }
                                else
                                {
                                    base.GameController.ExhaustCoroutine(coroutine);
                                }
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        //Select a keyword.
                        List<string> keywords = GetKeywords();
                        List<SelectWordDecision> results = new List<SelectWordDecision>();
                        coroutine = base.GameController.SelectWord(DecisionMaker, keywords, SelectionType.SelectKeyword, results, false, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        //Increase the next damage dealt to each target with that keyword by 1.
                        string word = GetSelectedWord(results);
                        if (word != null)
                        {
                            List<Card> targets = FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && base.GameController.DoesCardContainKeyword(c, word)).ToList();
                            foreach (Card c in targets)
                            {
                                IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
                                effect.NumberOfUses = 1;
                                effect.TargetCriteria.IsSpecificCard = c;
                                effect.UntilTargetLeavesPlay(c);
                                IEnumerator increase = AddStatusEffect(effect);
                                if (base.UseUnityCoroutines)
                                {
                                    yield return base.GameController.StartCoroutine(increase);
                                }
                                else
                                {
                                    base.GameController.ExhaustCoroutine(increase);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private List<string> GetKeywords()
        {
            IEnumerable<Card> cards = FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText);
            List<string> keywords = new List<string>();
            foreach (Card c in cards)
            {
                List<string> cardKeywords = base.GameController.GetAllKeywords(c).ToList();
                keywords.AddRange(cardKeywords.Where((string s) => !keywords.Contains(s)));
            }
            return keywords;
        }
    }
}

