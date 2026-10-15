using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DriveClient.WPF;
using Microsoft.Win32;
using System.Diagnostics;

namespace DriveClientWPF
{
	public partial class MainWindow : Window
	{
		private readonly string _remoteStorageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RemoteDriveStorage");

		public MainWindow()
		{
			InitializeComponent(); 

			if (!Directory.Exists(_remoteStorageDir))
				Directory.CreateDirectory(_remoteStorageDir);

			if (Session.CurrentUser != null && !string.IsNullOrEmpty(Session.CurrentUser.Username))
			{
				txtUserInfo.Text = $"Кабінет: {Session.CurrentUser.Username}";
			}

			LoadFiles(); 
		}

		private void LoadFiles()
		{
			if (FilesDataGrid == null || Session.CurrentUser == null || Session.CurrentUser.Id == 0)
				return;

			using var db = new AppDbContext();

			var query = db.FileItems.Where(f => f.UserId == Session.CurrentUser.Id).AsQueryable();

			if (cmbFilter != null && cmbFilter.SelectedItem is ComboBoxItem selectedFilter)
			{
				string filterValue = selectedFilter.Content?.ToString() ?? "";
				if (filterValue == ".cpp")
				{
					query = query.Where(f => f.Extension.ToLower() == ".cpp");
				}
				else if (filterValue == ".png")
				{
					query = query.Where(f => f.Extension.ToLower() == ".png");
				}
			}

			if (cmbSort != null && cmbSort.SelectedItem is ComboBoxItem selectedSort)
			{
				string sortValue = selectedSort.Content?.ToString() ?? "";
				if (sortValue.Contains("зростання"))
				{
					query = query.OrderBy(f => f.UpdatedAt);
				}
				else
				{
					query = query.OrderByDescending(f => f.UpdatedAt);
				}
			}
			else
			{
				query = query.OrderByDescending(f => f.UpdatedAt);
			}

			FilesDataGrid.ItemsSource = query.ToList();
		}

		private void FilterOrSort_Changed(object sender, SelectionChangedEventArgs e)
		{
			LoadFiles();
		}

		private void ToggleColumn_Click(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox cb)
			{
				var column = FilesDataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == cb.Content?.ToString());
				if (column != null)
				{
					column.Visibility = cb.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
				}
			}
		}


		private void Download_Click(object sender, RoutedEventArgs e)
		{
			if (FilesDataGrid.SelectedItem is FileItem selected)
			{
				var saveFileDialog = new SaveFileDialog
				{
					FileName = selected.Name,
					DefaultExt = selected.Extension,
					Filter = $"Файл (*{selected.Extension})|*{selected.Extension}|Всі файли (*.*)|*.*"
				};

				if (saveFileDialog.ShowDialog() == true)
				{
					if (File.Exists(selected.StoragePath))
					{
						File.Copy(selected.StoragePath, saveFileDialog.FileName, true);
						MessageBox.Show("Файл успішно вивантажено!");
					}
					else
					{
						MessageBox.Show("Помилка: фізичний файл відсутній у сховищі.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			if (FilesDataGrid.SelectedItem is FileItem selected)
			{
				var result = MessageBox.Show($"Ви дійсно бажаєте видалити файл '{selected.Name}'?", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Question);

				if (result == MessageBoxResult.Yes)
				{
					try
					{
						if (File.Exists(selected.StoragePath))
						{
							File.Delete(selected.StoragePath);
						}

						using var db = new AppDbContext();
						var itemToDelete = db.FileItems.FirstOrDefault(f => f.Id == selected.Id);
						if (itemToDelete != null)
						{
							db.FileItems.Remove(itemToDelete);
							db.SaveChanges();
						}

						LoadFiles();
						MessageBox.Show("Файл успішно видалено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Помилка при видаленні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
		}

		private void Sync_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFolderDialog();
			if (dialog.ShowDialog() == true)
			{
				string folder = dialog.FolderName;
				using var db = new AppDbContext();
				var userFiles = db.FileItems.Where(f => f.UserId == Session.CurrentUser.Id).ToList();

				var localFiles = Directory.GetFiles(folder);
				var localNames = localFiles.Select(Path.GetFileName).Where(n => n != null).ToList();
				var dbNames = userFiles.Select(f => f.Name).ToList();

				foreach (var localPath in localFiles)
				{
					string? name = Path.GetFileName(localPath);
					if (name != null && !dbNames.Contains(name))
					{
						var fi = new FileInfo(localPath);
						string storagePath = Path.Combine(_remoteStorageDir, Guid.NewGuid() + fi.Extension);
						File.Copy(localPath, storagePath, true);

						db.FileItems.Add(new FileItem
						{
							Name = fi.Name,
							Extension = fi.Extension,
							Size = fi.Length,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow,
							UploadedBy = Session.CurrentUser.Username,
							EditedBy = Session.CurrentUser.Username,
							StoragePath = storagePath,
							UserId = Session.CurrentUser.Id,
							User = null
						});
					}
				}

				foreach (var dbFile in userFiles)
				{
					if (!localNames.Contains(dbFile.Name) && File.Exists(dbFile.StoragePath))
					{
						File.Copy(dbFile.StoragePath, Path.Combine(folder, dbFile.Name), true);
					}
				}

				db.SaveChanges();
				LoadFiles();
				MessageBox.Show("Синхронізацію успішно завершено!");
			}
		}

		private void Logout_Click(object sender, RoutedEventArgs e)
		{
			Session.CurrentUser = new User();

			var loginWindow = new LoginWindow();
			loginWindow.Show();

			this.Close();
		}
		
		private void ProcessAndUploadFiles(string[] filePaths)
		{
			try
			{
				using var db = new AppDbContext();
				int currentUserId = Session.CurrentUser.Id;

				foreach (var filePath in filePaths)
				{
					if (!File.Exists(filePath)) continue;

					var fi = new FileInfo(filePath);
					string storagePath = Path.Combine(_remoteStorageDir, Guid.NewGuid() + fi.Extension);
					File.Copy(filePath, storagePath, true);

					var nowUtc = DateTime.UtcNow;

					var newFile = new FileItem
					{
						Name = fi.Name,
						Extension = fi.Extension,
						Size = fi.Length,
						CreatedAt = nowUtc,
						UpdatedAt = nowUtc,
						ModificationDate = nowUtc,
						UploadedBy = Session.CurrentUser.Username,
						EditedBy = Session.CurrentUser.Username,
						StoragePath = storagePath,
						UserId = currentUserId,
						User = null
					};

					db.FileItems.Add(newFile);
				}

				db.SaveChanges();
				LoadFiles();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
			{
				string innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
				MessageBox.Show($"Помилка збереження в базі даних Neon DB:\n{innerMessage}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Виникла помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Upload_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog { Multiselect = true };
			if (openFileDialog.ShowDialog() == true)
			{
				ProcessAndUploadFiles(openFileDialog.FileNames);
			}
		}

		private void FilesDataGrid_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effects = DragDropEffects.Copy;
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}
			e.Handled = true;
		}

		private void FilesDataGrid_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (files != null && files.Length > 0)
				{
					ProcessAndUploadFiles(files);
				}
			}
		}

		private void ResetFilters_Click(object sender, RoutedEventArgs e)
		{
			if (cbiAllFiles != null)
			{
				cbiAllFiles.IsSelected = true;
			}

			if (cbiSortDesc != null)
			{
				cbiSortDesc.IsSelected = true;
			}

			LoadFiles();
		}

		private void FilesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (FilesDataGrid.SelectedItem is FileItem selected)
			{
				if (File.Exists(selected.StoragePath))
				{
					try
					{
						var processInfo = new ProcessStartInfo
						{
							FileName = selected.StoragePath,
							UseShellExecute = true 
						};
						Process.Start(processInfo);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Не вдалося відкрити файл: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
				else
				{
					MessageBox.Show("Файл відсутній у локальному сховищі!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

	}
}