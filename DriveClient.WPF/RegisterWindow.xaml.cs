using System.Linq;
using System.Windows;
using DriveClientWPF;

namespace DriveClient.WPF
{
	public partial class RegisterWindow : Window
	{
		public RegisterWindow()
		{
			InitializeComponent();
		}

		private void RegisterButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
			{
				MessageBox.Show("Заповніть усі поля!");
				return;
			}

			using var db = new AppDbContext();
			if (db.Users.Any(u => u.Username == txtUsername.Text))
			{
				MessageBox.Show("Користувач із таким логіном вже існує!");
				return;
			}

			var newUser = new User
			{
				Username = txtUsername.Text,
				Password = txtPassword.Password
			};
			db.Users.Add(newUser);
			db.SaveChanges();

			MessageBox.Show("Реєстрацію успішно виконано! Тепер ви можете увійти.");
			this.Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}