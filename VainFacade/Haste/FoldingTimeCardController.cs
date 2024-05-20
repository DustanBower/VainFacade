using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class FoldingTimeCardController:HasteUtilityCardController
	{
		public FoldingTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(SpeedPool);
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
        }

        public override string DecisionAction => " to increase this damage by 1";


        private Card Target
        {
            get;
            set;
        }

        private YesNoAmountDecision yesNo
        {
            get;
            set;
        }

        public override void AddTriggers()
        {
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cep) => cep.CardEnteringPlay.Owner == this.TurnTaker && cep.CardEnteringPlay != this.Card, CardPlayResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.CardSource != null && ((dd.CardSource.Card == this.CardWithoutReplacements && dd.CardSource.Card == this.Card) || (dd.CardSource.Card != this.CardWithoutReplacements && dd.CardSource.Card == this.Card && dd.CardSource.AssociatedCardSources.Any((CardSource cs) => cs.Card == this.CardWithoutReplacements))) && HasteSpeedPoolUtility.GetSpeedPool(this) != null && (HasteSpeedPoolUtility.GetSpeedPool(this).CurrentValue > 0 || (yesNo != null && DidPlayerAnswerYes(yesNo))), IncreaseResponse, new TriggerType[] { TriggerType.ModifyTokens, TriggerType.ModifyDamageAmount }, TriggerTiming.Before);

            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.IsSameCard(this.CharacterCard) && dd.Target == this.CharacterCard && dd.DidDealDamage, (DealDamageAction dd) => DrawCards(DecisionMaker, dd.Amount, false, true), TriggerType.DrawCard, TriggerTiming.After, ActionDescription.DamageTaken);
        }

        private IEnumerator CardPlayResponse(CardEntersPlayAction cep)
        {
            return base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 1, DamageType.Energy, 1, false, 0, cardSource: GetCardSource());
        }

        //Based on Twist the Ether
        private IEnumerator IncreaseResponse(DealDamageAction dealDamage)
        {
            //Log.Debug($"Entered Folding Time response. yesNo is {(yesNo == null ? "null" : DidPlayerAnswerYes(yesNo).ToString())}");
            if ((base.GameController.PretendMode || dealDamage.Target != Target))
            {
                SelectionType type = SelectionType.Custom;
                yesNo = new YesNoAmountDecision(this.GameController, DecisionMaker, type, 1, upTo: false, requireUnanimous: false, dealDamage, null, GetCardSource());
                IEnumerator coroutine = base.GameController.MakeDecisionAction(yesNo);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                //Log.Debug($"Entered first if statement. yesNo is {(yesNo == null ? "null" : DidPlayerAnswerYes(yesNo).ToString())}");
            }

            if (yesNo != null && DidPlayerAnswerYes(yesNo))
            {
                IEnumerator coroutine2 = base.GameController.IncreaseDamage(dealDamage,1,cardSource:GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (!base.GameController.PretendMode)
            {
                if (yesNo != null && DidPlayerAnswerYes(yesNo))
                {
                    //IEnumerator coroutine3 = base.GameController.RemoveTokensFromPool(SpeedPool, 1, null, cardSource: GetCardSource());
                    IEnumerator coroutine3 = RemoveSpeedTokens(1);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine3);
                    }
                }
            }
        }
    }
}

