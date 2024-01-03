using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class CoverFireCardController:ManeuverCardController
	{
		public CoverFireCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.CanCauseDamageOutOfPlay);
		}

        public string FirstDamageKey = "FirstDamageKey";
        public Dictionary<string, ITrigger> tempTriggers = new Dictionary<string, ITrigger>();

        public override void AddTriggers()
        {
            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Until the start of your next turn, the first time each turn any hero target would be dealt damage by another target, reduce that damage by 1, then {Peacekeeper} deals the source of that damage 2 projectile damage.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 2);
            IEnumerable<CoverFireStatusEffect> effects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is CoverFireStatusEffect).Select((StatusEffectController sec) => (CoverFireStatusEffect)(sec.StatusEffect));
            int ID = 0;
            if (effects.Count() > 0)
            {
                ID = effects.Select((CoverFireStatusEffect s) => s.ID).Max() + 1;
            }
            CoverFireStatusEffect effect = new CoverFireStatusEffect(this.CardWithoutReplacements, "PowerResponse", $"The first time a hero target would be dealt damage, reduce that damage by {num1}, then {this.TurnTaker.Name} deals the source of that damage {num2} projectile damage.", new TriggerType[] { TriggerType.ReduceDamage }, this.TurnTaker, this.Card, new int[] {num1, num2 }, ID);
            effect.UntilStartOfNextTurn(this.TurnTaker);
            effect.UntilTargetLeavesPlay(this.CharacterCard);
            effect.TargetCriteria.IsHero = true;
            effect.BeforeOrAfter = BeforeOrAfter.Before;
            IEnumerator coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public IEnumerator PowerResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            if (dda.DamageSource.IsTarget && dda.DamageSource.Card != dda.Target)
            {
                string key = GenerateKey(FirstDamageKey, (CoverFireStatusEffect)effect, hero);
                int num1 = powerNumerals[0];
                int num2 = powerNumerals[1];
                //Console.WriteLine("Entered Cover Fire response. Key = " + key);
                if (!IsPropertyTrue(key))
                {
                    SetCardPropertyToTrueIfRealAction(key);
                    IEnumerator coroutine = base.GameController.ReduceDamage(dda, num1, null, new CardSource(FindCardController(effect.CardSource)));
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (dda.DamageSource.IsTarget && dda.DamageSource.Card.IsInPlayAndHasGameText && !dda.IsPretend)
                    {
                        ITrigger tempTrigger = AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.InstanceIdentifier == dda.InstanceIdentifier, (DealDamageAction dd) => DamageResponse(dd, effect, hero, dd.DamageSource.Card, num2), TriggerType.DealDamage, TriggerTiming.After);
                        int ID = ((CoverFireStatusEffect)effect).ID;
                        string tempTriggerKey = GenerateTempTriggerKey(dda, ID);
                        //Console.WriteLine("Creating temporary trigger for Cover Fire. tempTriggerKey = " + tempTriggerKey);
                        tempTriggers.Add(tempTriggerKey, tempTrigger);
                    }
                }
            }
        }

        public string GenerateKey(string key, CoverFireStatusEffect effect, TurnTaker hero)
        {
            return key + effect.ID + hero.Name;
        }

        public string GenerateTempTriggerKey(DealDamageAction dd, int ID)
        {
            return dd.InstanceIdentifier.ToString() + ID.ToString();
        }

        public IEnumerator DamageResponse(DealDamageAction dda, StatusEffect effect, TurnTaker hero, Card target, int amount)
        {
            //Console.WriteLine("Entered DamageResponse. tempTriggers = " + tempTriggers.ToCommaList());
            int ID = ((CoverFireStatusEffect)effect).ID;
            string tempTriggerKey = GenerateTempTriggerKey(dda, ID);
            RemoveTrigger(tempTriggers[tempTriggerKey]);
            tempTriggers.Remove(tempTriggerKey);

            if (target.IsInPlayAndHasGameText && target.IsTarget)
            {
                AddInhibitorException((GameAction ga) => true);
                List<Card> results2 = new List<Card>();
                DealDamageAction dd = new DealDamageAction(base.GameController, null, null, amount, DamageType.Projectile);
                HeroTurnTakerController httc = FindHeroTurnTakerController(hero.ToHero());
                IEnumerator coroutine = base.GameController.FindCharacterCard(httc, hero, SelectionType.HeroToDealDamage, results2, damageInfo: new DealDamageAction[] { dd }, cardSource: new CardSource(FindCardController(effect.CardSource)));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                Card selected = results2.FirstOrDefault();
                if (selected != null)
                {
                    coroutine = base.GameController.DealDamage(httc, selected, (Card c) => c == target, amount, DamageType.Projectile, cardSource: new CardSource(FindCardController(effect.CardSource)));
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }

                RemoveInhibitorException();
            }
        }
    }
}

