﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kursa4_build42820.Forms.MainForms.UserSubsystem.Operations
{
    public partial class GiveBookForm : Form
    {
        public GiveBookForm()
        {
            InitializeComponent();
        }

        Database database;

        private void GiveBookForm_Load(object sender, EventArgs e)
        {
            database = new Database();
            database.OpenConnection();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            database.CloseConnection();
            this.Close();
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        Point lastPoint;

        private void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void Title_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private async void OkButton_Click(object sender, EventArgs e)
        {
            if (ExceptionLabel.Visible)
                ExceptionLabel.Visible = false;

            if (!string.IsNullOrEmpty(IdTextBox.Text) && IsNumber(IdTextBox.Text) && !string.IsNullOrWhiteSpace(IdTextBox.Text))
            {
                string sqlQuery = "DELETE FROM [Clients] WHERE [Id] = @Id";
                SqlCommand command = new SqlCommand(sqlQuery, database.GetSqlConnection());
                command.Parameters.AddWithValue("Id", IdTextBox.Text);

                //operation
                string operationQuery = "INSERT INTO [Operations] ([Date], [ClientId], [BookId], [Type])" +
                                        "VALUES (GETDATE(), @ClientId, @BookId, @Type)";
                SqlCommand operationCommand = new SqlCommand(operationQuery, database.GetSqlConnection());

                operationCommand.Parameters.AddWithValue("ClientId", User.GetClientId(database.GetSqlConnection()));
                operationCommand.Parameters.AddWithValue("BookId", Book.GetBookId(database.GetSqlConnection(), IdTextBox.Text));
                operationCommand.Parameters.AddWithValue("Type", "вернул");
                //

                //copies
                string booksQuery = "UPDATE [Books] SET [Copies] = @Copies WHERE [Id] = @Id";
                SqlCommand booksCommand = new SqlCommand(booksQuery, database.GetSqlConnection());
                booksCommand.Parameters.AddWithValue("Id", Book.GetBookId(database.GetSqlConnection(), IdTextBox.Text));
                booksCommand.Parameters.AddWithValue("Copies", Book.GetCopies(database.GetSqlConnection(), 
                    Book.GetBookId(database.GetSqlConnection(), IdTextBox.Text)) + 1);
                //

                try
                {
                    await command.ExecuteNonQueryAsync(); //--
                    await operationCommand.ExecuteNonQueryAsync(); //--
                    await booksCommand.ExecuteNonQueryAsync(); //--
                    Console.WriteLine("Successful delete a record...");
                    database.CloseConnection();
                    this.Close();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Exception thrown...\n" + ex.Message);
                    MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Console.WriteLine("Failure! Check yor input data and try again...");
                ExceptionLabel.Visible = true;
                ExceptionLabel.Text = "Заполните обязательное поле и проверьте его правильность!";
            }
        }

        private bool IsNumber(string str)
        {
            Regex rxNums = new Regex(@"^\d+$"); // любые цифры
            if (rxNums.IsMatch(str))
                return true;
            else
                return false;
        }
    }
}
