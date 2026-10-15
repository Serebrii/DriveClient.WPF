using System;
using System.ComponentModel.DataAnnotations;

namespace DriveClientWPF
{
	public class User
	{
		public int Id { get; set; }

		[Required]
		public string Username { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
	}

	public class FileItem
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Extension { get; set; } = string.Empty;
		public long Size { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

		public DateTime CreatedAtLocal => CreatedAt.ToLocalTime();
		public DateTime UpdatedAtLocal => ModificationDate.ToLocalTime();

		public string UploadedBy { get; set; } = string.Empty;
		public string EditedBy { get; set; } = string.Empty;
		public string StoragePath { get; set; } = string.Empty;

		public int UserId { get; set; }
		public User? User { get; set; }
	}
}