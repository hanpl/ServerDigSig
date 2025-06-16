namespace DigitalSignageSevice.Models
{
    public class AHT_GateInformation
    {
        public string Id { get; set; }
        public string Adi { get; set; }
        public string? LineCode { get; set; }
        public string? Number { get; set; }
        public string? ScheduledDate { get; set; }
        public DateTime? Schedule { get; set; }
        public DateTime? Estimated {  get; set; }
        public DateTime? Actual { get; set; }
        public string? Status { get; set; }
        public string? Gate { get; set; }
        public string? Remark { get; set; }
        public string? Mcdt { get; set; }
    }
}
