using System.Collections.Generic;

namespace prefaCalendarAdmin.Models.Session
{
    public class SessionStats
    {
        public decimal TotalAmount { get; set; }
        public int TotalCalendarsSold { get; set; }
        public List<SectorData> Sectors { get; set; } = new List<SectorData>();
    }

    public class SectorData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#6c757d";
        public decimal TotalAmount { get; set; }
        public int ToursCount { get; set; }
        public int CalendarsSold { get; set; }
    }
}
