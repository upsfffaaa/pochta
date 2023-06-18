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
using System.Windows.Navigation;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

		string server = ConfigurationManager.AppSettings["gmail_server"];
		int port = int.Parse(ConfigurationManager.AppSettings["gmail_port"]);

		private string username;
		private string password;

		public int messagesPerPage = 5;

		public MainWindow(string _username, string _password)
		{
			InitializeComponent();

			username = _username;
			password = _password;
			RetriveMessages();
			textbox_messages_per_page.Text = messagesPerPage.ToString();
		}

		private async void OpenMessage(object sender, MouseButtonEventArgs e)
		{
			using (var client = new ImapClient())
			{
				await client.ConnectAsync(server, port, SecureSocketOptions.SslOnConnect);

				try
				{
					await client.AuthenticateAsync(username, password);

					var selectedEmail = (EmailListData)((ListBox)sender).SelectedItem;
					var folderName = selectedEmail.Folder;

					var folder = client.GetFolder(folderName);

					await folder.OpenAsync(FolderAccess.ReadOnly);

					var message = await folder.GetMessageAsync(selectedEmail.Id);
					var messageTheme = message.Subject;
					var messageSender = message.Sender.Name;

					string messageBody = string.Empty;

					if (message.HtmlBody != null)
					{
						messageBody = message.HtmlBody;
					}
					else if (message.TextBody != null)
					{
						messageBody = message.TextBody;
					}

					OpenMessageWindow openMessageWindow = new OpenMessageWindow(messageSender, messageTheme, messageBody);
					openMessageWindow.Show();
				}
				catch (AuthenticationException ex)
				{
					MessageBox.Show($"{ex.Message}", "Authentication error");
				}
				finally
				{
					await client.DisconnectAsync(true);
				}
			}
		}


		private void menuitem_new_message_Click(object sender, RoutedEventArgs e)
		{
			NewMessageWindow newMessageWindow = new NewMessageWindow(username, password);
			newMessageWindow.Show();
		}

		private void menuitem_logout_Click(object sender, RoutedEventArgs e)
		{
			LoginWindow loginWindow = new LoginWindow();
			loginWindow.Show();
			this.Close();
		}

		private void menuitem_exit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void menuitem_refresh_Click(object sender, RoutedEventArgs e)
		{
			RetriveMessages();
		}

		private async void RetriveAllMessages()
		{
			using (var client = new ImapClient())
			{
				await client.ConnectAsync(server, port, SecureSocketOptions.SslOnConnect);
				try
				{
					await client.AuthenticateAsync(username, password);
					var folder = client.GetFolder(SpecialFolder.All);
					await folder.OpenAsync(FolderAccess.ReadOnly);
					var uids = folder.Search(SearchQuery.All);
					int messagesCount = uids.Count - 1;
					List<EmailListData> emailist = new List<EmailListData>();
					for (int i = messagesCount; i > messagesCount - messagesPerPage; i--)
					{
						emailist.Add(new EmailListData { Id = i, Subject = folder.GetMessage(i).Subject, Folder = folder.FullName });
					}
					listbox_all_messages.ItemsSource = emailist.AsEnumerable();
				}
				catch (AuthenticationException ex)
				{
					MessageBox.Show($"{ex.Message}", "Authentication error");
				}
				finally
				{
					await client.DisconnectAsync(true);
				}
			}
		}
		private async void RetriveSendedMessages()
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(server, port, SecureSocketOptions.SslOnConnect);
                try
                {
                    await client.AuthenticateAsync(username, password);
                    var folder = client.GetFolder(SpecialFolder.Sent);
                    await folder.OpenAsync(FolderAccess.ReadOnly);
                    var uids = folder.Search(SearchQuery.All);
                    int messagesCount = uids.Count - 1;
                    List<EmailListData> emailist = new List<EmailListData>();
                    for (int i = messagesCount; i > messagesCount - messagesPerPage; i--)
                    {
                        emailist.Add(new EmailListData { Id = i, Subject = folder.GetMessage(i).Subject, Folder = folder.FullName });
                    }
                    listbox_sended_messages.ItemsSource = emailist.AsEnumerable();
                }
                catch (AuthenticationException ex)
                {
                    MessageBox.Show($"{ex.Message}", "Authentication error");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }



        private void RetriveMessages()
        {

            RetriveAllMessages();
            RetriveSendedMessages();
            textbox_last_update_time.Text = $"{DateTime.Now}";

        }

		private async void listbox_all_messages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (listbox_all_messages.SelectedItem != null)
			{
				var email = (EmailListData)listbox_all_messages.SelectedItem;
				using (var client = new ImapClient())
				{
					await client.ConnectAsync(server, port, SecureSocketOptions.SslOnConnect);
					try
					{
						await client.AuthenticateAsync(username, password);
						var folder = client.GetFolder(email.Folder);
						await folder.OpenAsync(FolderAccess.ReadOnly);
						var message = await folder.GetMessageAsync(email.Id);
						// Removed MessageWindow code
					}
					catch (AuthenticationException ex)
					{
						MessageBox.Show($"{ex.Message}", "Authentication error");
					}
					finally
					{
						await client.DisconnectAsync(true);
					}
				}
			}
		}
	}

    /// <summary>
    /// Class for storing email lists data
    /// </summary>
    public class EmailListData
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Folder { get; set; }
    }
}
