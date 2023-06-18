using System;
using System.Configuration;
using System.Collections.Generic;
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
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

namespace GmailClient
{
	/// <summary>
	/// Логика взаимодействия для NewMessageWindow.xaml
	/// </summary>
	public partial class NewMessageWindow : Window
	{

		string smtpServer = ConfigurationManager.AppSettings["smtp_server"];
		int smtpPort = int.Parse(ConfigurationManager.AppSettings["smtp_port"]);

		private string email;
		private string password;

		public NewMessageWindow(string _email, string _password)
		{
			InitializeComponent();
			email = _email;
			password = _password;
		}

		private void button_Send_Click(object sender, RoutedEventArgs e)
		{
			MimeMessage message = new MimeMessage();
			message.From.Add(new MailboxAddress(txtboxFromWhom.Text, email));
			message.To.Add(MailboxAddress.Parse(txtboxToWhom.Text));
			message.Subject = txtboxTheme.Text == "" ? "No subject" : txtboxTheme.Text;
			message.Body = new TextPart("plain")
			{
				Text = txtboxBody.Text
			};
			message.Importance = MessageImportance.High;
			SmtpClient client = new SmtpClient();
			try
			{
				client.Connect(smtpServer, smtpPort, true);
				client.Authenticate(email, password);
				client.Send(message);
				MessageBox.Show("Email sent successfully!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				client.Disconnect(true);
				client.Dispose();
				this.Close();
			}
		}
	}
}
