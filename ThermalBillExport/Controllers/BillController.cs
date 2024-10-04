using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Collections.Generic; 
using System.IO;
using System.Threading.Tasks;
using ThermalBillExport.ViewModels;


public class BillController : Controller
{
	private readonly ICompositeViewEngine _viewEngine;
	private readonly IConverter _converter;

	public BillController(ICompositeViewEngine viewEngine, IConverter converter)
	{
		_viewEngine = viewEngine;
		_converter = converter;
	}
	[HttpPost]
	public async Task<IActionResult> PrintBill([FromForm] BillVM model, string products)
	{
		if (model == null)
		{
			return BadRequest("Dữ liệu hóa đơn không hợp lệ.");
		}

		try
		{
			// Nạp thư viện libwkhtmltox.dll
			var context = new CustomAssemblyLoadContext();
			var libPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lib", "libwkhtmltox.dll");

			if (!System.IO.File.Exists(libPath))
			{
				return StatusCode(500, "Không tìm thấy thư viện tạo PDF cần thiết.");
			}

			context.LoadUnmanagedLibrary(libPath);

			if (!string.IsNullOrEmpty(products))
			{
				model.Products = JsonConvert.DeserializeObject<List<ProductVM>>(products);
			}
			else
			{
				return BadRequest("Dữ liệu sản phẩm bị thiếu.");
			}

			var html = await RenderViewToStringAsync("_BillPartial", model);
			if (!string.IsNullOrEmpty(html))
			{
				html = html.Replace("<button id=\"printBillBtn\"", "<!--button id=\"printBillBtn\"");
				html = html.Replace("</button>", "</button-->");
			}
			if (string.IsNullOrWhiteSpace(html))
			{
				return StatusCode(500, "Nội dung HTML để tạo PDF bị rỗng.");
			}

			// Tạo PDF từ nội dung HTML
			var pdf = new HtmlToPdfDocument()
			{
				GlobalSettings = {
				ColorMode = ColorMode.Color,
				Orientation = Orientation.Portrait,
				PaperSize = new PechkinPaperSize("90mm", "150mm"),  
			},
				Objects = {
				new ObjectSettings()
				{
					HtmlContent = html,
					WebSettings = { DefaultEncoding = "utf-8", LoadImages = true },
				}
			}
			};

			var pdfBytes = _converter.Convert(pdf);

			// Trả về file PDF trực tiếp
			return File(pdfBytes, "application/pdf", $"hoa_don_{model.BillNumber}.pdf");
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("Lỗi khi tạo PDF: " + ex.Message);
			return StatusCode(500, "Đã có lỗi xảy ra trong quá trình tạo hóa đơn.");
		}
	}



	private async Task<string> RenderViewToStringAsync(string viewName, object model)
	{
		ViewData.Model = model;

		using (var writer = new StringWriter())
		{
			var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

			if (viewResult.View == null)
			{
				throw new FileNotFoundException($"View {viewName} không tồn tại.");
			}

			var viewContext = new ViewContext(
				ControllerContext,
				viewResult.View,
				ViewData,
				TempData,
				writer,
				new HtmlHelperOptions()
			);

			await viewResult.View.RenderAsync(viewContext);
			return writer.ToString();
		}
	}

	[HttpPost]
	public IActionResult Generate([FromBody] BillVM model)
	{
		if (string.IsNullOrEmpty(model.BillNumber))
		{
			var random = new Random();
			model.BillNumber = random.Next(100000000, 999999999).ToString();
		}

		model.Date = DateTime.Now;
		return PartialView("_BillPartial", model);
	}
}
