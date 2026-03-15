namespace TransactionProcessor.Models.MerchantSchedule
{
    public class MerchantSchedule
    {
        public Guid MerchantScheduleId { get; set; }

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Int32 Year { get; set; }

        public List<MerchantScheduleMonth> Months { get; set; } = [];
    }

    public class MerchantScheduleMonth
    {
        public Int32 Month { get; set; }

        public List<Int32> ClosedDays { get; set; } = [];
    }
}
