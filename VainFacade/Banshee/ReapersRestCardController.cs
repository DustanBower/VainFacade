using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class ReapersRestCardController:CardController
	{
		public ReapersRestCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => $"{base.Game.ActiveTurnTaker.Name} has not played a card this turn.").Condition = () => base.Game.ActiveTurnTaker.IsPlayer && GetNumberOfCardsPlayedThisTurn(FindTurnTakerController(base.Game.ActiveTurnTaker)) == 0;
            base.SpecialStringMaker.ShowSpecialString(() => $"{base.Game.ActiveTurnTaker.Name} has not used a power this turn.").Condition = () => base.Game.ActiveTurnTaker.IsPlayer && GetNumberOfPowersUsedThisTurn(FindHeroTurnTakerController(base.Game.ActiveTurnTaker.ToHero())) == 0;
            base.SpecialStringMaker.ShowSpecialString(() => $"{base.Game.ActiveTurnTaker.Name} has not drawn a card this turn.").Condition = () => base.Game.ActiveTurnTaker.IsPlayer && GetNumberOfCardsDrawnThisTurn(base.Game.ActiveTurnTaker) == 0;
            base.SpecialStringMaker.ShowSpecialString(() => $"{base.Game.ActiveTurnTaker.Name} has played a card, used a power, and drawn a card this turn.").Condition = () => base.Game.ActiveTurnTaker.IsPlayer && base.Game.ActiveTurnTaker.IsPlayer && GetNumberOfCardsPlayedThisTurn(FindTurnTakerController(base.Game.ActiveTurnTaker)) > 0 && GetNumberOfPowersUsedThisTurn(FindHeroTurnTakerController(base.Game.ActiveTurnTaker.ToHero())) > 0 && GetNumberOfCardsDrawnThisTurn(base.Game.ActiveTurnTaker) > 0;
        }

        public override IEnumerator Play()
        {
            //When this card enters play, you may draw and play a card.
            IEnumerator coroutine = DrawCard(this.HeroTurnTaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = SelectAndPlayCardFromHand(DecisionMaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddTriggers()
        {
            //At the end of each player's turn, if that player did not play a card, did not use a power, or did not draw a card this turn, select a target. Increase the next damage dealt to that target by 1.
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsPlayer && (GetNumberOfCardsPlayedThisTurn(FindTurnTakerController(tt)) == 0 || GetNumberOfPowersUsedThisTurn(FindHeroTurnTakerController(tt.ToHero())) == 0 || GetNumberOfCardsDrawnThisTurn(tt) == 0), IncreaseResponse, TriggerType.IncreaseDamage);
            //AddEndOfTurnTrigger((TurnTaker tt) => tt.IsPlayer, IncreaseResponse, TriggerType.IncreaseDamage);
        }

        private int GetNumberOfCardsDrawnThisTurn(TurnTaker tt)
        {
            return (from p in Journal.DrawCardEntriesThisTurn()
                    where p.Hero == tt
                    select p).Count();
        }

        private IEnumerator IncreaseResponse(PhaseChangeAction pca)
        {
            //Console.WriteLine("Entered trigger for Reaper's Rest");
            //Console.WriteLine($"{this.TurnTaker.Name} is selecting a card for {this.Card.Title}, play index {this.Card.PlayIndex.ToString()}");
            //Console.WriteLine($"{this.TurnTaker.Name} is selecting a card for {this.Card.Title}");
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.IncreaseDamageTaken, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText), results, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Card card = GetSelectedCard(results);
            if (card != null)
            {
                IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
                effect.NumberOfUses = 1;
                effect.TargetCriteria.IsSpecificCard = card;
                effect.UntilTargetLeavesPlay(card);
                coroutine = AddStatusEffect(effect);
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

