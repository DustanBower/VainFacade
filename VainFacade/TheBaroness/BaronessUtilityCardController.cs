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
    public class BaronessUtilityCardController : CardController
    {
        public BaronessUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string SchemeKeyword = "scheme";
        protected const string SpellKeyword = "spell";
        protected const string VampirismKey = "VampirismEffectKey";
		protected const string VampirismIdentifier = "Vampirism";
		protected const string CloudIdentifier = "CloudOfBats";
		protected const string TerrorIdentifier = "WingedTerror";
		protected const string BatIdentifier = "Bat";
		protected const string BatKeyword = "bat";

		protected bool IsVampirismInPlay()
        {
			Card vCard = base.TurnTaker.FindCard(VampirismIdentifier);
			return vCard.IsInPlayAndHasGameText;
        }

		protected LinqCardCriteria BloodCard()
        {
			if (IsVampirismInPlay())
            {
				return new LinqCardCriteria((Card c) => c.IsFaceDownNonCharacter && c.Location.IsPlayAreaOf(base.TurnTaker), "Blood");
            }
			else
            {
				return new LinqCardCriteria((Card c) => false, "Blood");
            }
        }

		protected int Resonance(TurnTaker tt)
		{
			//Log.Debug("BaronessUtilityCardController.Resonance: IsVampirismInPlay(): " + IsVampirismInPlay().ToString());
			if (IsVampirismInPlay())
            {
				return base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => BloodCard().Criteria(c) && c.Owner == tt)).Count();
            }
			else
            {
				return 0;
            }
        }

		protected IEnumerator MoveFaceDownToVillainPlayArea(Card card)
		{
			IEnumerator coroutine = base.GameController.MoveCard(base.TurnTakerController, card, base.TurnTaker.PlayArea, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: false, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: true, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(coroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(coroutine);
			}
        }

        public SpecialString ShowResonancePerHero()
        {
            Func<string> output = delegate
            {
                List<string> list = new List<string>();
                foreach (HeroTurnTakerController httc in base.GameController.FindHeroTurnTakerControllers())
                {
                    if (!httc.IsIncapacitatedOrOutOfGame)
                    {
                        list.Add(httc.Name + "'s resonance is " + Resonance(httc.TurnTaker).ToString());
                    }
                }
                return list.ToCommaList(useWordAnd: true) + ".";
            };
            return SpecialStringMaker.ShowSpecialString(output, () => base.Card.IsInPlayAndHasGameText, () => base.GameController.FindCardsWhere(BloodCard(), visibleToCard: GetCardSource()));
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            if (FindCardsWhere(BloodCard()).Any())
            {
                List<Card> list = GetOrderedCardsInLocation(TurnTaker.PlayArea).ToList();
                Card firstBlood = list.Where((Card c) => BloodCard().Criteria(c)).FirstOrDefault();
                int num = list.IndexOf(firstBlood);
                list.Insert(num, base.Card);
                list.ForEach(delegate (Card c)
                {
                    base.GameController.Game.AssignPlayCardIndex(c);
                });
            }

            storedResults.Add(new MoveCardDestination(TurnTaker.PlayArea));

            yield return null;
        }

        private IEnumerable<Card> GetOrderedCardsInLocation(Location location)
        {
            return location.Cards.OrderBy((Card c) => c.PlayIndex);
        }
    }
}
