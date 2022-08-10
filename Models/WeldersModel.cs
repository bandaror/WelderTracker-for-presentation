using System.ComponentModel.DataAnnotations;

namespace WelderTracker150722.Models
{
    public class WeldersModel
    {
        [Key]
        public int Id { get; set; }
        public int? WelderId { get; set; }
        public string WelderName { get; set; }
        public ICollection<OrderItemsModel>? Items { get; set; }
    }
}
