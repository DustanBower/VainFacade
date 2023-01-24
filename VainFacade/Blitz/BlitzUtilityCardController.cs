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
    public class BlitzUtilityCardController : CardController
    {
        public BlitzUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string CircuitKeyword = "circuit";
        protected const string PlaybookKeyword = "playbook";

        public LinqCardCriteria IsCircuit = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CircuitKeyword), "Circuit");
        public LinqCardCriteria IsPlaybook = new LinqCardCriteria((Card c) => c.DoKeywordsContain(PlaybookKeyword), "Playbook");

        public IEnumerator IncreaseNextLightningDamage(int x = 1, CardSource cardSource = null)
        {
            // Increase the next lightning damage dealt by Blitz by X
            IncreaseDamageStatusEffect charge = new IncreaseDamageStatusEffect(x);
            charge.SourceCriteria.IsSpecificCard = base.CharacterCard;
            charge.NumberOfUses = 1;
            charge.DamageTypeCriteria.AddType(DamageType.Lightning);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(charge, true, cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }
    }
}
