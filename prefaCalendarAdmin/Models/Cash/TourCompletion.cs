using System.Collections.Generic;

namespace prefaCalendarAdmin.Models.Cash
{
    public class TourCompletion
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public int CalendarsSold { get; set; }
        
        // Billets
        public BanknotesDetail Banknotes { get; set; } = new BanknotesDetail();
        
        // Pièces
        public CoinsDetail Coins { get; set; } = new CoinsDetail();
        
        // Chèques
        public ChecksDetail Checks { get; set; } = new ChecksDetail();
        
        // Totaux
        public decimal TotalAmount { get; set; }
        public decimal BanknotesTotal { get; set; }
        public decimal CoinsTotal { get; set; }
        public decimal ChecksTotal { get; set; }
        
        public string Notes { get; set; } = string.Empty;
    }
    
    public class BanknotesDetail
    {
        public int Tickets5 { get; set; }
        public int Tickets10 { get; set; }
        public int Tickets20 { get; set; }
        public int Tickets50 { get; set; }
        public int Tickets100 { get; set; }
        public int Tickets200 { get; set; }
        public int Tickets500 { get; set; }
        
        public decimal GetTotal()
        {
            return (Tickets5 * 5) + (Tickets10 * 10) + (Tickets20 * 20) + 
                   (Tickets50 * 50) + (Tickets100 * 100) + (Tickets200 * 200) + 
                   (Tickets500 * 500);
        }
        
        public int GetTotalCount()
        {
            return Tickets5 + Tickets10 + Tickets20 + Tickets50 + Tickets100 + Tickets200 + Tickets500;
        }
    }
    
    public class CoinsDetail
    {
        public int Coins1c { get; set; }
        public int Coins2c { get; set; }
        public int Coins5c { get; set; }
        public int Coins10c { get; set; }
        public int Coins20c { get; set; }
        public int Coins50c { get; set; }
        public int Coins1e { get; set; }
        public int Coins2e { get; set; }
        
        public decimal GetTotal()
        {
            return (Coins1c * 0.01m) + (Coins2c * 0.02m) + (Coins5c * 0.05m) + 
                   (Coins10c * 0.10m) + (Coins20c * 0.20m) + (Coins50c * 0.50m) + 
                   (Coins1e * 1.00m) + (Coins2e * 2.00m);
        }
        
        public int GetTotalCount()
        {
            return Coins1c + Coins2c + Coins5c + Coins10c + Coins20c + Coins50c + Coins1e + Coins2e;
        }
    }
    
    public class ChecksDetail
    {
        public int CheckCount { get; set; }
        public decimal CheckTotalAmount { get; set; }
        public List<decimal> CheckAmounts { get; set; } = new List<decimal>();
    }
}
