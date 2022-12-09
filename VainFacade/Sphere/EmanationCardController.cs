using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class EmanationCardController : SphereUtilityCardController
    {
        public EmanationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Emanation cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(SphereUtilityCardController.isEmanation);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of your turn, select an Emanation. Destroy all Emanations that are not copies of that card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, FocusEmanations, TriggerType.DestroyCard);
        }

        protected IEnumerator FocusEmanations(PhaseChangeAction pca)
        {
            // "... select an Emanation. Destroy all Emanations that are not copies of that card."
            // If no Emanation is chosen, all Emanations will be destroyed
            LinqCardCriteria toDestroy = SphereUtilityCardController.isEmanationInPlay;
            List<Card> allEmanations = base.GameController.FindCardsWhere(isEmanationInPlay, visibleToCard: GetCardSource()).ToList();
            if (allEmanations.Where((Card c) => c.Title != base.Card.Title || c.Owner != base.Card.Owner).Any())
            {
                // If there are multiple different Emanations in play, the player chooses one
                List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.Custom, SphereUtilityCardController.isEmanationInPlay, storedResults, false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                SelectCardDecision choice = storedResults.FirstOrDefault();
                if (choice != null && choice.SelectedCard != null)
                {
                    toDestroy = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(emanationKeyword) && c.Title != choice.SelectedCard.Title, "in play other than copies of " + choice.SelectedCard.Title, false, true, "Emanation", "Emanations");
                }
            }
            else
            {
                // If all Emanations in play have the same title and deck identity, the player doesn't need to choose- nothing will be destroyed because there are no non-matching Emanations
                toDestroy = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(emanationKeyword) && (c.Title != base.Card.Title || c.Owner != base.Card.Owner), "in play other than copies of " + base.Card.Title, false, true, "Emanation", "Emanations");
            }
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(base.HeroTurnTakerController, toDestroy, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select an Emanation to maintain", "choosing an Emanation to maintain", "Vote for an Emanation for " + base.TurnTaker.Name + " to maintain", "Emanation to maintain");
        }
    }
}
