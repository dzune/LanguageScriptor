﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Globalization;
using System.Net.Http;
using System.Collections;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using RestSharp;
using System.Configuration;

namespace WindowsFormsApp1
{
    public partial class FrmTranslate : Form
    {
        
        public string table = "ZSCRLANG";
        string format = "D2";

        TextInfo TxConvert = CultureInfo.CurrentCulture.TextInfo;
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (optZPAYFIX.Checked)
                doSettings(3);
        }

        private void optZSCLANG_CheckedChanged(object sender, EventArgs e)
        {
            if (optZSCLANG.Checked)
                doSettings(1);
        }

        private void optZPAYMSG_CheckedChanged(object sender, EventArgs e)
        {
            if (optZPAYMSG.Checked)
                doSettings(2);
        }

        private void optMenu_CheckedChanged(object sender, EventArgs e)
        {
            if (optMenu.Checked)
                doSettings(4);
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
                cmdClear.PerformClick();
                txtFormName.Text = "";
                txtOrder.Text = table=="MASTERMENU"?"":1.ToString(format);
                txtValue.Text = "1";
                txtOwner.Text = "";
                txtScript.Text = "";
                EP.SetError(txtFormName, "");
                EP.SetError(txtOwner, "");
                EP.SetError(txtOrder, "");
                EP.SetError(txtAPI, "");
                txtFormName.Focus();
                cmdGenerate.Enabled = !false;
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            txtLang1.Text = "";
            txtLang2.Text = "";
            txtLang3.Text = "";
            txtLang4.Text = "";
            txtLang5.Text = "";
            txtLang6.Text = "";
            txtLang7.Text = "";
            txtLang8.Text = "";
            txtLang9.Text = "";
            txtLang10.Text = "";
            txtMultiple.Text = "";
            var stringNumber = txtOrder.Text.ToString();
            int numericValue;
            bool isNumber = int.TryParse(stringNumber, out numericValue);

            if (!isNumber)
                txtOrder.Text = "1";
       

            switch (table)
            {
                case "ZSCRLANG":
                case "ZPAYMSG":
                    {
                        txtOrder.Text = (Convert.ToInt32(txtOrder.Text) + 1).ToString(format);
                        txtLang1.Focus();
                        break;
                    }
                case "ZPAYFIX":
                    {
                        txtValue.Text = (Convert.ToInt32(txtValue.Text) + 1).ToString();
                        txtLang1.Focus();
                        break;
                    }
                default:
                    {
                        txtOwner.Text = "";
                        txtOrder.Text = "";
                        txtOwner.Focus();
                        break;
                    }
            }
            cmdGenerate.Enabled = !false;


        }
        private void cmdCopy_Click(object sender, EventArgs e)
        {
            if (txtScript.Text!= "")
            {
                if (!txtScript.Text.Contains("Make sure that the file Encoding is Windows-1252"))
                {
                    txtScript.Text = txtScript.Text + Environment.NewLine + Environment.NewLine + "-- Run your script on PAYSONNELCE database and update the AfterRestore script";
                    txtScript.Text = txtScript.Text + Environment.NewLine + "-- Make sure that the file Encoding is Windows-1252";
                }
                Clipboard.SetText(txtScript.Text.ToString());
        }
        }

        private void cmdGenerate_Click(object sender, EventArgs e)
        {
            if (txtOwner.Text == "")
            {
                if (table == "ZSCRLANG")
                {
                    if (txtFormName.Text == "" || txtOwner.Text == "")
                    {
                        if (txtOwner.Text == "")
                        {
                            EP.SetError(txtOwner, "Required Field: Use Work Order Number");
                            txtOwner.Focus();
                        }

                        if (txtFormName.Text == "")
                        {
                            EP.SetError(txtFormName, "Required Field: Use Form that will use the Label");
                            txtFormName.Focus();
                        }
                    }
                }
                else
                {
                    EP.SetError(txtOwner, "Required Field");
                    txtOwner.Focus();
                }
                return;
            }

            if (chkGoogle.Checked)
            {
                if (txtAPI.Text.ToString().Trim() == "")
                {
                    EP.SetError(txtAPI, "API Key Cannot be left blank for this option");
                    txtAPI.Focus();
                    return;
                }
            }

            if (txtMultiple.Text.ToString() != "" && txtLang1.Text == "")
            {
                char[] separators = new char[] { ',' };
                string[] mySource = txtMultiple.Text.ToString().Split(separators);

                foreach (string mySrc in mySource)

                {
                    txtLang1.Text = mySrc.ToString().Trim();
                    //
                    getTranslation();
                    switch (table)
                    {
                        case "ZSCRLANG":
                            {
                                DoZSCRLANG();
                                break;
                            }
                        case "ZPAYFIX":
                            {
                                DoZPAYFIX();
                                break;
                            }
                        default:
                            {
                                DoZPAYMSG();
                                break;
                            }
                    }
                    cmdClear.PerformClick();
                }
            }
            else // use language1
            {
                if (chkGoogle.Checked)
                {
                    if (txtLang1.Text != "")
                    {
                        getTranslation();
                    }
                }
                else
                {
                    if (!DoValidate())
                        return;
                }
                switch (table)
                {
                    case "ZSCRLANG":
                        DoZSCRLANG();
                     //   cmdClear.PerformClick();
                        break;
                    case "ZPAYMSG":
                        DoZPAYMSG();
                     //   cmdClear.PerformClick();
                        break;
                    case "ZPAYFIX":
                        DoZPAYFIX();
                    //    cmdClear.PerformClick();
                        break;
                    case "MASTERMENU":
                        if (txtOrder.Text == "")
                        {
                            EP.SetError(txtOrder, "Menu Option Cannot Be Blank");
                            cmdGenerate.Enabled = true;
                            txtOrder.Focus();
                            return;
                        }

                        DoMASTERMENU();
                     // cmdClear.PerformClick();
                        break;

                }
                cmdGenerate.Enabled = false;
            }


        }


        public FrmTranslate()
        {
            InitializeComponent();
        }

        private void FrmTranslate_Load(object sender, EventArgs e)
        {
            cboModule.SelectedIndex = 0;
            optZSCLANG.Checked = true;
            // rapid api
            // if (System.Net.Dns.GetHostName()== "ASC9003435")
            txtAPI.Text = "aa81f37d9dmshd4148cb6d6781f8p158e02jsn41a391523992";
            //** end

            // microsoft api
            //txtAPI.Text = "581e129ea7cb4843a71f74c842544c1f";
            //** end


#pragma warning disable CS0618 // Type or member is obsolete
            string appSettings = ConfigurationSettings.AppSettings["APIKey"].ToString();
#pragma warning restore CS0618 // Type or member is obsolete
          //  txtAPI.Text = appSettings;  // restore for MS-only
        }
        private void chkGoogle_CheckedChanged(object sender, EventArgs e)
        {
            
            bool flag = chkGoogle.Checked;
            if (!flag) EP.SetError(txtAPI, "");

            txtMultiple.Enabled = flag;
            txtLang2.ReadOnly = flag;
            txtLang3.ReadOnly = flag;
            txtLang4.ReadOnly = flag;
            txtLang5.ReadOnly = flag;
            txtLang6.ReadOnly = flag;
            txtLang7.ReadOnly = flag;
            txtLang8.ReadOnly = flag;
            txtLang9.ReadOnly = flag;
            txtLang10.ReadOnly = flag;
            txtAPI.Enabled = flag;
        }

        private void txtOwner_TextChanged(object sender, EventArgs e)
        {
            if (txtOwner.Text != "")
                EP.SetError(txtOwner, "");
        }

        private void txtFormName_TextChanged(object sender, EventArgs e)
        {
            if (txtFormName.Text != "")
                EP.SetError(txtFormName, "");
        }

        private void txtOrder_TextChanged(object sender, EventArgs e)
        {
            if (txtOrder.Text != "" && chkGoogle.Checked)
                EP.SetError(txtOrder, "");
        }

        private void txtAPI_TextChanged(object sender, EventArgs e)
        {
            if (txtAPI.Text != "")
                EP.SetError(txtAPI, "");
        }
        public bool DoValidate()
        {
            if (txtLang1.Text == "" || txtLang2.Text == "" || txtLang3.Text == "" || txtLang4.Text == "" || txtLang5.Text == "" ||
                txtLang6.Text == "" || txtLang7.Text == "" || txtLang8.Text == "" || txtLang9.Text == "" || txtLang10.Text == "")
            {
                MessageBox.Show("You Cannot Leave any Language Translation Empty", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                return true;
            }
        }

        //FUNCTIONS       
        private void DoZSCRLANG()
        {
            //if (txtLang1.Text != "")
            //{
            //    getTranslation();
            //}

            StringBuilder xScript = new StringBuilder();
            if (txtScript.Text.ToString().Trim() == "")
            {
                xScript.Append("SET ANSI_NULLS ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET QUOTED_IDENTIFIER ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET ANSI_PADDING ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET NOCOUNT ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine + Environment.NewLine);
            }
            xScript.Append("DECLARE @formname VARCHAR(50)"+Environment.NewLine) ;
            xScript.Append("DECLARE @lblname VARCHAR(2100)" + Environment.NewLine) ;
            xScript.Append("DECLARE @owner VARCHAR(20)" + Environment.NewLine) ;
            xScript.Append("DECLARE @cnt NUMERIC(10)" + Environment.NewLine);
            xScript.Append("DECLARE @ord VARCHAR(4)" + Environment.NewLine+Environment.NewLine);
            
            xScript.Append("SET @formname = '" +  txtFormName.Text.ToUpper() + "'" + Environment.NewLine);
            xScript.Append("SET @lblname = '" + txtLang1.Text.Trim().Replace("'","''") + "'" + Environment.NewLine);
            xScript.Append("SET @owner = '" + txtOwner.Text.ToUpper() +"'" + Environment.NewLine);
            xScript.Append("SET @ord = '" + Convert.ToInt32(txtOrder.Text).ToString("D2") +"'" + Environment.NewLine + Environment.NewLine);

            xScript.Append("SELECT @cnt = (SELECT COUNT(*)  FROM zscrlang WHERE form_name = @formname and ord = @ord)" + Environment.NewLine);
            xScript.Append("IF @cnt = 0" + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("\t INSERT  zscrlang(form_name,ord,[lang_1],[owner],[ready])" + Environment.NewLine);
            xScript.Append("\t     VALUES " + Environment.NewLine);
            xScript.Append("\t (@formname,@ord, @lblname, @owner,'1')" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("UPDATE zscrlang " + Environment.NewLine);
            xScript.Append("\t SET [lang_1] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_2]    = '" + txtLang2.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_3]    = '" + txtLang3.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_4]    = '" + doGB2312(txtLang4.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_5]    = '" + txtLang5.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_6]    = '" + txtLang6.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_7]    = '" + HtmlEncode_ConvertAll(txtLang7.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_8]    = '" + HtmlEncode_ConvertAll(txtLang8.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_9]    = '" + HtmlEncode_ConvertAll(txtLang9.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_10]    = '" + HtmlEncode_ConvertAll(txtLang10.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_11] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_12] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_13] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_14] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_15] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_16] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_17] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_18] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_19] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_20] = @lblname" + Environment.NewLine);
            xScript.Append("WHERE  (form_name=@formname and ord=@ord)" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("SELECT 'Record Added' , * FROM zscrlang WHERE [form_name] = @formname and [lang_1]=@lblname" + Environment.NewLine);
            xScript.Append("GO" + Environment.NewLine + Environment.NewLine);
           
            txtScript.Text = txtScript.Text + xScript.ToString();
            //Clipboard.SetText(txtScript.Text.ToString());
        }

        private void DoZPAYFIX()
        {
            //if (txtLang1.Text != "")
            //{
            //    getTranslation();
            //}

            StringBuilder xScript = new StringBuilder();

            if (txtScript.Text.ToString().Trim() == "")
            {
                xScript.Append("SET ANSI_NULLS ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET QUOTED_IDENTIFIER ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET ANSI_PADDING ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET NOCOUNT ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine + Environment.NewLine);
            }
            xScript.Append("DECLARE @code VARCHAR(30)" + Environment.NewLine);
            xScript.Append("DECLARE @lblname VARCHAR(1050)" + Environment.NewLine);
            xScript.Append("DECLARE @owner VARCHAR(20)" + Environment.NewLine);
            xScript.Append("DECLARE @cnt NUMERIC(10)" + Environment.NewLine);
            xScript.Append("DECLARE @option_avai VARCHAR(3)" + Environment.NewLine + Environment.NewLine);

            /* Change the Four variable below */

            
            xScript.Append("SET @LBLName = '" + txtLang1.Text.Trim().Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("SET @owner = '" + txtOwner.Text.ToUpper() + "'" + Environment.NewLine);
            xScript.Append("SET @option_avai = '" + txtValue.Text.ToUpper() + "'" + Environment.NewLine);
            xScript.Append("SET @code = '" + Convert.ToInt32(txtOrder.Text).ToString(format) + "'" + Environment.NewLine + Environment.NewLine);

            xScript.Append("SELECT @cnt = (SELECT COUNT(*)  FROM zpayfix WHERE code = @code and option_avai = @option_avai)" + Environment.NewLine);
            xScript.Append("IF @cnt = 0" + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("\t INSERT  zpayfix([code],[lang_1],[option_avai],[owner],[ready])" + Environment.NewLine);
            xScript.Append("\t     VALUES " + Environment.NewLine);
            xScript.Append("\t (@code,@lblname,@option_avai,@owner,'1')" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("UPDATE zpayfix " + Environment.NewLine);
            xScript.Append("\t SET [lang_1] = @lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_2]    = '" + txtLang2.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_3]    = '" + txtLang3.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_4]    = '" + doGB2312(txtLang4.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_5]    = '" + txtLang5.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_6]    = '" + txtLang6.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_7]    = '" + HtmlEncode_ConvertAll(txtLang7.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_8]    = '" + HtmlEncode_ConvertAll(txtLang8.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_9]    = '" + HtmlEncode_ConvertAll(txtLang9.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_10]    = '" + HtmlEncode_ConvertAll(txtLang10.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_11] = '11_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_12] = '12_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_13] = '13_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_14] = '14_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_15] = '15_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_16] = '16_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_17] = '17_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_18] = '18_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_19] = '19_'+@lblname" + Environment.NewLine);
            xScript.Append("\t ,[lang_20] = '20_'+@lblname" + Environment.NewLine);
            xScript.Append("WHERE ([lang_1] = @lblname) or (code = @code and option_avai = @option_avai)" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("SELECT 'Record Added' , * FROM zpayfix WHERE [code] = @code and option_avai = @option_avai " + Environment.NewLine);
            xScript.Append("GO" + Environment.NewLine + Environment.NewLine);


            txtScript.Text = txtScript.Text + xScript.ToString();
            //Clipboard.SetText(txtScript.Text.ToString());
        }


        private void DoZPAYMSG()
        {
            //if (txtLang1.Text != "")
            //{
            //    getTranslation();
            //}

            StringBuilder xScript = new StringBuilder();

            if (txtScript.Text.ToString().Trim() == "")
            {
                xScript.Append("SET ANSI_NULLS ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET QUOTED_IDENTIFIER ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET ANSI_PADDING ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET NOCOUNT ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine + Environment.NewLine);
            }
            xScript.Append("DECLARE @code VARCHAR(30)" + Environment.NewLine);
            xScript.Append("DECLARE @name VARCHAR(2100)" + Environment.NewLine);
            xScript.Append("DECLARE @owner VARCHAR(20)" + Environment.NewLine);

            xScript.Append("SET @name = '" + txtLang1.Text.Trim().Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("SET @owner = '" + txtOwner.Text.ToUpper() + "'" + Environment.NewLine);
            xScript.Append("SET @code = '" + Convert.ToInt32(txtOrder.Text).ToString(format) + "'" + Environment.NewLine + Environment.NewLine);
            
            xScript.Append("IF NOT  EXISTS( SELECT * FROM zpaymsg WHERE code = @code )" + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("\t INSERT  zpaymsg([code],[lang_1],[owner],[ready])" + Environment.NewLine);
            xScript.Append("\t     VALUES " + Environment.NewLine);
            xScript.Append("\t (@code,@name,@owner,'1')" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("UPDATE zpaymsg " + Environment.NewLine);
            xScript.Append("\t SET [lang_1] = @name" + Environment.NewLine);
            xScript.Append("\t ,[lang_2]    = '" + txtLang2.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_3]    = '" + txtLang3.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_4]    = '" + doGB2312(txtLang4.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_5]    = '" + txtLang5.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_6]    = '" + txtLang6.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_7]    = '" + HtmlEncode_ConvertAll(txtLang7.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_8]    = '" + HtmlEncode_ConvertAll(txtLang8.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_9]    = '" + HtmlEncode_ConvertAll(txtLang9.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_10]    = '" + HtmlEncode_ConvertAll(txtLang10.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[lang_11] = '11_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_12] = '12_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_13] = '13_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_14] = '14_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_15] = '15_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_16] = '16_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_17] = '17_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_18] = '18_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_19] = '19_'+@name" + Environment.NewLine);
            xScript.Append("\t ,[lang_20] = '20_'+@name" + Environment.NewLine);
            xScript.Append("WHERE ([lang_1] = @name) or (code = @code)" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("SELECT 'Record Added' , * FROM zpaymsg WHERE [code] = @code and lang_1 = @name " + Environment.NewLine);
            xScript.Append("GO" + Environment.NewLine + Environment.NewLine);


            txtScript.Text = txtScript.Text + xScript.ToString();
            //Clipboard.SetText(txtScript.Text.ToString());
        }

    
        private void DoMASTERMENU()
        {
            
            //if (txtLang1.Text != "")
            //{
            //    getTranslation();
            //}

            StringBuilder xScript = new StringBuilder();

            if (txtScript.Text.ToString().Trim() == "")
            {
                xScript.Append("SET ANSI_NULLS ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET QUOTED_IDENTIFIER ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET ANSI_PADDING ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine);
                xScript.Append("SET NOCOUNT ON" + Environment.NewLine);
                xScript.Append("GO" + Environment.NewLine + Environment.NewLine);
            }
            xScript.Append("DECLARE @module VARCHAR(3)" + Environment.NewLine);
            xScript.Append("DECLARE @option_1 VARCHAR(200)" + Environment.NewLine);
            xScript.Append("DECLARE @link VARCHAR(200)" + Environment.NewLine);
            xScript.Append("DECLARE @cnt NUMERIC" + Environment.NewLine);
            xScript.Append("DECLARE @option_code VARCHAR(20)" + Environment.NewLine);

            xScript.Append("SET @module = '" + cboModule.SelectedItem.ToString().Substring(0,2) + "'" + Environment.NewLine);
            xScript.Append("SET @link = '" + txtOwner.Text.ToLower() + "'" + Environment.NewLine);
            xScript.Append("SET @option_1 = '" + txtLang1.Text.ToString().Trim() + "'" + Environment.NewLine);
            xScript.Append("SET @option_code = '" + txtOrder.Text.ToString() + "'" + Environment.NewLine + Environment.NewLine);

            xScript.Append("SELECT  @cnt = (SELECT COUNT(*)  FROM [mastermenu] WHERE [module] = @module and [option_code]=@option_code)" + Environment.NewLine);
            xScript.Append("IF @cnt = 0 " + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("\t INSERT [masterMenu] ([module],[option_code]) VALUES (@module,@option_code)" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("BEGIN" + Environment.NewLine);
            xScript.Append("UPDATE [mastermenu] " + Environment.NewLine);
            xScript.Append("\t SET [option_1] = @option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_2]    = '" + txtLang2.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_3]    = '" + txtLang3.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_4]    = '" + doGB2312(txtLang4.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_5]    = '" + txtLang5.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_6]    = '" + txtLang6.Text.Replace("'", "''") + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_7]    = '" + HtmlEncode_ConvertAll(txtLang7.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_8]    = '" + HtmlEncode_ConvertAll(txtLang8.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_9]    = '" + HtmlEncode_ConvertAll(txtLang9.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_10]    = '" + HtmlEncode_ConvertAll(txtLang10.Text) + "'" + Environment.NewLine);
            xScript.Append("\t ,[option_11] = '11_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_12] = '12_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_13] = '13_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_14] = '14_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_15] = '15_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_16] = '16_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_17] = '17_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_18] = '18_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_19] = '19_'+@option_1" + Environment.NewLine);
            xScript.Append("\t ,[option_20] = '20_'+@option_1" + Environment.NewLine);

            xScript.Append("\t ,[link] = @link" + Environment.NewLine);
            xScript.Append("\t ,[iscode]=1" + Environment.NewLine);
            xScript.Append("\t ,[isemptranx]=0" + Environment.NewLine);
            xScript.Append("\t ,[access_global]=1" + Environment.NewLine);
            xScript.Append("\t ,[isdeleteadd1]=1" + Environment.NewLine);
            xScript.Append("\t ,[onlycountry]=0" + Environment.NewLine);
            xScript.Append("\t ,[onlyemp]=0" + Environment.NewLine);
            xScript.Append("\t ,[tablename]=''" + Environment.NewLine);
            xScript.Append("\t ,[menu_iscode]=0" + Environment.NewLine);
            xScript.Append("\t ,[modulecond]='01'" + Environment.NewLine);
            xScript.Append("\t ,[displaylink]=1" + Environment.NewLine);

            xScript.Append("\t,[ctry_1]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_2]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_3]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_4]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_5]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_6]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_7]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_8]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_9]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_10]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_11]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_12]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_13]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_14]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_15]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_16]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_17]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_18]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_19]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_20]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_21]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_22]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_23]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_24]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_25]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_26]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_27]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_28]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_29]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_30]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_31]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_32]=1" + Environment.NewLine);
            xScript.Append("\t,[ctry_33]=1" + Environment.NewLine);
            xScript.Append("\t,[oth_pay]=0" + Environment.NewLine);
            xScript.Append("\t,[ispda]=0" + Environment.NewLine);
            xScript.Append("\t,[ready]=1" + Environment.NewLine);

            xScript.Append("WHERE ([module] = @module) AND ([option_code] = @option_code)" + Environment.NewLine);
            xScript.Append("END" + Environment.NewLine + Environment.NewLine);
            xScript.Append("SELECT 'Record Added' , * FROM mastermenu WHERE [module] = @module and [option_code] = @option_code " + Environment.NewLine);
            xScript.Append("GO" + Environment.NewLine + Environment.NewLine);

            txtScript.Text = txtScript.Text + xScript.ToString();
            //Clipboard.SetText(txtScript.Text.ToString());
        }


        private void doSettings(int caller=1)
        {
         //   myOption = caller;
        //    string table;
            switch (caller)
            {
                case 1: // ZSCRLANG
                    {
                        table = "ZSCRLANG";
                        lblFormName.Text = "Form Name";
                        lblOwner.Text = "Owner";
                        lblOrderCode.Text = "Order";
                        lblValue.Visible = false;
                        txtValue.Visible = false;
                        lblMulti.Visible = true;
                        txtMultiple.Visible = true;
                        txtFormName.Visible = true;
                        txtFormName.Enabled = true;
                        lblFormName.Text = "Form Name";
                        cboModule.Visible = false;
                        format = "D2";
                        break;
                    }
                case 2: //ZPAYMSG
                    {
                        table = "ZPAYMSG";
                        lblFormName.Text = "Form Name";
                        lblOwner.Text = "Owner";
                        lblOrderCode.Text = "Error Code";
                        lblValue.Visible = false;
                        txtValue.Visible = false;
                        lblMulti.Visible = true;
                        txtMultiple.Visible = true;
                        txtFormName.Enabled = !true;
                        txtFormName.Visible = true;
                        lblFormName.Text = "Form Name";
                        cboModule.Visible = false;
                        format = "D5";
                        break;
                    }
                case 3: // ZPAYFIX
                    {
                        table = "ZPAYFIX";
                        lblFormName.Text = "Form Name";
                        lblOwner.Text = "Owner";
                        lblValue.Visible = !false;
                        txtValue.Visible = !false;
                        lblMulti.Visible = true;
                        txtMultiple.Visible = true;
                        txtFormName.Enabled = !true;
                        txtFormName.Visible = true;
                        lblFormName.Text = "Form Name";
                        cboModule.Visible = false;
                        lblOrderCode.Text = "Option Code";
                        format = "D5";
                        break;
                    }
                case 4: // MASTERMENU
                    {
                        table = "MASTERMENU";
                        lblFormName.Text = "Module Code";
                        lblOwner.Text = "Menu Link";
                        lblValue.Visible = false;
                        txtValue.Visible = false;
                        lblMulti.Visible = !true;
                        txtMultiple.Visible = !true;
                        txtFormName.Visible = !true;
                        txtFormName.Enabled = !true;
                        lblFormName.Text = "Module Name";
                        cboModule.Visible = !false;
                        lblOrderCode.Text = "Menu Option";
                        format = "";
                        break;

                    }
            }
            cmdReset.PerformClick();
            txtOrder.Text = table=="MASTERMENU"?"":Convert.ToInt32(txtOrder.Text).ToString(format);
        }

        private void getTranslation()
        {
            /*
            // MICROSOFT API
            //ms,id,zh-Hans,th,fil,ja,ko,vi
            string lang = "";
            string text = "";
            
            dynamic myTranslation = MSTranslate1(txtLang1.Text.Trim());
            foreach (var trLang in myTranslation)
            {
                lang = trLang.to.ToString().ToLower();
                text = trLang.text.ToString();
                // text = TxConvert.ToTitleCase(trLang.text.ToString());

                switch (lang)
                {
                    case "ms":
                        {
                            txtLang2.Text = text;
                            txtLang3.Text = text;
                            break;
                        }
                   case "id":
                        {
                            txtLang5.Text = text;
                            break;
                        }
                    case "zh-hans":
                        {
                            txtLang4.Text = text;
                            break;
                        }
                    case "th":
                        {
                            txtLang7.Text = text;
                            break;
                        }

                    case "fil":
                        {
                            txtLang6.Text = text;
                            break;
                        }
                    case "ja":
                        {
                            txtLang8.Text = text;
                            break;
                        }
                    case "ko":
                        {
                            txtLang9.Text = text;
                            break;
                        }
                    case "vi":
                        {
                            txtLang10.Text = text;
                            break;
                        }



                }
            }

            */

            // RAPID API
            txtLang2.Text = TranslateText(txtLang1.Text.Trim(), "ms");
            txtLang3.Text = txtLang2.Text; // TranslateText(txtLang1.Text, "ms");
            txtLang4.Text = TranslateText(txtLang1.Text.Trim(), "zh-CN");
            txtLang5.Text = TranslateText(txtLang1.Text.Trim(), "id");
            txtLang6.Text = TranslateText(txtLang1.Text.Trim(), "fil");
            txtLang7.Text = TranslateText(txtLang1.Text.Trim(), "th");
            txtLang8.Text = TranslateText(txtLang1.Text.Trim(), "ja");
            txtLang9.Text = TranslateText(txtLang1.Text.Trim(), "ko");
            txtLang10.Text = TranslateText(txtLang1.Text.Trim(), "vi");
        
         }

        public string doGB2312(string s)
        {
            Encoding encoding = Encoding.GetEncoding("GB2312");
            return Encoding.GetEncoding("windows-1252").GetString(encoding.GetBytes(s));
        }

        private string HtmlEncode_ConvertAll(string s)
        {
            uint[] arrayOfUtf32Chars = this.StringToArrayOfUtf32Chars(s);
            StringBuilder stringBuilder = new StringBuilder(2000);
            foreach (uint num in arrayOfUtf32Chars)
            {
                if (num > (uint)sbyte.MaxValue)
                {
                    stringBuilder.AppendFormat("&#{0};", (object)num);
                }
                else
                {
                    switch (Convert.ToChar(num))
                    {
                        case '"':
                            stringBuilder.Append("&quot;");
                            break;
                        case '&':
                            stringBuilder.Append("&amp;");
                            break;
                        case '\'':
                            stringBuilder.Append("&apos;");
                            break;
                        case '<':
                            stringBuilder.Append("&lt;");
                            break;
                        case '>':
                            stringBuilder.Append("&gt;");
                            break;
                        default:
                            stringBuilder.AppendFormat("&#{0};", (object)num);
                            break;
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private uint[] StringToArrayOfUtf32Chars(string s)
        {
            byte[] bytes = Encoding.UTF32.GetBytes(s);
            uint[] instance = (uint[])Array.CreateInstance(typeof(uint), bytes.Length / 4);
            int startIndex = 0;
            int index = 0;
            while (startIndex < bytes.Length)
            {
                instance[index] = BitConverter.ToUInt32(bytes, startIndex);
                startIndex += 4;
                ++index;
            }
            return instance;
        }

        public dynamic MSTranslate1(string Input)
        {
            

            dynamic obj = new ExpandoObject();
            dynamic myTranslation = new ExpandoObject();
            //*** uncomment if rapid-api.
                var client = new RestClient("https://microsoft-translator-text.p.rapidapi.com/translate?to=REQUIRED&api-version=3.0&profanityAction=NoAction&textType=plain&to=ms,id,zh-Hans,th,fil,ja,ko,vi");
            //** end


            // uncomment if microsoft-api.
                //var client = new RestClient("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=ms,id,zh-Hans,th,fil,ja,ko,vi");
            //** end

            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");

            // uncomment if rapid-api.
                request.AddHeader("x-rapiapi-host", "microsoft-translator-text.p.rapidapi.com");
                request.AddHeader("x-rapidapi-key", txtAPI.Text.ToString());
            //** end

            // uncomment if microsoft-api.
                //request.AddHeader("Ocp-Apim-Subscription-Key", txtAPI.Text.ToString());
                //request.AddHeader("Ocp-Apim-Subscription-Region", "southeastasia");
            //** end

            string bdy = "[{ \"Text\":\""+Input +"\"}]";

            var body = bdy;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode.ToString() == "OK")	
            {
                obj = JsonConvert.DeserializeObject(response.Content);
                var myObj = JsonConvert.SerializeObject(obj[0]["translations"]);
                myTranslation = JsonConvert.DeserializeObject(myObj);
            }
            return myTranslation;
         }

        public string TranslateText(string input, string tranlang = "ms")
        {
            // Set the language from/to in the url (or pass it into this function)
            string url = String.Format
            ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
             "en", tranlang, Uri.EscapeUriString(input));

            HttpClient httpClient = new HttpClient();
            string result;
            try
            {
                 result = httpClient.GetStringAsync(url).Result;
            }
            catch
            {
                return "Google Not Available";
            }


            // Get all json data
            var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);

            // Extract just the first array element (This is the only data we are interested in)
            var translationItems = jsonData[0];

            // Translation Data
            string translation = "";

            // Loop through the collection extracting the translated objects
            foreach (object item in translationItems)
            {
                // Convert the item array to IEnumerable
                IEnumerable translationLineObject = item as IEnumerable;

                // Convert the IEnumerable translationLineObject to a IEnumerator
                IEnumerator translationLineString = translationLineObject.GetEnumerator();

                // Get first object in IEnumerator
                translationLineString.MoveNext();

                // Save its value (translated text)
                translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
            }

            // Remove first blank character
            if (translation.Length > 1) { translation = translation.Substring(1); };

            // Return translation
            return translation;
        }


    }

}
