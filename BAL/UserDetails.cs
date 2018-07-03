﻿using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using static BAL.JsonMember;

namespace BAL
{
    public class UserDetails
    {

        DBManager Sqldbmanager = new DBManager(DataProvider.SqlServer, ConfigurationManager.ConnectionStrings["E_wallet"].ConnectionString);
        DataSet DS = new DataSet();
        DataTable dt = new DataTable();
        Email objEMail = new Email();
        bool SmsResult = false;
        bool MailResult = false;
        public DataSet LogError(string ModuleName, string ErrorSource, string Description)
        {
            try
            {
                Sqldbmanager.Open();
                Sqldbmanager.CreateParameters(3);
                Sqldbmanager.AddParameters(0, "@ModuleName", ModuleName.Trim());
                Sqldbmanager.AddParameters(1, "@ErroSource", ErrorSource.Trim());
                Sqldbmanager.AddParameters(2, "@Description", Description.Trim());
                DS = Sqldbmanager.ExecuteDataSet(CommandType.StoredProcedure, "usp_LogError");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Sqldbmanager.Close();
            }
            return DS;
        }

        public Object LoginMaster(JsonMember.UserDetails obj)
        {
            LoginReturn Lobj = new LoginReturn();
            DataTable Logindt = new DataTable();
            try
            {
                Sqldbmanager.Open();
                Sqldbmanager.CreateParameters(1);
                Sqldbmanager.AddParameters(0, "@LoginId", obj.LoginId);
                DS = Sqldbmanager.ExecuteDataSet(CommandType.StoredProcedure, "USP_LoginMaster");
                if (DS.Tables[1].Rows.Count > 0)
                {
                    if (DS.Tables[0].Rows[0]["UserPassword"].ToString() == obj.Password)
                    {
                        
                        Lobj = new LoginReturn()
                        {
                            flag = "true",
                            Message = "Success",
                            EmailId = DS.Tables[1].Rows[0]["EmailId"].ToString(),
                            MobileNo = DS.Tables[1].Rows[0]["MobileNo"].ToString(),
                            Name = DS.Tables[1].Rows[0]["Name"].ToString(),
                            UserName = DS.Tables[1].Rows[0]["UserName"].ToString(),
                            UserId = DS.Tables[1].Rows[0]["UserId"].ToString()
                        };
                        
                    }
                    else
                    {
                        Lobj = new LoginReturn()
                        {
                            flag = "false",
                            Message = "Invalid Password",
                            EmailId = "",
                            MobileNo = "",
                            Name = "",
                            UserId=""
                        };
                        
                    }
                }
                else
                {
                    Lobj = new LoginReturn()
                    {
                        flag = "false",
                        Message = "Invalid Email/Mobile",
                        EmailId = "",
                        MobileNo = "",
                        Name = "",
                        UserId = ""
                    };
                    
                }
            }
            catch (Exception Ex)
            {
                Lobj = new LoginReturn()
                {
                    flag = "false",
                    Message = "Invalid Email/Mobile",
                    EmailId = "",
                    MobileNo = "",
                    Name = "",
                    UserId = ""
                };
                
            }
            finally
            {
                Sqldbmanager.Close();
            }
            return Lobj;
        }

        public object GenerateUser(JsonMember.UserDetails obj)
        {
            registrationReturn obj1 = new registrationReturn();
            try
            {
                Sqldbmanager.Open();
                Sqldbmanager.CreateParameters(5);
                Sqldbmanager.AddParameters(0, "@Name", obj.Name);
                Sqldbmanager.AddParameters(1, "@UserName", obj.UserName);
                Sqldbmanager.AddParameters(2, "@UserPassword", obj.Password);
                Sqldbmanager.AddParameters(3, "@EmailId", obj.EmailId);
                Sqldbmanager.AddParameters(4, "@MobileNo", obj.MobileNo);
                DS = Sqldbmanager.ExecuteDataSet(CommandType.StoredProcedure, "USP_GenerateUser");
                if (Convert.ToBoolean(DS.Tables[0].Rows[0]["flag"]) == true)
                {

                    Thread thrdSms = new Thread(() => SmsResult = (new Email()).SendSMS(obj.MobileNo, DS.Tables[0].Rows[0]["OTP"].ToString() + " is your flipprr verification code."));
                    thrdSms.Start();

                    //Thread thrdMail = new Thread(() => MailResult = (new Email()).sendMail(obj.EmailId, "", "Flipprr Verification Code", DS.Tables[0].Rows[0]["OTP"].ToString(), ""));
                    //thrdMail.Start();

                }
            }
            catch (Exception Ex)
            {
                DS = LogError("Generate User", Ex.Message.ToString(), "SP Name: USP_GenerateUser");
                obj1 = new registrationReturn()
                {
                    flag = "false",
                    Message = DS.Tables[0].Rows[0]["Meaasge"].ToString(),
                    OTPId = "",
                    OTP = "",
                    UserId = ""
                };
            }
            finally
            {
                Sqldbmanager.Close();
            }
           obj1 = new registrationReturn()
            {
                flag = DS.Tables[0].Rows[0]["flag"].ToString(),
                Message = DS.Tables[0].Rows[0]["Message"].ToString(),
                OTPId = DS.Tables[0].Rows[0]["OTPId"].ToString(),
                OTP = DS.Tables[0].Rows[0]["OTP"].ToString(),
                UserId = DS.Tables[0].Rows[0]["UserId"].ToString()
            };
            return obj1;
        }

        public object ValidateOTP(JsonMember.UserDetails obj)
        {
            registrationReturn obj1 = new registrationReturn();
            try
            {
                Sqldbmanager.Open();
                Sqldbmanager.CreateParameters(2);
                Sqldbmanager.AddParameters(0, "@OTPId", obj.OTPId);
                Sqldbmanager.AddParameters(1, "@OTP", obj.OTP);
                DS = Sqldbmanager.ExecuteDataSet(CommandType.StoredProcedure, "USP_ValidateOTP");
               
            }
            catch (Exception Ex)
            {
                DS = LogError("Validate OTP", Ex.Message.ToString(), "SP Name: USP_ValidateOTP");
                obj1 = new registrationReturn()
                {
                    flag = "false",
                    Message = DS.Tables[0].Rows[0]["Meaasge"].ToString(),
                    OTPId = "",
                    OTP = "",
                    UserId = ""
                };
            }
            finally
            {
                Sqldbmanager.Close();
            }
            obj1 = new registrationReturn()
            {
                flag = DS.Tables[0].Rows[0]["flag"].ToString(),
                Message = DS.Tables[0].Rows[0]["Message"].ToString(),
                OTPId = DS.Tables[0].Rows[0]["OTPId"].ToString(),
                OTP = DS.Tables[0].Rows[0]["OTP"].ToString(),
                UserId = DS.Tables[0].Rows[0]["UserId"].ToString()
            };
            return obj1;
        }

        public object ResendOTP(JsonMember.UserDetails obj)
        {
            registrationReturn obj1 = new registrationReturn();
            try
            {
                Sqldbmanager.Open();
                Sqldbmanager.CreateParameters(1);
                Sqldbmanager.AddParameters(0, "@OTPId", obj.OTPId);
                DS = Sqldbmanager.ExecuteDataSet(CommandType.StoredProcedure, "USP_ResendOTP");
                if (Convert.ToBoolean(DS.Tables[0].Rows[0]["flag"]) == true)
                {

                    Thread thrdSms = new Thread(() => SmsResult = (new Email()).SendSMS(obj.MobileNo, DS.Tables[0].Rows[0]["OTP"].ToString() + " is your flipprr verification code."));
                    thrdSms.Start();

                    //Thread thrdMail = new Thread(() => MailResult = (new Email()).sendMail(obj.EmailId, "", "Flipprr Verification Code", DS.Tables[0].Rows[0]["OTP"].ToString(), ""));
                    //thrdMail.Start();

                }
            }
            catch (Exception Ex)
            {
                DS = LogError("Resend OTP", Ex.Message.ToString(), "SP Name: USP_ResendOTP");
                obj1 = new registrationReturn()
                {
                    flag = "false",
                    Message = DS.Tables[0].Rows[0]["Meaasge"].ToString(),
                    OTPId = "",
                    OTP = "",
                    UserId = ""
                };
            }
            finally
            {
                Sqldbmanager.Close();
            }
            obj1 = new registrationReturn()
            {
                flag = DS.Tables[0].Rows[0]["flag"].ToString(),
                Message = "",
                OTPId = DS.Tables[0].Rows[0]["OTPId"].ToString(),
                OTP = DS.Tables[0].Rows[0]["OTP"].ToString(),
                UserId = DS.Tables[0].Rows[0]["UserId"].ToString()
            };
            return obj1;
        }

      


    }
}