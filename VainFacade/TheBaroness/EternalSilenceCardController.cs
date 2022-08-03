using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class EternalSilenceCardController : BaronessUtilityCardController
    {
        public EternalSilenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero with highest resonance?
            // ...
        }

        public override IEnumerator Play()
        {
            // "Destroy all Blood cards belonging to the hero with the highest resonance."
            List<TurnTaker> found = new List<TurnTaker>();
            IEnumerator findCoroutine = base.GameController.DetermineTurnTakersWithMostOrFewest(true, 1, 1, (TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame, (TurnTaker tt) => Resonance(tt), SelectionType.MostCardsInPlay, found, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (found.Count() > 0)
            {
                List<DestroyCardAction> bloodDestroyed = new List<DestroyCardAction>();
                TurnTaker mostBlood = found.FirstOrDefault();
                IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => BloodCard().Criteria(c) && c.Owner == mostBlood, "Blood", singular: "card belonging to " + mostBlood.Name, plural: "cards belonging to " + mostBlood.Name), autoDecide: true, storedResults: bloodDestroyed, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
                // "{TheBaroness} deals that hero X times 2 melee damage, where X is the number of Blood cards destroyed in this way."
                int x = bloodDestroyed.Where((DestroyCardAction dca) => dca.WasCardDestroyed).Count();
                // Find a hero character target for mostBlood
                List<Card> chosen = new List<Card>();
                IEnumerator chooseCoroutine = FindCharacterCardToTakeDamage(mostBlood, chosen, base.CharacterCard, x * 2, DamageType.Infernal);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (chosen.Count() > 0)
                {
                    Card unluckyCharacter = chosen.FirstOrDefault();
                    IEnumerator damageCoroutine = DealDamage(base.CharacterCard, unluckyCharacter, x * 2, DamageType.Infernal, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                }
            }
        }
    }
}
