using System.ComponentModel.DataAnnotations;
namespace WelderTracker150722.Models
{
    public class CompletedItemModel
    {




        [Key]
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Amount { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateofCompletition { get; set; }
        public int WelderId { get; set; }
       // public ICollection<OrderItemsModel>? ComletedItems { get; set; } = null;
    }
}



