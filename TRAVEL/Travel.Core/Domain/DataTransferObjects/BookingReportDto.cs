
namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingReportDto
    {
        public int SeatNumber { get; set; } 
        public string BookingReferenceCode { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string NextOfKinName { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal RerouteFeeDiff { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string TerminalName { get; set; }
        public string Destination { get; set; }


        public int TotalCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalDiscountedSales { get; set; }

    }
}
