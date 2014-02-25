using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ScriptRunner2
{
    public partial class Options : Window, IComponentConnector
    {
       public string sqlConnectionString { get; set; }
        public Options()
        {
            this.InitializeComponent();
            tbServerName.Focus();
            tbServerName.SelectAll();
        }

        private void options_Loaded(object sender, RoutedEventArgs e)
        {
           this.cbOpenScript.IsChecked = new bool?(true);
           //this.cbContinueOnError.IsChecked = new bool?(false);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void options_Initialized(object sender, EventArgs e)
        {
            this.cbContinueOnError.IsChecked = new bool?(false);
            this.cbOpenScript.IsChecked = new bool?(false);
        }

        private void cbContinueOnError_Initialized(object sender, EventArgs e)
        {
            this.cbContinueOnError.IsChecked = new bool?(false);
        }

        private void cbOpenScript_Initialized(object sender, EventArgs e)
        {
           this.cbOpenScript.IsChecked = new bool?(false);
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            this.tbPassword.Password = "";
            this.tbUserName.Text = "";
            this.tbPassword.IsEnabled = this.tbUserName.IsEnabled = false;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            this.tbPassword.Password = "";
            this.tbUserName.Text = "";
            this.tbPassword.IsEnabled = this.tbUserName.IsEnabled = true;
        }

        private void btConectar_Click(object sender, RoutedEventArgs e)
        {
           tentaConectarBancoDeDados();
        }
        private void tentaConectarBancoDeDados()
        {
           if (string.IsNullOrEmpty(tbServerName.Text))
           {
              tbServerName.Focus();
              return;
           }

           if (string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Password))
           {
              sqlConnectionString = string.Format("Data Source={0};Initial Catalog=master;Integrated Security=True", tbServerName.Text);
           }
           else
           {
              sqlConnectionString =
                  string.Format("Data Source={0};UID={1};PWD={2};Database=master;",
                      tbServerName.Text, tbUserName.Text, tbPassword.Password);
           }

           using (SqlConnection connection = new SqlConnection(sqlConnectionString))
           {
              try
              {
                 connection.Open();

                 carregaBancoDeDados(connection);
              }
              catch (Exception ex)
              {
                 MessageBox.Show("Erro: " + ex.Message);
              }

           }

        }

        private void carregaBancoDeDados(SqlConnection connection)
        {
           string query = "select name from master.sys.sysdatabases WHERE dbid > 4 ORDER BY name";// where owner_sid > 1

           // you must set already sqlConnection for sqlCon parameter
           SqlDataReader dReader;
           SqlCommand cmd = new SqlCommand(query, connection);
           cmd.CommandType = CommandType.Text;
           try
           {
              dReader = cmd.ExecuteReader();
           }
           catch (Exception ex)
           {
              MessageBox.Show("Erro: " + ex.Message);
              return;
           }

           if (dReader.HasRows)
           {
              cbBancoDeDados.Items.Clear();
              cbBancoDeDados.Items.Add("Selecione");
              cbBancoDeDados.SelectedIndex = 0;
              while (dReader.Read())
              {
                 cbBancoDeDados.Items.Add(dReader[0]);
              }
              //BtConfirmar.Enabled = true;
           }
           else
           {
              //BtConfirmar.Enabled = false;
              MessageBox.Show("Atenção: Nenhum Banco de Dados foi Encontrado.");
           }
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
           this.Hide();
           e.Cancel = true;
        }
    }
}
