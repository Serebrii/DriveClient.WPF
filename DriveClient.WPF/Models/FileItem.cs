using System;
using System.Globalization;

namespace DriveClient.WPF.Models
{
	public class FileItem
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
		public string? Extension { get; set; }
		public long Size { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime ModificationDate { get; set; }
		public string? UploadedBy { get; set; }
		public string? EditedBy { get; set; }

		public string FormattedSize => (Size / 1024.0).ToString("F2", CultureInfo.InvariantCulture) + " KB";
	}
}