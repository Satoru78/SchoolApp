using SchoolApp.Context;
using SchoolApp.Model;
using SchoolApp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using  Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

using Microsoft.Win32;

namespace SchoolApp.Views.Pages
{
	/// <summary>
	/// Логика взаимодействия для ServiceList.xaml
	/// </summary>
	public partial class ServiceList : Page
	{
		public List<string> sortList = new List<string>()
		{
			"По возрастанию", "По убыванию"
		};
		public List<string> filterList = new List<string>()
		{
			"Все", "от 0 до 5", "от 5 до 15","от 15 до 30","от 30 до 70","от 70 до 100",
		};
		public List<Service> Services { get; set; }
		public Service Service { get; set; }
		public ServiceList(Service service)
		{
			InitializeComponent();
			Service = service;
			this.DataContext = this;
			LoadData();
		}

		private void LoadData()
		{
			var services = DataApp.db.Service.ToList();

			switch (cmdSort.SelectedIndex)
			{
				case 0:
					services = services.OrderBy(x => x.Discount == null ? x.Cost : x.CostWithDiscount).ToList();
					break;
				case 1:
					services = services.OrderByDescending(x => x.Discount == null ? x.Cost : x.CostWithDiscount).ToList();
					break;
			}

			switch (cmdFilter.SelectedIndex)
			{
				case 1:
					services = services.Where(x => x.Discount >= 0 && x.Discount < 5).ToList();
					break;
				case 2:
					services = services.Where(x => x.Discount >= 10 && x.Discount < 15).ToList();
					break;
				case 3:
					services = services.Where(x => x.Discount >= 15 && x.Discount < 30).ToList();
					break;
				case 4:
					services = services.Where(x => x.Discount >= 30 && x.Discount < 70).ToList();
					break;
				case 5:
					services = services.Where(x => x.Discount >= 70 && x.Discount < 100).ToList();
					break;
			}

			services = services.Where(x => x.Title.Contains(txbSearch.Text)).ToList();
			ServiceDataList.ItemsSource = services;
			tlbCount.Text = $"{services.Count} из {DataApp.db.Service.Count()}";
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Services = DataApp.db.Service.ToList();
			ServiceDataList.ItemsSource = Services;
			cmdSort.ItemsSource = sortList;
			cmdFilter.ItemsSource = filterList;
		}
		private void cmdFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LoadData();
		}

		private void cmdSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LoadData();
		}

		private void txbSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			LoadData();
		}

		private void btnEdit_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnDel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
		    	var service = (sender as Button).DataContext as Service;
				if (service != null)
				{
					DataApp.db.Service.Remove(service);
					DataApp.db.SaveChanges();
					MessageBox.Show("Данные успешно удалены", "Ынимаиие", MessageBoxButton.OK, MessageBoxImage.Information);
					Page_Loaded(null, null);
				}			
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);			
			}
		}

		private void btnADDEdit_Click(object sender, RoutedEventArgs e)
		{
			ActionWindow action = new ActionWindow(new Service());
			action.ShowDialog();
		}

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {

			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = "report";
			saveFileDialog.Filter = "Excel files (*.xlsx)|*xlsx|Word files (*.doc, *.docx)|*.doc,*.docx|PDF Files (*.pdf)|*.pdf";
			saveFileDialog.FilterIndex = 0;
            if (saveFileDialog.ShowDialog() == true)
            {
                switch (saveFileDialog.FilterIndex)
                {
                    case 0 :
						CreateExcelReport(saveFileDialog.FileName);
                        break;
					case 1:
						MessageBox.Show(saveFileDialog.FileName);
						break;
					case 2:
						CreateWordReport(saveFileDialog.FileName);
						break;
					case 3:
						CreatePdfFormat(saveFileDialog.FileName);
						break;
                }
            }
        }

        private void CreatePdfFormat(string savePath)
        {
			Word.Application app = new Word.Application();
			Word.Document document = app.Documents.Add();
			Word.Paragraph paragraph = document.Paragraphs.Add();
			Word.Range range = paragraph.Range;
			var services = DataApp.db.Service.ToList();
			Word.Table table = document.Tables.Add(range, services.Count + 1, 4);
			Word.Range cellRange;
			cellRange = table.Cell(1, 1).Range;
			cellRange.Text = "Наименование";

			cellRange = table.Cell(1, 2).Range;
			cellRange.Text = "Актуальная цена";

			cellRange = table.Cell(1, 3).Range;
			cellRange.Text = "Продолжительность в минутах";

			cellRange = table.Cell(1, 4).Range;
			cellRange.Text = "Изображение";

			for (int i = 0; i < services.Count; i++)
			{
				if (services[i] != null)
				{
					cellRange = table.Cell(i + 2, 1).Range;
					cellRange.Text = services[i].Title;

					cellRange = table.Cell(i + 2, 2).Range;
					cellRange.Text = services[i].CostWithDiscount.ToString();

					cellRange = table.Cell(i + 2, 3).Range;
					cellRange.Text = services[i].DurationInMunites.ToString();

					cellRange = table.Cell(i + 2, 4).Range;
					Word.InlineShape rangePicture = cellRange.InlineShapes.AddPicture(services[i].AbsolutePhotoPath,
						Type.Missing,
						Type.Missing,
						Type.Missing);
					rangePicture.Height = 80;
					rangePicture.Width = 80;

				}
			}
			document.SaveAs(savePath , Word.WdSaveFormat.wdFormatPDF);
	
			app.Quit();
		}

        private void CreateWordReport(string savePath)
        {
			Word.Application app = new Word.Application();
			Word.Document document = app.Documents.Add();
			Word.Paragraph paragraph = document.Paragraphs.Add();
			Word.Range range = paragraph.Range;
			var services = DataApp.db.Service.ToList();
			Word.Table table = document.Tables.Add(range, services.Count + 1, 4);
			Word.Range cellRange;
			cellRange = table.Cell(1, 1).Range;
			cellRange.Text = "Наименование";

			cellRange = table.Cell(1, 2).Range;
			cellRange.Text = "Актуальная цена";

			cellRange = table.Cell(1, 3).Range;
			cellRange.Text = "Продолжительность в минутах";

			cellRange = table.Cell(1, 4).Range;
			cellRange.Text = "Изображение";

			for (int i = 0; i < services.Count; i++)
            {
				if (services[i] != null)
                {
					cellRange = table.Cell(i + 2, 1).Range;
					cellRange.Text = services[i].Title;

					cellRange = table.Cell(i + 2, 2).Range;
					cellRange.Text = services[i].CostWithDiscount.ToString();

					cellRange = table.Cell(i + 2, 3).Range;
					cellRange.Text = services[i].DurationInMunites.ToString();

					cellRange = table.Cell(i + 2, 4).Range;
					Word.InlineShape rangePicture = cellRange.InlineShapes.AddPicture(services[i].AbsolutePhotoPath,
						Type.Missing,
						Type.Missing,
						Type.Missing);
					rangePicture.Height = 80;
					rangePicture.Width = 80;

				}
			}
			document.SaveAs(savePath);
			document.Close();
			app.Quit();
		}

		private void CreateExcelReport(string savePath)
        {
			Excel.Application app = new Excel.Application();
			Excel.Workbook wb = app.Workbooks.Add();
			Excel.Worksheet ws = wb.Worksheets.Add();
			ws.Name = "Услуги";
			ws.Range["A1"].Value = "Наименование";
			ws.Range["B1"].Value = "Актуальная цена";
			ws.Range["C1"].Value = "Продолжительность в минутах";

			var services = DataApp.db.Service.ToList();
            for (int i = 0; i < services.Count; i++)
            {
                if (services[i] != null)
                {
					ws.Range[$"A{i + 2}"].Value = services[i].Title;
					ws.Range[$"B{i + 2}"].Value = services[i].CostWithDiscount;
					ws.Range[$"C{i + 2}"].Value = services[i].DurationInMunites;



                    Excel.Range imgRange = ws.Cells[i + 2, 4];
                    ws.Paste(imgRange, services[i].AbsolutePhotoPath);

					/// Переустановить SahrePoint
					//var left = imgRange.Left;
					//var top = imgRange.Top;
					//var imgSize = 50;
					//ws.Shapes.AddPicture(services[i].AbsolutePhotoPath, MsoTriState )


                }
			}
			wb.SaveAs(savePath);
			wb.Close();
			app.Quit();
		}
	}
}
