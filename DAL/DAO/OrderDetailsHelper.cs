namespace CaseStudy.DAL.DAO
{
    public class OrderDetailsHelper
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal Cost { get; set; }
        public string Name { get; set; }
        public string ProductId { get; set; }
        public int QtyB { get; set; }
        public int QtyO { get; set; }
        public int QtyS { get; set; }
        public string DateCreated { get; set; }
    }
}