using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SchoolApp.Model
{
	public partial class Service
	{
		public string AbsolutePhotoPath => $"{Environment.CurrentDirectory}\\{MainImagePath.TrimStart()}";
		public int DurationInMunites
		{
			get
			{

				return DurationInSeconds / 60;
			}
			set
			{
				DurationInSeconds = value * 60;
			}

		}

		public decimal CostWithDiscount
		{
			get
			{
				if (Discount.HasValue)
				{
					var discountPrice = Cost * (decimal)Discount.Value / 100;
					return Cost - discountPrice;
				}
				return Cost;
			}
		}
		public string DescriptionN
		{
			get
			{
				if (Description is null)
				{
					return "";
				}
				return Description;
			}
		}
		public string GetPhoto
		{
			get
			{
				return Environment.CurrentDirectory + "\\" + MainImagePath;
			}
		}
	}
	public partial class ServicePhoto
	{
		public bool IsDataBase { get; set; }
		public bool IsDeleted { get; set; }
		public string PhotoPathControl { get; set; }
	}
}
