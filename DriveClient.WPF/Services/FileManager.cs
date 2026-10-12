using System;
using System.Collections.Generic;
using System.Linq;
using DriveClient.WPF.Models;

namespace DriveClient.WPF.Services
{
	public class FileManager
	{
		// Фільтрація (.cpp, .png або всі) та сортування за датою модифікації
		public IEnumerable<FileItem> FilterAndSort(
			IEnumerable<FileItem> files,
			string filterExtension, // "", ".cpp" або ".png"
			bool ascending)
		{
			var filtered = string.IsNullOrEmpty(filterExtension)
				? files
				: files.Where(f => f.Extension?.Equals(filterExtension, StringComparison.OrdinalIgnoreCase) == true);

			return ascending
				? filtered.OrderBy(f => f.ModificationDate)
				: filtered.OrderByDescending(f => f.ModificationDate);
		}
		public IEnumerable<FileItem> GetFilesToUpload(IEnumerable<FileItem> localFiles, IEnumerable<FileItem> remoteFiles)
		{
			var remoteIds = new HashSet<string?>(remoteFiles.Select(r => r.Id));

			return localFiles.Where(l => l.Id != null && !remoteIds.Contains(l.Id));
		}
	}
}