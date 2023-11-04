using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Push
{
	public class HarmonicJunctionCardController:PushCardControllerUtilities
	{
		public HarmonicJunctionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsAnchor(c), "anchors", false, false));
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.HeroTurnTaker.Hand, AlterationCriteria());
		}

        private const string StartOfTurnEffectActivated = "StartOfTurnEffectActivated";
        private string playingAlterations = "PlayingAlterations";

        public override IEnumerator Play()
        {
            //You may play an anchor. If you do, play all alterations in your hand.
            List<PlayCardAction> results = new List<PlayCardAction>();
            IEnumerator coroutine = SelectAndPlayCardFromHand(DecisionMaker, true, results, AnchorCriteria(), false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidPlayCards(results) && !IsPropertyTrue(playingAlterations, (CardPropertiesJournalEntry e) => e.Key == playingAlterations))
            {
                SetCardProperty(playingAlterations, true);
                coroutine = SelectAndPlayCardsFromHand(DecisionMaker, 100, false, 100, AlterationCriteria());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                SetCardProperty(playingAlterations, false);
            }

            //Activate start of turn affects on all anchors, then destroy all alterations and anchors.
            if (FindCardsWhere((Card c) => IsAnchor(c) && c.IsInPlayAndHasGameText && c.BattleZone == this.Card.BattleZone, true, GetCardSource()).Count() > 0)
            {
                coroutine = base.GameController.SendMessageAction(this.Card.Title + " activates the start of turn text for each anchor in play!", Priority.High, GetCardSource(), showCardSource: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                //Based on Omnitron XI variant
                //IEnumerable<PhaseChangeTrigger> triggers = FindTriggersWhere((ITrigger t) => t.CardSource != null && t.CardSource.Card.IsEnvironment && t.CardSource.Card.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(t.CardSource.Card, GetCardSource()) && t is PhaseChangeTrigger pct && pct.PhaseCriteria(Phase.End)).Select(trigger => trigger as PhaseChangeTrigger).OrderBy((PhaseChangeTrigger pct) => pct.CardSource.Card.PlayIndex);
                //foreach (PhaseChangeTrigger trigger in triggers)
                //{
                //    if (base.Card.IsInPlayAndHasGameText)
                //    {
                //        coroutine = trigger.Response(pca);
                //        if (UseUnityCoroutines)
                //        {
                //            yield return GameController.StartCoroutine(coroutine);
                //        }
                //        else
                //        {
                //            GameController.ExhaustCoroutine(coroutine);
                //        }
                //    }
                //}

                //This method should account for new anchors coming into play during other cards' effects.
                bool continueLoop = true;
                while (continueLoop)
                {
                    IEnumerable<PhaseChangeTrigger> triggers = FindTriggersWhere((ITrigger t) => t.CardSource != null && IsAnchor(t.CardSource.Card) && t.CardSource.Card.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(t.CardSource.Card, GetCardSource()) && t is PhaseChangeTrigger pct && pct.PhaseCriteria(Phase.Start) && !IsPropertyTrue(GeneratePerTargetKey(StartOfTurnEffectActivated, pct.CardSource.Card))).Select(trigger => trigger as PhaseChangeTrigger).OrderBy((PhaseChangeTrigger pct) => pct.CardSource.Card.PlayIndex);
                    if (triggers.Count() > 0 && triggers.FirstOrDefault() != null)
                    {
                        PhaseChangeTrigger trigger = triggers.FirstOrDefault();
                        SetCardProperty(GeneratePerTargetKey(StartOfTurnEffectActivated, trigger.CardSource.Card), true);
                        coroutine = trigger.Response(null);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    else
                    {
                        continueLoop = false;
                    }
                }
            }
            else
            {
                coroutine = base.GameController.SendMessageAction("There are no anchors in play", Priority.High, GetCardSource(), showCardSource: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }

            coroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => IsAnchor(c) || IsAlteration(c), "", false, false, "alteration or anchor", "alterations or anchors"), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}

