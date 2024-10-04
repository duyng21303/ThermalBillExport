namespace ThermalBillExport.ViewModels
{
    public class BillVM
    {
		public string BillNumber { get; set; }
		public DateTime Date { get; set; }
		public List<ProductVM> Products { get; set; }
		public decimal Subtotal => Products.Sum(p => p.Quantity * p.Price);
		public decimal Discount { get; set; }
		public decimal ConvenienceFee { get; set; }
		public decimal AmountDue => Subtotal + ConvenienceFee;
	}
}
