using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class PlaybookCardController : BlitzUtilityCardController
    {
        public PlaybookCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If not in play: show list of Playbook cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(IsPlaybook).Condition = () => !base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is destroyed, play the top card of the villain deck."
            AddWhenDestroyedTrigger(PlayTheTopCardOfTheVillainDeckResponse, TriggerType.PlayCard);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy all other Playbooks."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.DoKeywordsContain(PlaybookKeyword) && c != base.Card, "other Playbook"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
