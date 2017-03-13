using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamBot
{
    public class TF2Currency
    {
        public int Hat { get; private set; }
        public int Refined { get; private set; }
        public int Reclaimed { get; private set; }
        public int Scrap { get; private set; }
        public int Weapon { get; private set; }
        public int Card { get; private set; }
        public int FoilCard { get; private set; }

        public TF2Currency(int Hat = 0, int Refined = 0, int Reclaimed = 0, int Scrap = 0, int Weapon = 0, int Card = 0, int FoilCard = 0)
        {
            this.Hat = Hat;
            this.Refined = Refined;
            this.Reclaimed = Reclaimed;
            this.Scrap = Scrap;
            this.Weapon = Weapon;
            this.Card = Card;
            this.FoilCard = FoilCard;
        }

        public TF2Currency(double deMetal)
        {
            Hat = 0;
            Refined = 0;
            Reclaimed = 0;
            Scrap = 0;
            Weapon = 0;
            Card = 0;
            FoilCard = 0;

            while (deMetal >= 1)
            {
                deMetal--;
                Refined++;
            }
            while (deMetal >= 0.3)
            {
                deMetal -= 0.3;
                Reclaimed++;
            }

            while (deMetal >= 0.1)
            {
                deMetal -= 0.1;
                Scrap++;
            }


            //if (deMetal >= 0.05)
            //{
            //    Weapon++;
            //}
        }


        public void AddHat(int amount = 1)
        {
            this.Hat += amount;
        }

        public void AddRef(int amount = 1)
        {
            this.Refined+= amount;
        }

        public void AddRec(int amount = 1)
        {
            this.Reclaimed+= amount;
        }

        public void AddScrap(int amount = 1)
        {
            this.Scrap+= amount;
        }

        public void AddWeapon(int amount = 1)
        {
            this.Weapon+= amount;
        }

        public void AddCard(int amount = 1)
        {
            this.Card += amount;
        }

        public void AddFoilCard(int amount = 1)
        {
            this.FoilCard += amount;
        }

        public void RemoveHat(int amount = 1)
        {
            this.Hat-= amount;
        }

        public void RemoveRef(int amount = 1)
        {
            this.Refined-= amount;
        }

        public void RemoveRec(int amount = 1)
        {
            this.Reclaimed-= amount;
        }

        public void RemoveScrap(int amount = 1)
        {
            this.Scrap-= amount;
        }

        public void RemoveWeapon(int amount = 1)
        {
            this.Weapon-= amount;
        }

        public void RemoveCard(int amount = 1)
        {
            this.Card -= amount;
        }

        public void RemoveFoilCard(int amount = 1)
        {
            this.FoilCard -= amount;
        }

        public TF2Currency GetChange(TF2Currency other)
        {
            TF2Currency change = new TF2Currency();

            int weapons = 0;
            int myweapons = 0;

            weapons += other.Hat * 24;
            weapons += other.Refined * 18;
            weapons += other.Reclaimed * 6;
            weapons += other.Scrap * 2;
            weapons += other.Weapon;
            weapons += (int)(clsFunctions.tfPriceToBuyCards.ToScrap() * this.Card) / 2;
            weapons += (int)(clsFunctions.tfPriceToBuyFoilCards.ToScrap() * this.FoilCard) / 2;

            myweapons += this.Hat * 24;
            myweapons += this.Refined * 18;
            myweapons += this.Reclaimed * 6;
            myweapons += this.Scrap * 2;
            myweapons += this.Weapon;
            myweapons += (int)(clsFunctions.tfPriceToSellCards.ToScrap() * this.Card) / 2;
            myweapons += (int)(clsFunctions.tfPriceToSellFoilCards.ToScrap() * this.FoilCard) / 2;

            int ichange = myweapons - weapons;

            if (ichange > 0)
            {
                while (ichange >= 18)
                {
                    ichange -= 18;
                    change.AddRef();
                }
                while (ichange >= 6)
                {
                    ichange -= 6;
                    change.AddRec();
                }
                while (ichange >= 2)
                {
                    ichange -= 2;
                    change.AddScrap();
                }
                while (ichange >= 1)
                {
                    ichange--;
                    change.AddWeapon();
                }
            }
            else
            {
                while (ichange <= -18)
                {
                    ichange += 18;
                    change.RemoveRef();
                }
                while (ichange <= -6)
                {
                    ichange += 6;
                    change.RemoveRec();
                }
                while (ichange <= -2)
                {
                    ichange += 2;
                    change.RemoveScrap();
                }
                while (ichange <= -1)
                {
                    ichange++;
                    change.RemoveWeapon();
                }
            }
        
            return change;
        }

        public void Clear()
        {
            this.Hat = 0;
            this.Refined = 0;
            this.Reclaimed = 0;
            this.Scrap = 0;
            this.Weapon = 0;
            this.Card = 0;
            this.FoilCard = 0;
        }

        public bool Neutral()
        {
            return (this.Weapon == 0 && this.Scrap == 0 && this.Reclaimed == 0 && this.Refined == 0 && this.Hat == 0 && this.Card == 0 && this.FoilCard == 0);
        }

        public bool Positive()
        {
            return (this.Weapon > 0 || this.Scrap > 0 || this.Reclaimed > 0 || this.Refined > 0 || this.Hat > 0 || this.Card > 0 || this.FoilCard > 0);
        }

        public bool Negative()
        {
            return (this.Weapon < 0 || this.Scrap < 0 || this.Reclaimed < 0 || this.Refined < 0 || this.Hat < 0 || this.Card < 0 || this.FoilCard < 0);
        }

        public void MakePositive()
        {
            this.Weapon = Math.Abs(this.Weapon);
            this.Scrap = Math.Abs(this.Scrap);
            this.Reclaimed = Math.Abs(this.Reclaimed);
            this.Refined = Math.Abs(this.Refined);
            this.Hat = Math.Abs(this.Hat);
            this.Card = Math.Abs(this.Card);
            this.FoilCard = Math.Abs(this.FoilCard);
        }

        public int ToHats()
        {
            int hatTotal = 0;
            int refs = Refined;
            int recs = Reclaimed;

            while (refs >= 4)
            {
                refs -= 4;
                hatTotal += 3;
            }
            while (recs >= 4)
            {
                recs -= 4;
                hatTotal++;
            }
            while (recs > 0 && refs > 0)
            {
                recs--;
                refs--;
                hatTotal++;
            }

            return hatTotal;
        }

        public int ToWeps()
        {
            int wepTotal = 0;

            int refs = Refined;
            int recs = Reclaimed;
            int scraps = Scrap;

            while (refs > 0)
            {
                wepTotal += 18;
                refs--;
            }

            while (recs > 0)
            {
                wepTotal += 6;
                recs--;
            }
            while (scraps > 0)
            {
                wepTotal += 2;
                scraps--;
            }


            return wepTotal;
        }

        public double ToMetal()
        {
            double dMetal = 0.0;

            int iRef = Refined;
            int iRec = Reclaimed;
            int iScrap = Scrap;
            int iWeps = Weapon;
            int iHats = Hat;
            int iCards = Card;
            int ifoilcard = FoilCard;

            while (iRef > 0)
            {
                iRef--;
                dMetal += 9;
            }

            while (iRec > 0)
            {
                iRec--;
                dMetal += 3;
            }

            while (iScrap > 0)
            {
                iScrap--;
                dMetal++;
            }

            while (iHats > 0)
            {
                iHats--;
                dMetal += 12;
            }

            while (iWeps > 0)
            {
                iWeps--;
                dMetal += 0.5;
            }


            return dMetal;
        }

        public double ToScrap(bool bUsersCurrency = false)
        {
            double dTotalScrap = 0;

            int iRef = Refined;
            int iRec = Reclaimed;
            int iScrap = Scrap;
            int iWeps = Weapon;
            int iHats = Hat;
            int iCards = Card;
            int iFoilCards = FoilCard;



            while (iRef > 0)
            {
                iRef--;
                dTotalScrap += 9;
            }

            while (iRec > 0)
            {
                iRec--;
                dTotalScrap += 3;
            }

            dTotalScrap += iScrap;

            while (iWeps > 0)
            {
                iWeps--;
                dTotalScrap += 0.5;
            }
            while (iHats > 0)
            {
                iHats--;
                dTotalScrap += 12;
            }
            if (bUsersCurrency)
            {
                while (iFoilCards > 0)
                {
                    iFoilCards--;
                    dTotalScrap += clsFunctions.tfPriceToBuyFoilCards.ToScrap();
                }

                while (iCards > 0)
                {
                    iCards--;
                    dTotalScrap += clsFunctions.tfPriceToBuyCards.ToScrap();
                }
            }
            else
            {
                while (iFoilCards > 0)
                {
                    iFoilCards--;
                    dTotalScrap += clsFunctions.tfPriceToSellFoilCards.ToScrap();
                }

                while (iCards > 0)
                {
                    iCards--;
                    dTotalScrap += clsFunctions.tfPriceToSellCards.ToScrap();
                }
            }
            return dTotalScrap;
        }

        public double ToPrice(bool bUsersCurrency = false)
        {
            double dePrice = 0.0;
            int iRef = Refined;
            int iRec = Reclaimed;
            int iScrap = Scrap;
            int iWeps = Weapon;
            int iHats = Hat;
            int iFoilCards = FoilCard;
            int iCards = Card;

            int cardScrap = 0;
            if (bUsersCurrency)
            {
                while (iFoilCards > 0)
                {
                    iFoilCards--;
                    cardScrap += (int)clsFunctions.tfPriceToBuyFoilCards.ToScrap();
                }

                while (iCards > 0)
                {
                    iCards--;
                    cardScrap += (int)clsFunctions.tfPriceToBuyCards.ToScrap();
                }
            }
            else
            {
                while (iFoilCards > 0)
                {
                    iFoilCards--;
                    cardScrap += (int)clsFunctions.tfPriceToSellFoilCards.ToScrap();
                }

                while (iCards > 0)
                {
                    iCards--;
                    cardScrap += (int)clsFunctions.tfPriceToSellCards.ToScrap();
                }
            }

            while (cardScrap >= 9)
            {
                iRef++;
                cardScrap -= 9;
            }
            while (cardScrap >= 3)
            {
                iRec++;
                cardScrap -= 3;
            }
            while (cardScrap >= 1)
            {
                iScrap++;
                cardScrap--;
            }


            while (iRef > 0)
            {
                dePrice++;
                iRef--;
            }
            //adds the xx.yy (x)

            while (iHats > 0)
            {
                iHats--;
                dePrice += 1.33;
                if (dePrice.ToString().EndsWith(".99"))
                {
                    dePrice -= 0.99;
                    dePrice++;
                }
            }

            while (iRec > 0)
            {
                dePrice += 0.33;
                iRec--;
                if (dePrice.ToString().EndsWith(".99"))
                {
                    dePrice -= 0.99;
                    dePrice++;
                }
            }

            while (iScrap > 0)
            {
                dePrice += 0.11;
                iScrap--;
                if (dePrice.ToString().EndsWith(".99"))
                {
                    dePrice -= 0.99;
                    dePrice++;
                }
            }

            while (iWeps > 0)
            {
                if (iWeps >= 2)
                {
                    iWeps -= 2;
                    dePrice += 0.11;
                    if (dePrice.ToString().EndsWith(".99"))
                    {
                        dePrice -= 0.99;
                        dePrice++;
                    }
                }
                else
                {
                    iWeps--;                    
                    dePrice += 0.04;
                }
            }





            return dePrice;
        }

        /// <summary>
        /// Used to turn currency into keys
        /// </summary>
        /// <param name="LowValue">If inventory should be counted from keys low value</param>
        /// <returns>Amount of keys</returns>
        public int ToKeys(bool LowValue = true)
        {
            int iKeys = 0;

            

            int TotalScrap = Convert.ToInt32(this.ToScrap());

            int KeyScrap;

            if (LowValue)
            {
                KeyScrap = Convert.ToInt32(clsFunctions.KEY_BUY_VALUE.ToScrap());
            }
            else
            {
                KeyScrap = Convert.ToInt32(clsFunctions.KEY_SELL_VALUE.ToScrap());
            }
            if (KeyScrap != 0)
            {
                iKeys = TotalScrap / KeyScrap;
            }
            else
            {
                iKeys = 0;
            }
            


            return iKeys;
        }
    }
}
