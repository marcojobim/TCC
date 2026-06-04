namespace Tcc.Api.Models
{
    public record struct ProcessingResult
    {
        public double CheckTemp { get; set; }
        public long CheckDateTime { get; set; }
        public int CheckStatusWarning { get; set; }
        public int TotalItems { get; set; }
    }
}