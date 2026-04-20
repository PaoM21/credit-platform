namespace BatchProcessor.Functions.Models
{
 public class BatchRange
 {
 public string BatchId { get; set; } = default!;
 public long StartId { get; set; }
 public long EndId { get; set; }
 }
}