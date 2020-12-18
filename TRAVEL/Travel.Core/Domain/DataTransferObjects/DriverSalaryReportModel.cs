using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class DriverSalaryReportModel
    {
        public decimal DriverFee { get; set; }
        public string DriverCode { get; set; }
        public int NoofTrips { get; set; }
        public string Name { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
