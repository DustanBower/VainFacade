using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class AfflictedCardController : EldrenwoodTargetCardController
    {
        public AfflictedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If Howls in the Distance is face up, remind the player that text on this card doesn't matter
            SpecialStringMaker.ShowSpecialString(() => "Howls in the Distance: " + base.Card.Title + " has transformed into a Werewolf!").Condition = () => base.GameController.DoesCardContainKeyword(base.Card, AfflictedKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            SpecialStringMaker.ShowSpecialString(() => "Howls in the Distance: " + base.Card.Title + " has no game text.").Condition = () => base.GameController.DoesCardContainKeyword(base.Card, AfflictedKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            // If Howls in the Distance is face up and this is a Small Werewolf, indicate which target this card will attack
            SpecialStringMaker.ShowSpecialString(() => "Howls in the Distance: At the end of the environment turn, each Small Werewolf deals the non-Werewolf with the lowest HP " + (H - 2).ToString() + " irreducible melee damage.").Condition = () => base.GameController.DoesCardContainKeyword(base.Card, SmallKeyword) && base.GameController.DoesCardContainKeyword(base.Card, WerewolfKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            SpecialStringMaker.ShowLowestHP(1, cardCriteria: NonWerewolf()).Condition = () => base.GameController.DoesCardContainKeyword(base.Card, SmallKeyword) && base.GameController.DoesCardContainKeyword(base.Card, WerewolfKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            // If Howls in the Distance is face up and this is a Clever Werewolf, indicate which target this card will attack
            SpecialStringMaker.ShowSpecialString(() => "Howls in the Distance: At the end of the environment turn, each Clever Werewolf deals the non-Werewolf with the highest HP " + (H + 1).ToString() + " melee damage.").Condition = () => base.GameController.DoesCardContainKeyword(base.Card, CleverKeyword) && base.GameController.DoesCardContainKeyword(base.Card, WerewolfKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            SpecialStringMaker.ShowHighestHP(2, cardCriteria: NonWerewolf()).Condition = () => base.GameController.DoesCardContainKeyword(base.Card, CleverKeyword) && base.GameController.DoesCardContainKeyword(base.Card, WerewolfKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            // If Howls in the Distance is face up and this is a Common Werewolf, indicate which target this card will attack
            SpecialStringMaker.ShowSpecialString(() => "Howls in the Distance: At the end of the environment turn, each Common Werewolf deals the non-Werewolf with the second lowest HP " + H.ToString() + " melee damage.").Condition = () => base.GameController.DoesCardContainKeyword(base.Card, CommonKeyword) && base.GameController.DoesCardContainKeyword(base.Card, WerewolfKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
            SpecialStringMaker.ShowLowestHP(2, cardCriteria: NonWerewolf()).Condition = () => base.GameController.DoesCardContainKeyword(base.Card, CommonKeyword) && base.GameController.DoesCardContainKeyword(base.Card, WerewolfKeyword) && CanActivateEffect(base.TurnTakerController, HowlsKey);
        }

        protected const string SmallKeyword = "small";
        protected const string CleverKeyword = "clever";
        protected const string CommonKeyword = "common";

        protected LinqCardCriteria NonWerewolf()
        {
            return new LinqCardCriteria((Card c) => c.IsTarget && !base.GameController.DoesCardContainKeyword(c, WerewolfKeyword), "non-Werewolf");
        }

        public override IEnumerator ReducedToZeroResponse()
        {
            if (CanActivateEffect(base.TurnTakerController, QuaintKey))
            {
                IEnumerator respondCoroutine = SlainInHumanFormResponse();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(respondCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(respondCoroutine);
                }
            }
            else
            {
                // Howls in the Distance: Afflicted targets are Werewolves and have no game text
                yield break;
            }
        }

        public virtual IEnumerator SlainInHumanFormResponse()
        {
            yield return null;
        }
    }
}
