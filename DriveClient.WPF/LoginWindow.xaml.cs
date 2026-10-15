using System.Linq;
using System.Windows;
using DriveClientWPF;

namespace DriveClient.WPF
{
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();
			using var db = new AppDbContext();
			db.Database.EnsureCreated();
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			using var db = new AppDbContext();
			var user = db.Users.FirstOrDefault(u => u.Username == txtUsername.Text && u.Password == txtPassword.Password);

			if (user != null)
			{
				Session.CurrentUser = user;
				var mainWindow = new MainWindow();
				mainWindow.Show();
				this.Close();
			}
			else
			{
				MessageBox.Show("Невірний логін або пароль!");
			}
		}

		private void OpenRegister_Click(object sender, RoutedEventArgs e)
		{
			var registerWindow = new RegisterWindow();
			registerWindow.Owner = this;
			registerWindow.ShowDialog();
		}
	}
}