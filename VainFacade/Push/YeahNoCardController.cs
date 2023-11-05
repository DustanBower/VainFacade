using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class YeahNoCardController:PushCardControllerUtilities
	{
		public YeahNoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Destroy a target with 4 or fewer HP.
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints <= 4, "", false, false, "target with 4 or fewer HP", "targets with 4 or fewer HP"), false, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDestroyCards(results))
            {
                Card destroyed = GetDestroyedCards(results).FirstOrDefault();
                int amount = results.FirstOrDefault().HitPointsOfCardBeforeItWasDestroyed.Value;
                if (!destroyed.IsCharacter && destroyed.Location.IsTrash)
                {
                    //You may shuffle a non-character target destroyed this way into its deck.
                    List<YesNoCardDecision> YesNo = new List<YesNoCardDecision>();
                    coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, destroyed, storedResults: YesNo, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidPlayerAnswerYes(YesNo))
                    {
                        coroutine = base.GameController.ShuffleCardIntoLocation(DecisionMaker, destroyed, destroyed.Owner.Deck, false, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }

                        //If you do, increase the next damage dealt to Push by X, where X is the destroyed target's current HP before it was destroyed.
                        IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(amount);
                        effect.NumberOfUses = 1;
                        effect.TargetCriteria.IsSpecificCard = this.CharacterCard;
                        effect.UntilTargetLeavesPlay(this.CharacterCard);
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision && ((YesNoCardDecision)decision).Card != null)
            {
                Card target = ((YesNoCardDecision)decision).Card;
                string text = $"shuffle {target.Title} into its deck";
                return new CustomDecisionText(
                $"Do you want to {text}?",
                $"{decision.DecisionMaker.Name} is deciding whether to {text}.",
                $"Vote for whether to {text}.",
                $"whether to {text}."
                );
            }
            return null;

        }
    }
}