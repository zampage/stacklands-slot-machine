using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace SlotMachineNS
{
    public class SlotMachine : Mod
    {
        public override void Ready()
        {
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "slot_machine_card_idea", 1);
        }
    }

    public class SlotMachineCard : CardData
    {
        public CardBag LootBag = new CardBag();

        public CardBag ExplodeBag = new CardBag();

        public int ExplodeChance = 0;

        public int ExplodeChangeIncreaseStep = 10;

        public int MaxExplodeChance = 100;

        public int MaxGambleCoinLoot = 3;

        public int MaxCoinExplodeLoot = 5;

        public float GambleTime = 2f;

        protected override bool CanHaveCard(CardData otherCard)
        {
            // TODO:
            // be able to stack slots ontop of each other
            // but only one slot should run at once, not multiple
            // this is probably solved in UpdateCard...
            // maybe only start timer when there is a gold exactly on top of this card, and not anywhere in stack
            if (otherCard.Id == "gold") // || otherCard.Id == Id
                return true;

            return base.CanHaveCard(otherCard);
        }

        public override bool CanHaveCardsWhileHasStatus()
        {
            return true;
        }

        public override void UpdateCard()
        {
            if (ChildrenMatchingPredicateCount(card => card.Id == "gold") > 0)
            {
                MyGameCard.StartTimer(GambleTime, Gamble, SokLoc.Translate("slot_machine_card_gamble_status"), "gamble");
            }
            else
            {
                MyGameCard.CancelTimer("gamble");
            }

            base.UpdateCard();
        }

        [TimedAction("gamble")]
        public void Gamble()
        {
            // destroy 1 gold
            // TODO
            // not sure why I have to do `MyGameCard.GetRootCard().CardData`
            // saw that in GameScripts for other cards
            // but if I don't then somehow the slot does not get destroyed
            // which makes no sense to me
            MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c.Id == "gold", 1);

            if (new System.Random().Next(0, MaxExplodeChance) <= ExplodeChance)
            {
                MyGameCard.DestroyCard();
            }
            else
            {
                // increase explode chance
                ExplodeChance += ExplodeChangeIncreaseStep;

                // get random card from LootBag
                ICardId cardId = LootBag.GetCard(removeCard: false);

                // base values
                int numOfCards = 1;
                bool checkAddToStack = false;

                // modify if card is gold
                if (cardId.Id == "gold")
                {
                    numOfCards = new System.Random().Next(0, MaxGambleCoinLoot + 1);
                    checkAddToStack = true;
                }

                if (numOfCards > 0)
                {
                    // create card(s)
                    GameCard card = WorldManager.instance.CreateCardStack(Position, numOfCards, cardId.Id, checkAddToStack: checkAddToStack);

                    // spawn animation
                    WorldManager.instance.CreateSmoke(Position);

                    // spawn card(s)
                    WorldManager.instance.StackSend(card);

                    // TODO
                    // the spawning of coins here looks a bit strange in game
                    // the stack jitters a lot when coins are spawned and added
                }

            }
        }

        public override void OnDestroyCard()
        {
            // get random card from ExplodeBag
            ICardId cardId = ExplodeBag.GetCard(removeCard: false);
            int numOfCards = 1;

            if (cardId.Id == "gold")
            {
                numOfCards = new System.Random().Next(0, MaxCoinExplodeLoot + 1);
            }

            // explode animation
            WorldManager.instance.CreateSmoke(Position);

            // spawn something as compensation
            WorldManager.instance.CreateCardStack(Position, numOfCards, cardId.Id, checkAddToStack: false);

            base.OnDestroyCard();
        }
    }
}