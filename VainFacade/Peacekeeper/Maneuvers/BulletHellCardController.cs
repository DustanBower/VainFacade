using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class BulletHellCardController:ManeuverCardController
	{
		public BulletHellCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        private string FirstDrawKey = "FirstDrawKey";
        public string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //Prevent the first card draw during your draw phase.
            AddTrigger<DrawCardAction>((DrawCardAction dc) => !IsPropertyTrue(FirstDrawKey) && base.Game.ActiveTurnPhase.IsDrawCard && base.Game.ActiveTurnPhase.TurnTaker == this.TurnTaker, PreventDrawResponse, TriggerType.CancelAction, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDrawKey), TriggerType.Hidden);
        }

        private IEnumerator PreventDrawResponse(DrawCardAction dc)
        {
            SetCardProperty(FirstDrawKey, true);

            IEnumerator coroutine = CancelAction(dc, isPreventEffect: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            switch (index)
            {
                case 0:
                    //Until the start of your next turn, the first time each turn a target deals damage, {Peacekeeper} may discard a card to deal 1 target 1 projectile damage.
                    int num1 = GetPowerNumeral(0, 1);
                    int num2 = GetPowerNumeral(1, 1);
                    IEnumerable<BulletHellStatusEffect> effects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is BulletHellStatusEffect).Select((StatusEffectController sec) => (BulletHellStatusEffect)(sec.StatusEffect));
                    int ID = 0;
                    if (effects.Count() > 0)
                    {
                        ID = effects.Select((BulletHellStatusEffect s) => s.ID).Max() + 1;
                    }
                    BulletHellStatusEffect effect = new BulletHellStatusEffect(this.CardWithoutReplacements, "PowerResponse", $"The first time each turn any target deals damage, {this.TurnTaker.Name} may discard a card to deal {num1} target {num2} projectile damage.", new TriggerType[] { TriggerType.DiscardCard, TriggerType.DealDamage }, this.TurnTaker, this.Card, new int[] { num1, num2 },ID);
                    effect.UntilStartOfNextTurn(this.TurnTaker);
                    effect.BeforeOrAfter = BeforeOrAfter.After;
                    effect.UntilTargetLeavesPlay(this.CharacterCard);
                    effect.DoesDealDamage = true;
                    effect.DamageAmountCriteria.GreaterThan = 0;
                    IEnumerator coroutine = AddStatusEffect(effect);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
                case 1:
                    //Draw a card or destroy this card.
                    IEnumerable<Function> functionChoices = new Function[2]
                    {
                    new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => DrawCard()),
                    new Function(base.HeroTurnTakerController, "Destroy this card", SelectionType.DestroyCard, () => DestroyThisCardResponse(null))
                    };

                    SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
                    IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(choose);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(choose);
                    }
                    break;
            }
            
        }

        public IEnumerator PowerResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            string key = GenerateKey(FirstDamageKey, (BulletHellStatusEffect)effect, hero);
            int num1 = powerNumerals[0];
            int num2 = powerNumerals[1];
            //Console.WriteLine("Entered Bullet Hell response. Key = " + key);
            if (!IsPropertyTrue(key))
            {
                SetCardProperty(key, true);
                HeroTurnTakerController httc = FindHeroTurnTakerController(hero.ToHero());
                List<DiscardCardAction> results = new List<DiscardCardAction>();
                IEnumerator coroutine = SelectAndDiscardCards(httc, 1, false, 0, results, responsibleTurnTaker: hero);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidDiscardCards(results))
                {
                    List<Card> results2 = new List<Card>();
                    DealDamageAction dd = new DealDamageAction(base.GameController, null, null, num1, DamageType.Projectile);
                    coroutine = base.GameController.FindCharacterCard(httc, hero, SelectionType.HeroToDealDamage, results2, damageInfo: new DealDamageAction[] { dd }, cardSource: new CardSource(FindCardController(effect.CardSource)));
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
                        coroutine = base.GameController.SelectTargetsAndDealDamage(httc, new DamageSource(base.GameController, selected), num2, DamageType.Projectile, num1, false, num1, cardSource: new CardSource(FindCardController(effect.CardSource)));
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
            }
        }

        public string GenerateKey(string key, BulletHellStatusEffect effect, TurnTaker hero)
        {
            return key + effect.ID + hero.Name;
        }
    }
}

