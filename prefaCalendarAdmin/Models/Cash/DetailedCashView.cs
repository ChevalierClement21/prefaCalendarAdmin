using System.Collections.Generic;
using System.Linq;

namespace prefaCalendarAdmin.Models.Cash
{
    public class DetailedCashView
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public List<TourCompletion> Tours { get; set; } = new List<TourCompletion>();
        
        // Totaux de la session
        public decimal TotalAmount => Tours.Sum(t => t.TotalAmount);
        public int TotalCalendarsSold => Tours.Sum(t => t.CalendarsSold);
        public int TotalCheckCount => Tours.Sum(t => t.Checks.CheckCount);
        public decimal TotalCheckAmount => Tours.Sum(t => t.Checks.CheckTotalAmount);
        
        // Totaux par type de monnaie
        public decimal TotalBanknotes => Tours.Sum(t => t.BanknotesTotal);
        public decimal TotalCoins => Tours.Sum(t => t.CoinsTotal);
        
        // Statistiques des billets
        public BanknotesStatistics BanknotesStats => new BanknotesStatistics
        {
            Total5 = Tours.Sum(t => t.Banknotes.Tickets5),
            Total10 = Tours.Sum(t => t.Banknotes.Tickets10),
            Total20 = Tours.Sum(t => t.Banknotes.Tickets20),
            Total50 = Tours.Sum(t => t.Banknotes.Tickets50),
            Total100 = Tours.Sum(t => t.Banknotes.Tickets100),
            Total200 = Tours.Sum(t => t.Banknotes.Tickets200),
            Total500 = Tours.Sum(t => t.Banknotes.Tickets500)
        };
        
        // Statistiques des pièces
        public CoinsStatistics CoinsStats => new CoinsStatistics
        {
            Total1c = Tours.Sum(t => t.Coins.Coins1c),
            Total2c = Tours.Sum(t => t.Coins.Coins2c),
            Total5c = Tours.Sum(t => t.Coins.Coins5c),
            Total10c = Tours.Sum(t => t.Coins.Coins10c),
            Total20c = Tours.Sum(t => t.Coins.Coins20c),
            Total50c = Tours.Sum(t => t.Coins.Coins50c),
            Total1e = Tours.Sum(t => t.Coins.Coins1e),
            Total2e = Tours.Sum(t => t.Coins.Coins2e)
        };
    }
    
    public class BanknotesStatistics
    {
        public int Total5 { get; set; }
        public int Total10 { get; set; }
        public int Total20 { get; set; }
        public int Total50 { get; set; }
        public int Total100 { get; set; }
        public int Total200 { get; set; }
        public int Total500 { get; set; }
        
        public decimal GetTotalValue()
        {
            return (Total5 * 5) + (Total10 * 10) + (Total20 * 20) + 
                   (Total50 * 50) + (Total100 * 100) + (Total200 * 200) + 
                   (Total500 * 500);
        }
        
        public int GetTotalCount()
        {
            return Total5 + Total10 + Total20 + Total50 + Total100 + Total200 + Total500;
        }
    }
    
    public class CoinsStatistics
    {
        public int Total1c { get; set; }
        public int Total2c { get; set; }
        public int Total5c { get; set; }
        public int Total10c { get; set; }
        public int Total20c { get; set; }
        public int Total50c { get; set; }
        public int Total1e { get; set; }
        public int Total2e { get; set; }
        
        public decimal GetTotalValue()
        {
            return (Total1c * 0.01m) + (Total2c * 0.02m) + (Total5c * 0.05m) + 
                   (Total10c * 0.10m) + (Total20c * 0.20m) + (Total50c * 0.50m) + 
                   (Total1e * 1.00m) + (Total2e * 2.00m);
        }
        
        public int GetTotalCount()
        {
            return Total1c + Total2c + Total5c + Total10c + Total20c + Total50c + Total1e + Total2e;
        }
    }
}
