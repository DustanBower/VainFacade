using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class FridayCharacterCardController:HeroCharacterCardController
	{
		public FridayCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            //Show all damage types that have been dealt since the end of your last turn
            base.SpecialStringMaker.ShowSpecialString(() => HadLastTurn() ? SpecialStringText() : TextIfNoLastTurn());

            //This needs to be here so Friday can copy Completionist Guise's power with Prototype Combat Mimic
            AddThisCardControllerToList(CardControllerListType.UnderCardsCanHaveText);
        }

        private bool HadLastTurn()
        {
            return base.Game.Journal.PhaseChangeEntries().Where(base.Game.Journal.SinceLastTurn<PhaseChangeJournalEntry>(this.TurnTaker)).Any();
        }

        private string SpecialStringText()
        {
            return $"Damage types dealt since the end of {this.TurnTaker.Name}'s last turn: {GetDamageTypesSinceLastTurn().Select((DamageType t) => t.ToString()).ToCommaList()}";
        }

        private string TextIfNoLastTurn()
        {
            return $"There has not been a previous turn for {this.TurnTaker.Name}";
        }

        private IEnumerable<DamageType> GetDamageTypesSinceLastTurn()
        {
            //"Since the end of Friday's last turn" does not include damage dealt during Friday's end-of-turn phase
            //In OblivAeon mode, this sees all damage that was dealt in both battle zones since Friday's last turn
            return base.GameController.Game.Journal.QueryJournalEntries((DealDamageJournalEntry e) => e.Amount > 0).Where(base.GameController.Game.Journal.SinceLastTurn<DealDamageJournalEntry>(this.TurnTaker)).Select((DealDamageJournalEntry dde) => dde.DamageType).Distinct();
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //{Friday} deals 1 target 2 melee or projectile damage or 3 damage of a type that has been dealt since the end of your last turn.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 2);
            int num3 = GetPowerNumeral(2, 3);

            IEnumerable<DamageType> DamageTypes = GetDamageTypesSinceLastTurn();

            IEnumerable<Function> functionChoices = new Function[2]
                {
            new Function(base.HeroTurnTakerController, "Deal " + num1 + " target " + num2 + " melee or projectile damage", SelectionType.DealDamage, () => PowerResponse(num1, num2, new DamageType[2] {DamageType.Melee, DamageType.Projectile }), forcedActionMessage: HadLastTurn() ? "No damage has been dealt since the end of your last turn" : TextIfNoLastTurn()),
            new Function(base.HeroTurnTakerController, "Deal " + num1 + " target " + num3 + " damage of a type that has been dealt since the end of your last turn", SelectionType.DealDamage, () => PowerResponse(num1, num3, DamageTypes), DamageTypes.Count() > 0)
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
        }

        private IEnumerator PowerResponse(int num1, int num2, IEnumerable<DamageType> DamageTypes)
        {
            DamageType? type = DamageTypes.FirstOrDefault();
            IEnumerator coroutine;
            if (DamageTypes.Count() > 1)
            {
                List<SelectDamageTypeDecision> results = new List<SelectDamageTypeDecision>();
                coroutine = base.GameController.SelectDamageType(DecisionMaker, results, DamageTypes, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                type = GetSelectedDamageType(results);
            }
            else
            {
                if (type.HasValue)
                {
                    coroutine = base.GameController.SendMessageAction($"{type.Value} is the only available type", Priority.Low, GetCardSource());
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
            
            if (type.HasValue)
            {
                coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.Card), (Card c) => num2, type.Value, () => num1, false, num1, cardSource: GetCardSource());
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

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    //Once before the start of your next turn, when a target deals damage, it may deal 1 damage of the same type to the same target.
                    OnDealDamageStatusEffect effect = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "Incap1Response", "When a target deals damage, it may deal 1 damage of the same type to the same target", new TriggerType[] { TriggerType.DealDamage }, this.TurnTaker, this.Card);
                    effect.UntilStartOfNextTurn(this.TurnTaker);
                    effect.SourceCriteria.IsTarget = true;
                    effect.BeforeOrAfter = BeforeOrAfter.After;
                    effect.DamageAmountCriteria.GreaterThan = 0;
                    coroutine = AddStatusEffect(effect);
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
                    //One player may put the top card of their trash into play if that card is an ongoing or one-shot. If they do, discard the top card of that deck.
                    SelectTurnTakerDecision decision = new SelectTurnTakerDecision(base.GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && tt.Trash.HasCards && (IsOngoing(tt.Trash.TopCard) || tt.Trash.TopCard.IsOneShot)), SelectionType.PutIntoPlay, true, cardSource: GetCardSource());
                    coroutine = base.GameController.SelectTurnTakerAndDoAction(decision, Incap2Response);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
                case 2:
                    //One hero may use a power.
                    coroutine = base.GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
            }
        }

        public IEnumerator Incap1Response(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            //...it may deal 1 damage of the same type to the same target
            Card source = dda.DamageSource.Card;
            DamageType type = dda.DamageType;
            Card target = dda.Target;
            HeroTurnTakerController decisionmaker = FindHeroTurnTakerController(hero.ToHero());
            //IEnumerator coroutine = DealDamage(source, target, 1, type, false, true, cardSource: new CardSource(FindCardController(effect.CardSource)) );
            //if (base.UseUnityCoroutines)
            //{
            //    yield return base.GameController.StartCoroutine(coroutine);
            //}
            //else
            //{
            //    base.GameController.ExhaustCoroutine(coroutine);
            //}

            if (target.IsInPlayAndHasGameText)
            {
                List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
                IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(decisionmaker, SelectionType.DealDamage, source, new DealDamageAction(base.GameController, new DamageSource(base.GameController, source), target, 1, type), storedResults, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidPlayerAnswerYes(storedResults))
                {
                    base.GameController.StatusEffectManager.RemoveStatusEffect(effect);
                    //coroutine = base.GameController.ExpireStatusEffect(effect, GetCardSource());
                    //if (base.UseUnityCoroutines)
                    //{
                    //    yield return base.GameController.StartCoroutine(coroutine);
                    //}
                    //else
                    //{
                    //    base.GameController.ExhaustCoroutine(coroutine);
                    //}

                    coroutine = DealDamage(source, target, 1, type, false, false, cardSource: new CardSource(FindCardController(effect.CardSource)));
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

        private IEnumerator Incap2Response(TurnTaker player)
        {
            //...put the top card of their trash into play if that card is an ongoing or one-shot. If they do, discard the top card of that deck.
            TurnTakerController ttc = FindTurnTakerController(player);
            List<Card> results = new List<Card>();
            IEnumerator coroutine = base.GameController.PlayTopCardOfLocation(ttc, player.Trash, false, isPutIntoPlay: true, cardSource: GetCardSource(), playedCards: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (results.FirstOrDefault() != null)
            {
                //I interpret the text as meaning that Friday is the one discarding the top card, not the hero whose card was played.
                coroutine = DiscardCardsFromTopOfDeck(ttc, 1, responsibleTurnTaker: this.TurnTaker);
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

