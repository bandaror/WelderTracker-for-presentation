using System.ComponentModel.DataAnnotations;
namespace WelderTracker150722.Models
{
    public class ExcelModel
    {
        [Key]
        public int Id { get; set; }
        
        public string FirNumber { get; set; }
        public string ItemName { get; set; }
        public int Amount { get; set; }
        public string ToCompleteBy { get; set; }
    }
}
