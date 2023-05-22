using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Data.SqlClient;


namespace WindowsFormsApp1
{
     public partial class FrmDDL : Form
    {
        string strLen;
        string strDefault;
        public FrmDDL()
        {
            InitializeComponent();
        }

        private void cboDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtLength.Text = "";
            switch (cboDataType.SelectedItem.ToString())
                { 
              case "nvarchar":
                    {
                        strLen = "1";
                        strDefault = "N''";
                        txtLength.Enabled = true;
                        break;
                    }
                case "int" :
                    {
                        strLen = "0";
                        strDefault = "0";
                        txtLength.Enabled = false;
                        break;
                    }
                case "decimal":
                {
                    strLen = "1";
                    strDefault = "0";
                    txtLength.Enabled = true;
                    break;
                }
                case "bit":
                    {
                        strLen = "0";
                        strDefault = "0";
                        txtLength.Enabled = false;
                        break;
                    }
                default:
                {
                    strLen = "0";
                    strDefault = "N''";
                    txtLength.Enabled = false;
                    break;

                }

            }
        }

        private void cmdGenerate_Click(object sender, EventArgs e)
        {
            if (txtTable.Text.ToLower() == "" || txtColumn.Text.ToLower() == "" || cboDataType.SelectedIndex < 0 )
            {
                MessageBox.Show("Cannot Process Your Request", "SQL Constructor");
                return;
            }


            if (txtLength.Enabled == true && txtLength.Text == "")
            {
                switch (cboDataType.SelectedItem.ToString())
                {
                    case "nvarchar":
                        {
                            txtLength.Text = "50";
                            break;
                        }
                    case "decimal":
                        {
                            txtLength.Text = "18,2";
                            break;
                        }
                }


            }


            //                MessageBox.Show("Data Length is Required for this Data Type", "SQL Constructor");
            //              txtLength.Focus();
            //             return;



            string strDataLength = "";
            strDataLength = strLen == "0" ? "" : "(" + txtLength.Text.ToLower() + ")";
            string sqlString;
            sqlString = "IF COL_LENGTH('" + txtTable.Text.ToLower() + "','" + txtColumn.Text.ToLower() + "') IS NULL " + Environment.NewLine +
                "   BEGIN " + Environment.NewLine +
                "     ALTER TABLE [dbo].[" + txtTable.Text.ToLower() + "] ADD [" + txtColumn.Text.ToUpper() + "] " + cboDataType.SelectedItem.ToString().ToLower() + strDataLength + " NOT NULL " + Environment.NewLine +
                "       CONSTRAINT [DF_" + txtTable.Text.ToUpper() + "_" + txtColumn.Text.ToUpper() + "] DEFAULT " + strDefault + Environment.NewLine +
                "   END " + Environment.NewLine +
                "ELSE " + Environment.NewLine +
                "   BEGIN " + Environment.NewLine +
                "     IF EXISTS ( SELECT 1 " + Environment.NewLine +
                "                    FROM sys.default_constraints " + Environment.NewLine +
                "                    WHERE object_id = OBJECT_ID('[dbo].[DF_" + txtTable.Text.ToLower() + "_" + txtColumn.Text.ToLower() + "]') " + Environment.NewLine +
                "                      AND parent_object_id = OBJECT_ID('[dbo].[" + txtTable.Text.ToLower() + "]')" + Environment.NewLine +
                "                  ) " + Environment.NewLine +
                "            BEGIN " + Environment.NewLine +
                "                ALTER TABLE [dbo].["+ txtTable.Text.ToLower() + "] DROP CONSTRAINT [DF_" + txtTable.Text.ToLower() + "_" + txtColumn.Text.ToLower() + "] " + Environment.NewLine +
                "                ALTER TABLE [dbo].[" + txtTable.Text.ToLower() + "] ALTER COLUMN [" + txtColumn.Text.ToUpper() + "] " + cboDataType.SelectedItem.ToString().ToLower() + strDataLength + " NOT NULL " + Environment.NewLine +
                "                ALTER TABLE [dbo].[" + txtTable.Text.ToLower() + "] ADD CONSTRAINT [DF_" + txtTable.Text.ToUpper() + "_" + txtColumn.Text.ToUpper() + "] DEFAULT " + strDefault + " FOR [" + txtColumn.Text.ToLower() + "] " +  Environment.NewLine +
                "            END " + Environment.NewLine +
                "    END;" + Environment.NewLine +
                "GO";
            txtScript.Text = sqlString;
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtScript.Text);
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            txtTable.Text = "";
            txtColumn.Text = "";
            txtLength.Text = "";
            txtScript.Text = "";
            txtLength.Enabled = false;
            txtTable.Focus();
        }

        private void cmdDrop_Click(object sender, EventArgs e)
        {

            if (txtTable.Text.ToLower() == "" || txtColumn.Text.ToLower() == "" )
            {
                MessageBox.Show("Cannot Process Your Request", "SQL Constructor");
                return;
            }
            string sqlString;
            sqlString = "IF COL_LENGTH('" + txtTable.Text.ToLower() + "','" + txtColumn.Text.ToLower() + "') IS NOT NULL " + Environment.NewLine +
                   "   BEGIN " + Environment.NewLine +
                "     IF EXISTS ( SELECT 1 " + Environment.NewLine +
                "                    FROM sys.default_constraints " + Environment.NewLine +
                "                    WHERE object_id = OBJECT_ID('[dbo].[DF_" + txtTable.Text.ToLower() + "_" + txtColumn.Text.ToLower() + "]') " + Environment.NewLine +
                "                      AND parent_object_id = OBJECT_ID('[dbo].[" + txtTable.Text.ToLower() + "]')" + Environment.NewLine +
                "                  ) " + Environment.NewLine +
                "            BEGIN " + Environment.NewLine +
                "                ALTER TABLE [dbo].[" + txtTable.Text.ToLower() + "] DROP CONSTRAINT [DF_" + txtTable.Text.ToLower() + "_" + txtColumn.Text.ToLower() + "] " + Environment.NewLine +
                "            END " + Environment.NewLine +
                "            ALTER TABLE [dbo].[" + txtTable.Text.ToLower() + "] DROP COLUMN [" + txtColumn.Text.ToUpper() + "] " + Environment.NewLine +
                "END;" + Environment.NewLine +
                "GO";
            txtScript.Text = sqlString;

        }

        private void cmdValidate_Click(object sender, EventArgs e)
        {
            string strconn = "Data Source=172.29.11.213,1433;Trusted_Connection=false;MultipleActiveResultSets=true;User ID=DevTeam;Password=D3vT3am09;Initial Catalog=QATEAM4;";
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = strconn;
            conn.Open();
            string strCommand = txtScript.Text; // "Declare @emp_no nvarchar(10)\r\nset @emp_no = '00112233N'\r\n select * from esec where emp_no = @emp_no";
            string sqlCured = strCommand.Replace("\r\nGO", " ");
            SqlCommand cmd = new SqlCommand(sqlCured, conn);
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            
            MessageBox.Show("Database Validation Successful","Success");
            conn.Close();

        }

        private void FrmDDL_Load(object sender, EventArgs e)
        {

        }
    }
}



