using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class AreaCardController : ParadiseIsleUtilityCardController
    {
        public AreaCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If not in play: show list of Areas in play
            SpecialStringMaker.ShowListOfCardsInPlay(isArea).Condition = () => !base.Card.IsInPlayAndHasGameText;
        }

        public static readonly string AreaKeyword = "area";

        public static LinqCardCriteria isArea = new LinqCardCriteria((Card c) => c.DoKeywordsContain(AreaKeyword), "Area");
        public static LinqCardCriteria isAreaInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(AreaKeyword), "Area in play", false, false, "Area in play", "Areas in play");

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy all other Areas."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c != base.Card && c.DoKeywordsContain(AreaKeyword), "other Area"), cardSource: GetCardSource());
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
