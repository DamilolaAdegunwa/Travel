namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingResponseDTO
    {
        public int SeatNumber { get; set; }
        public string Response { get; set; }
        public decimal? Amount { get; set; }
        public long? MainBookerId { get; set; }
        public string BookingReferenceCode { get; set; }
        public string Route { get; set; }
        public string DepartureDate { get; set; }
        public string PickUpDetails { get; set; }
        public string SelectedSeats { get; set; }
        public string DepartureTime { get; set; }
        public string PaymentResponse { get; set; }
    }
}
