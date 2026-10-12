using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using DriveClient.WPF.Models;
using DriveClient.WPF.Services;

namespace DriveClient.WPF
{
	public partial class MainWindow : Window
	{
		private readonly FileManager _fileManager = new FileManager();
		private List<FileItem> _mockFiles = new List<FileItem>();

		public MainWindow()
		{
			InitializeComponent();
			LoadMockData();
			ApplyFilters();
		}

		private void LoadMockData()
		{
			_mockFiles = new List<FileItem>
			{
				new FileItem { Id = "1", Name = "main.cpp", Extension = ".cpp", Size = 4096, ModificationDate = DateTime.Now.AddDays(-2), UploadedBy = "Студент" },
				new FileItem { Id = "2", Name = "diagram.png", Extension = ".png", Size = 153600, ModificationDate = DateTime.Now, UploadedBy = "Викладач" },
				new FileItem { Id = "3", Name = "utils.cpp", Extension = ".cpp", Size = 2048, ModificationDate = DateTime.Now.AddDays(-5), UploadedBy = "Студент" },
				new FileItem { Id = "4", Name = "readme.txt", Extension = ".txt", Size = 512, ModificationDate = DateTime.Now.AddDays(-1), UploadedBy = "Студент" }
			};
		}

		private void FilterOrSortChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded) return;
			ApplyFilters();
		}

		private void ApplyFilters()
		{
			var selectedFilterItem = (ComboBoxItem)FilterCombo.SelectedItem;
			string filterExtension = selectedFilterItem?.Tag?.ToString() ?? "";

			bool ascending = SortCombo.SelectedIndex == 0;

			var result = _fileManager.FilterAndSort(_mockFiles, filterExtension, ascending);

			FilesGrid.ItemsSource = result.ToList();
		}

		private void UploadButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Тут буде відкриватися діалог вибору файлу для відправки на Django-сервер.", "Завантаження");
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				MessageBox.Show($"Файл готовий до завантаження: {Path.GetFileName(files[0])}", "Drag-n-Drop");
			}
		}

		private void FilesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedFile = FilesGrid.SelectedItem as FileItem;
			if (selectedFile == null) return;

			PreviewImage.Visibility = Visibility.Collapsed;
			PreviewText.Visibility = Visibility.Collapsed;

			if (selectedFile.Extension == ".cpp")
			{
				PreviewText.Visibility = Visibility.Visible;
				PreviewText.Text = "// Симуляція вмісту файлу .cpp\n#include <iostream>\n\nint main() {\n    std::cout << \"Hello World\";\n    return 0;\n}";
			}
			else if (selectedFile.Extension == ".png")
			{
				PreviewText.Visibility = Visibility.Visible;
				PreviewText.Text = "[Тут відображатиметься зображення PNG]";
			}
		}
	}
}
