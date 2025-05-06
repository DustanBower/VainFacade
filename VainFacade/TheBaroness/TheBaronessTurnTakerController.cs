using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class TheBaronessTurnTakerController : TurnTakerController
    {
        public TheBaronessTurnTakerController(TurnTaker turnTaker, GameController gameController): base(turnTaker, gameController)
        {
            foreach (Card item in this.TurnTaker.GetAllCards().Where((Card c) => c.DoKeywordsContain("web") && c.MaximumHitPoints != 3 * gameController.Game.H))
            {
                //Set HP of webs
                item.SetMaximumHP(gameController.Game.H * 3, !item.IsInPlayAndHasGameText);
            }
        }

        public override IEnumerator StartGame()
        {
            if (base.FindCardController(base.CharacterCard) is TheBaronessSpiderCharacterCardController)
            {
                Card[] webs = new Card[] {
                    base.TurnTaker.FindCard("JustBusiness"),
                    base.TurnTaker.FindCard("PoliticsAsUsual"),
                    base.TurnTaker.FindCard("SecretSocieties")
                };

                foreach (Card c in webs)
                {
                    if (c.Location.IsOffToTheSide)
                    {
                        IEnumerator coroutine = GameController.PlayCard(this, c, isPutIntoPlay: true, cardSource: FindCardController(c).GetCardSource());
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

            // "Put the card Vampirism into play."
            Card vampirism = base.TurnTaker.GetCardByIdentifier("Vampirism");
            IEnumerator putCoroutine = base.GameController.PlayCard(this, vampirism, isPutIntoPlay: true, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }

            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
        }
    }
}
