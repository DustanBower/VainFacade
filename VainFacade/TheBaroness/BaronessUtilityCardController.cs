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
		protected const string BatsIdentifier = "CloudOfBats";
		protected const string TerrorIdentifier = "WingedTerror";

		protected LinqCardCriteria BloodCard()
        {
			if (CanActivateEffect(DecisionMaker, VampirismKey))
            {
				return new LinqCardCriteria((Card c) => c.IsFaceDownNonCharacter && c.Location.IsPlayAreaOf(base.TurnTaker), "Blood");
            }
			else
            {
				return new LinqCardCriteria((Card c) => false, "Blood");
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
	}
}
