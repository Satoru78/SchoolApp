using Microsoft.Win32;
using SchoolApp.Context;
using SchoolApp.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;

namespace SchoolApp.Views.Windows
{
	/// <summary>
	/// Логика взаимодействия для ActionWindow.xaml
	/// </summary>
	public partial class ActionWindow : Window
	{
		private OpenFileDialog openFile;
		private Service _service;
		public ActionWindow(Service service)
		{
			InitializeComponent();
			_service = service;
			DataContext = _service;
			if (service.ServicePhoto.Any())
			{
				servicePhotos = _service.ServicePhoto.ToList();
				_service.ServicePhoto.ToList().ForEach(x =>
				{
					var path = $"{System.IO.Path.Combine(Environment.CurrentDirectory, "..\\.\\")}{x.PhotoPath}";
					x.PhotoPathControl = path;
					x.IsDataBase = true;
					ServicePhotoPath.Items.Add(x);
				});
				
			}
			openFile = new OpenFileDialog
			{
				Filter = "Jpeg files(*.jpeg)|8.jpeg|all files(*.*)|*.*",
			};

		}

		private void BtnDeleteMainImage_Click(object sender, RoutedEventArgs e)
		{
			MainImage.Source = null;
		}
		private string saveFileName;
		private string fileName;
		private void BtnAddImage_Click(object sender, RoutedEventArgs e)
		{
			if (openFile.ShowDialog() == true)
			{
				saveFileName = openFile.SafeFileName;
				fileName = openFile.FileName;
				MainImage.Source = new BitmapImage(new Uri(fileName));
			}
		}

		private void BtnDeleteAddImage_Click(object sender, RoutedEventArgs e)
		{
			var selectedServicePhoto = ServicePhotoPath.SelectedItem as ServicePhoto;
			if (selectedServicePhoto != null)
			{
				selectedServicePhoto.IsDeleted = true;
				ServicePhotoPath.Items.Remove(selectedServicePhoto);
			}
		}
		private List<ServicePhoto> servicePhotos = new List<ServicePhoto>();
		private void btnAddAdditionalImage_Click(object sender, RoutedEventArgs e)
		{
			if (openFile.ShowDialog() == true)
			{
				var fileName = openFile.FileName;
				var saveFileName = openFile.SafeFileName;
				var photoPath = $"Услуги школы/{saveFileName}";
				var servicePhoto = new ServicePhoto() { ServiceID = _service.ID, PhotoPath = photoPath, PhotoPathControl = fileName, Service = _service, IsDataBase = false };
				servicePhotos.Add(servicePhoto);
				ServicePhotoPath.Items.Add(servicePhoto);
				
			}
		}

		private void SaveFiles()
		{
			if (string.IsNullOrEmpty(saveFileName) && string.IsNullOrEmpty(fileName))
			{
				if (_service.MainImagePath == null)
				{
					_service.MainImagePath = null;
				}
			}
			else
			{
				CopyFile(fileName, saveFileName);
			}
			foreach (var servicePhoto in servicePhotos)
			{
				CopyFile(servicePhoto.PhotoPathControl, servicePhoto.PhotoPath.Replace("Услуги школы\\", ""));
			}
		}

		private void btnSubmit_Click(object sender, RoutedEventArgs e)
		{
			if (_service.ID == 0)
			{
				SetMainImage();
				SetAddionalImages();
			}
			else
			{
				SetMainImage();
				SetAddionalImages();
			}
			DataApp.db.Service.AddOrUpdate(_service);
			DataApp.db.SaveChanges();
			SaveFiles();
		}
		private void SetAddionalImages()
		{
			foreach (var servicePhoto in servicePhotos.ToArray())
			{
				if (servicePhoto.IsDataBase && servicePhoto.IsDeleted)
				{
					servicePhotos.Remove(servicePhoto);
					DataApp.db.ServicePhoto.Remove(servicePhoto);
				}
				if (!servicePhoto.IsDeleted && !servicePhoto.IsDataBase)
				{
					DataApp.db.ServicePhoto.Add(servicePhoto);
				}
			}
		}
		private void SetMainImage()
		{
			if (!string.IsNullOrWhiteSpace(saveFileName) && !string.IsNullOrWhiteSpace(fileName))
			{
				var photoPath = $"Услуги школы\\{saveFileName}";
				_service.MainImagePath = photoPath;
			}
		}
		private void CopyFile(string sourceFileName, string destFileName)
		{
			try
			{
			File.Copy(sourceFileName, $"{System.IO.Path.Combine(Environment.CurrentDirectory)}Услуги школы\\{destFileName}");

			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
