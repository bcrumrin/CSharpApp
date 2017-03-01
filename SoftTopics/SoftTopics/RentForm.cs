﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace SoftTopics
{
    public partial class RentForm : Form
    {
        private SqlConnection myConn;
        private SqlCommand myCmd;
        private SqlDataReader myReader;
        private string CustomerName;
        private string CustomerLastName;
        private string CustomerPhone;
        int totalPrice;
        public RentForm()
        {
            InitializeComponent();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            string FirstName = txtFirstName.Text;
            string LastName = txtLastName.Text;
            string PhoneNumber = txtPhoneNumber.Text;

            findCust(FirstName, LastName, PhoneNumber);
            lblCustomerName.Text = CustomerName + CustomerLastName;
            lblCustomerNumber.Text = CustomerPhone;

        }

        private void findCust(string FName, string LName, string PNumber)
        {
            string customerCard = txtCustomerCard.Text;

            myConn = new SqlConnection("Server=softwarecapproject.database.windows.net;Database=VideoStoreUsers;User ID = bcrumrin64; Password=S0ftT0pix!; Encrypt=True; TrustServerCertificate=False; Connection Timeout=30;");
            myConn.Open();
            myCmd = new SqlCommand(@"SELECT FName, LName, PhoneNumber, CustomerCard FROM Customers WHERE 
            (FName = @Fname AND LName = @Lname)
            OR (FName = @Fname AND PhoneNumber = @PNumber)
            OR (LName = @Lname AND PhoneNumber = @PNumber)
            OR (CustomerCard = @CustomerCard)", myConn);
            myCmd.Parameters.AddWithValue("@Fname", FName);
            myCmd.Parameters.AddWithValue("@Lname", LName);
            myCmd.Parameters.AddWithValue("@PNumber", PNumber);
            myCmd.Parameters.AddWithValue("@CustomerCard", customerCard);
            


            myReader = myCmd.ExecuteReader();
            while (myReader.Read())
            {
                string FirstName;
                string LastName;
                string PhoneNumber;

                FirstName = (string)myReader.GetValue(myReader.GetOrdinal("FName"));
                LastName = (string)myReader.GetValue(myReader.GetOrdinal("LName"));
                PhoneNumber = (string)myReader.GetValue(myReader.GetOrdinal("PhoneNumber"));

                lvSearchResults.View = View.Details;
                lvSearchResults.FullRowSelect = true;
                string[] result = {FirstName, LastName, PhoneNumber};
                lvSearchResults.Items.Add(new ListViewItem(result));
                

            }
            
 
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            ListViewItem selected = lvSearchResults.SelectedItems[0];
            CustomerName = selected.SubItems[0].Text;
            CustomerLastName = selected.SubItems[1].Text;
            CustomerPhone = selected.SubItems[2].Text;
            string customerFull = CustomerName + CustomerLastName;
            customerFull = customerFull.Replace(" ", "");
            lblCustomerName.Text = CustomerName + CustomerLastName;
            lblCustomerNumber.Text = CustomerPhone;

            string fileName = "..\\files\\" + CustomerName + CustomerLastName + CustomerPhone + ".txt";
            fileName = fileName.Replace(" ", "");
            if (File.Exists(fileName))
            {
                checkOverdue(fileName);
                checkLate(CustomerName,CustomerLastName,CustomerPhone);
            }
            else
            {
                StreamWriter sw = File.CreateText(fileName);
                sw.Close();
            }


        }

        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkLate(string firstName, string lastName, string Phone)
        {
            firstName = firstName.Replace(" ", "");
            lastName = lastName.Replace(" ", "");
            int totalDays = 0;
            string lateFile = "..\\Files\\LateReturns.txt";
            using (StreamReader sr = new StreamReader(lateFile))
            {
                string line;
                while ((line = sr.ReadLine())!=null)
                {
                    string[] lineInfo = line.Split(',');
                    string FName = lineInfo[1];
                    string LName = lineInfo[2];
                    string PNumber = lineInfo[3];
                    int days;
                    int.TryParse(lineInfo[4], out days);

                    if (FName.Equals(firstName) && LName.Equals(lastName) && PNumber.Equals(Phone))
                    {
                        totalDays += days;
                    }
                }
            }

            if (totalDays > 0)
            {
                MessageBox.Show("Inform the customer they have " + totalDays + " days of late fees", "Fee notice");
                totalPrice += (totalDays * 3);
                updateLateFees(firstName, lastName, Phone);
            }
        }

        private void updateLateFees(string firstName, string lastName, string Phone)
        {
            string lateFile = "..\\Files\\LateReturns.txt";
            string tempFile = "..\\Files\\temp.txt";
            using (StreamWriter sw = new StreamWriter(tempFile))
            {
                using (StreamReader sr = new StreamReader(lateFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] lineInfo = line.Split(',');
                        string FName = lineInfo[1];
                        string LName = lineInfo[2];
                        string PNumber = lineInfo[3];
                        int days;
                        int.TryParse(lineInfo[4], out days);

                        if (FName.Equals(firstName) && LName.Equals(lastName) && PNumber.Equals(Phone))
                        {
                            continue;
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
            }

            File.Delete(lateFile);
            File.Move(tempFile, lateFile);

        }


        private void checkOverdue(string file)
        {
            string line;
            Boolean update = false;
            
            StreamReader filePath = new StreamReader(file);
            int overdueDays = 0;
            int overdueMovies = 0;

            while ((line = filePath.ReadLine()) != null)
            {
                string[] customerInfo = line.Split(',');

                string barcode = customerInfo[1];
                if (checkReturn(barcode))
                {
                    update = true;
                    updateCustomerFile(barcode, file);
                    continue;
                }


                System.DateTime today = new System.DateTime();
                today = System.DateTime.Today;
                string day = customerInfo[3];
                string month = customerInfo[2];
                string year = customerInfo[4];

                int dateDay;
                int dateMonth;
                int dateYear;

                int.TryParse(day, out dateDay);
                int.TryParse(month, out dateMonth);
                int.TryParse(year, out dateYear);

                System.DateTime checkedOut = new System.DateTime(dateYear, dateMonth, dateDay);

                System.TimeSpan late = today.Subtract(checkedOut);
                int lateDays = late.Days;
                overdueDays+=lateDays;
                overdueMovies++;
  
            }
            filePath.Close();

            if (update)
            {
                File.Delete(file);
                File.Move("..\\Files\\temp.txt",file);
                update = false;
            }
            if (overdueMovies > 0)
            {
                MessageBox.Show("Inform customer they have " + overdueMovies + " overdue movies for " + overdueDays + " days in late fees");
            }
        }

        private void updateCustomerFile(string upc, string customerFile)
        {
            string line;
            StreamReader filePath = new StreamReader(customerFile);
            StreamWriter tempFile = new StreamWriter("..\\Files\\temp.txt");
            while ((line = filePath.ReadLine()) != null)
            {
                string[] movieInfo = line.Split(',');
                string barcode = movieInfo[1];
                if (!barcode.Equals(upc))
                {
                    tempFile.WriteLine(line);
                }
            }

            filePath.Close();
            tempFile.Close();
           
            
        }

        private Boolean checkReturn(string upc)
        {
            string line;
            StreamReader filePath = new StreamReader("..\\Files\\OverDue.txt");
            while ((line = filePath.ReadLine()) != null)
            {
                string[] movieInfo = line.Split(',');
                string barcode = movieInfo[1];
                if (upc.Equals(barcode))
                {
                    filePath.Close();
                    return false;
                }
            }
            filePath.Close();
            return true;
        }

        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && char.IsDigit(e.KeyChar) && !char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            e.Handled = !(char.IsLetter(e.KeyChar) || e.KeyChar == (char)Keys.Back);
        }

        private void txtDVDID_TextChanged(object sender, EventArgs e)
        {
            string barcode = txtDVDID.Text;
            if (barcode.Length == 12)
            {
                movieLookUp(barcode);
            }
        }

        private void movieLookUp(string barCode)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader("..\\Files\\MovieList.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] movieInfo = line.Split(',');
                if (barCode.Equals(movieInfo[0]))
                {
                    lblMovieTitle.Text = movieInfo[1];
                    break;
                }
            }
        }

        private void btnAddMovie_Click(object sender, EventArgs e)
        {
            if (txtRentalDays.Value > 0 && lblMovieTitle.Text != "")
            {
                lvOverview.View = View.Details;
                lvOverview.FullRowSelect = true;
                string[] addMovie = { lblMovieTitle.Text, txtRentalDays.Value.ToString(), txtDVDID.Text };
                lvOverview.Items.Add(new ListViewItem(addMovie));
                totalPrice += (int)(txtRentalDays.Value) * 3;
                lblTotalPrice.Text = totalPrice.ToString();
                lblMovieTitle.Text = "";
                txtRentalDays.ResetText();
                txtDVDID.ResetText();
            }
            
        }

        private void submitMovies(string filePath)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                string movieInfo;
                foreach (ListViewItem item in lvOverview.Items)
                {
                    System.DateTime due = new System.DateTime();
                    due = System.DateTime.Today;
                    int days;
                    int.TryParse(item.SubItems[1].Text, out days);
                    due = due.AddDays((double)days);
                    movieInfo = item.SubItems[0].Text + "," + item.SubItems[2].Text + "," + due.Month.ToString() + "," + due.Day.ToString() + "," + due.Year.ToString();
                    sw.WriteLine(movieInfo);
                }
            }

            using (StreamWriter sw = File.AppendText("..\\Files\\CheckedOut.txt"))
            {
                string movieInfo;
                string firstName = CustomerName;
                string lastName = CustomerLastName;
                lastName = lastName.Replace(" ", "");
                firstName = firstName.Replace(" ", "");
                string phoneNumber = lblCustomerNumber.Text;
                foreach (ListViewItem item in lvOverview.Items)
                {
                    System.DateTime due = new System.DateTime();
                    due = System.DateTime.Today;
                    int days;
                    int.TryParse(item.SubItems[1].Text, out days);
                    due = due.AddDays((double)days);
                    movieInfo = item.SubItems[0].Text + "," + item.SubItems[2].Text + "," + firstName + "," + lastName + "," + phoneNumber + "," + due.Month.ToString() + "," + due.Day.ToString() + "," + due.Year.ToString();
                    sw.WriteLine(movieInfo);
                }
            }

            lvOverview.Items.Clear();
            totalPrice = 0;
            lblTotalPrice.Text = "0.00";

            
        }

        private void btnRemoveMovie_Click(object sender, EventArgs e)
        {
            if (lvOverview.SelectedItems.Count != 0)
            {
                lvOverview.View = View.Details;
                lvOverview.FullRowSelect = true;

                ListViewItem selected = lvOverview.SelectedItems[0];
                int days;
                string temp = selected.SubItems[1].Text;
                int.TryParse(temp, out days);
                totalPrice -= (3 * days);
                lblTotalPrice.Text = totalPrice.ToString();
                lvOverview.SelectedItems[0].Remove();
            }
            

            
        }

        private void btnSubmitPayment_Click(object sender, EventArgs e)
        {
            if (!lblCustomerName.Text.Equals(""))
            {
                string customerFile = "..\\Files\\" + lblCustomerName.Text + lblCustomerNumber.Text + ".txt";
                customerFile = customerFile.Replace(" ", "");
                submitMovies(customerFile);
            }
            else
            {
                MessageBox.Show("Please select a customer profile before submitting order", "Rent Movie");
            }
        }

        private void txtCustomerCard_TextChanged(object sender, EventArgs e)
        {
            if (txtCustomerCard.Text.Length == 13)
            {
                findCust("Null", "Null", "Null");
            }
        }

        
    }
}
