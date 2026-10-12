using System;
using System.Collections.Generic;
using System.Linq;
using DriveClient.WPF.Models;
using DriveClient.WPF.Services;
using Xunit;

namespace DriveClient.Tests
{
	public class FileManagerTests
	{
		private readonly FileManager _fileManager = new FileManager();

		[Fact]
		public void FilterAndSort_ReturnsOnlyPngSortedByModificationDateDescending()
		{
			var files = new List<FileItem>
			{
				new FileItem { Name = "main.cpp", Extension = ".cpp", ModificationDate = new DateTime(2026, 1, 1) },
				new FileItem { Name = "logo.png", Extension = ".png", ModificationDate = new DateTime(2026, 1, 3) },
				new FileItem { Name = "bg.png", Extension = ".png", ModificationDate = new DateTime(2026, 1, 2) }
			};

			var result = _fileManager.FilterAndSort(files, ".png", ascending: false).ToList();

			Assert.Equal(2, result.Count);
			Assert.Equal("logo.png", result[0].Name);
			Assert.Equal("bg.png", result[1].Name);
		}

		[Fact]
		public void GetFilesToUpload_IdentifiesMissingRemoteFiles()
		{
			var local = new List<FileItem> { new FileItem { Id = "1" }, new FileItem { Id = "2" } };
			var remote = new List<FileItem> { new FileItem { Id = "1" } };

			var result = _fileManager.GetFilesToUpload(local, remote).ToList();

			Assert.Single(result);
			Assert.Equal("2", result[0].Id);
		}

		[Fact]
		public void FileItem_FormattedSize_ReturnsCorrectKBString()
		{
			var file = new FileItem { Size = 2048 };
			Assert.Equal("2.00 KB", file.FormattedSize);
		}
	}
}