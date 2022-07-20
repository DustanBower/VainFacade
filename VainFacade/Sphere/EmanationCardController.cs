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
            LinqCardCriteria toDestroy = SphereUtilityCardController.isEmanationInPlay;
            if (choice != null && choice.SelectedCard != null)
            {
                toDestroy = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(emanationKeyword) && c.Title != choice.SelectedCard.Title, "Emanations in play other than copies of " + choice.SelectedCard.Title, false, false, "Emanation in play other than copies of " + choice.SelectedCard.Title, "Emanations in play other than copies of " + choice.SelectedCard.Title);
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
            yield break;
        }
    }
}
