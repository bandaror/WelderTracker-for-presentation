using System.ComponentModel.DataAnnotations;

namespace WelderTracker150722.Models
{
    public class OrderItemsModel
    {
        [Key]
        public int Id { get; set; }
        public string FirNumber { get; set; }
        public string ItemName { get; set; }
        public int Amount { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ToCompleteBy { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Awaiting;
        public int CompletedAmount { get; set; } = 0;
        public int? WeldersId { get; set; }
        public ICollection<WeldersModel>? Welders { get; set; }


    }
}
namespace WelderTracker150722
{
    public enum OrderStatus
    {
        Awaiting,
        Started,
        Completed
    }
}
