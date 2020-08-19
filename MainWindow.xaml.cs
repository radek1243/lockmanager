using System.Windows;
using LockManager.Data;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace LockManager

{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private OracleConnection connection;
        private string query = "select v$session.USERNAME, l1.SID, v$session.SERIAL#, l1.BLOCK "
                                    + "from v$lock l1 join v$session on l1.sid=v$session.sid "
                                    + "where l1.id1 in (select l1.id1 from v$lock l1 where l1.block > 0) order by l1.id1, l1.block desc";

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                this.connection = new OracleConnection("Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = )(PORT = 1521))" +
                    "(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = )));Password=;User ID=;");
                this.connection.Open();
                this.initializeTable();
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show("Błąd połączenia do bazy danych", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(OracleException ex)
            {
                MessageBox.Show("Błąd połączenia do bazy danych", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void initializeTable()
        {
            try
            {
                this.dataGrid.ItemsSource = null;
                OracleCommand command = new OracleCommand(this.query, this.connection);
                OracleDataReader reader = command.ExecuteReader();
                ObservableCollection<Session> sessionList = new ObservableCollection<Session>();
                while (reader.Read())
                {
                    sessionList.Add(new Session("", reader.GetString(0), reader.GetDecimal(1).ToString(), reader.GetDecimal(2).ToString(), reader.GetDecimal(3).ToString()));
                }
                reader.Dispose();
                command.Dispose();
                this.dataGrid.ItemsSource = sessionList;
                sessionList = null;
            }
            catch(InvalidCastException ex)
            {
                MessageBox.Show("Błąd pobierania tabeli z lockami!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                this.connection.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Błąd połączenia z baza danych", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.initializeTable();
        }

        private void BtnKill_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid.SelectedItem != null)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("alter system kill session '");
                    builder.Append(((Session)this.dataGrid.SelectedItem).Sid);
                    builder.Append(",");
                    builder.Append(((Session)this.dataGrid.SelectedItem).Serial);
                    builder.Append("'");
                    OracleCommand query = new OracleCommand(builder.ToString(), this.connection);
                    query.ExecuteNonQuery();
                    ((Session)this.dataGrid.SelectedItem).Status = "KILLED";
                    this.dataGrid.Items.Refresh();
                    query.Dispose();
                    builder = null;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Błąd podczas ubijania sesjii użytkownika", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
