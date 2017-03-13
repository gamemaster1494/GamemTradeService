/////SteamRep
/////User ID <Primary key>
/////Status <Text> SCAMMER CAUTION or NONE
/////Last Checked <date/time>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SteamKit2;
//using SteamTrade;
//using System.Threading;
//using Newtonsoft.Json;
//using MySql.Data.MySqlClient;
//using System.Diagnostics;
//using System.IO;
//using System.Globalization;

//namespace SteamBot
//{
//    public static class SteamRepDatabase
//    {
//        static string MyConnectionString = "SERVER=localhost;DATABASE=steambot;UID=gamem_steam_botz;PASSWORD=learncomputers101;";//Connection STring



//        public static bool AddSteamRep(SteamID OtherSID, string sSummary)
//        {

//            string query = "INSERT INTO steamrep (UserID,Status,DateLastChecked) VALUES (\'" + OtherSID.ConvertToUInt64() + "\',\'" + sSummary + "\',\'" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\')";//Set up command
//            MySqlConnection connection = new MySqlConnection(MyConnectionString);

//            bool bSuccess = false;
//            try
//            {
//                connection.Open();
//                MySqlCommand cmd = new MySqlCommand(query, connection);
//                cmd.ExecuteNonQuery();//send command without wanting a query back
//                bSuccess = true;
//            }//try
//            catch (Exception ex)
//            {
//                bSuccess = false;
//                Console.WriteLine(ex.Message);//show error
//            }//catch
//            finally
//            {
//                connection.Close();//close connection NO MATTER WHAT
//            }//finally
//            return bSuccess;
//        }//AddSteamRep(SteamID OtherSID, string sInfo)

//        public static bool UpdateSteamRepInfo(SteamID OtherSID, string sSummary)
//        {
//            MySqlConnection connection = new MySqlConnection(MyConnectionString);
//            string query = "UPDATE steamrep SET DateLastChecked=\'" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\', Status=\'" + sSummary + "\' WHERE UserID=\'" + OtherSID.ConvertToUInt64() + "\'";

//            bool bSuccess = false;
//            try
//            {
//                connection.Open();
//                MySqlCommand cmd = new MySqlCommand(query, connection);
//                cmd.ExecuteNonQuery();
//                bSuccess = true;
//            }
//            catch (Exception ex)
//            {
//                bSuccess = false;
//                Console.WriteLine(ex.Message);
//            }
//            finally
//            {
//                connection.Close();
//            }
//            return bSuccess;
//        }

//        public static string GetSteamRepInfo(SteamID OtherSID)
//        {
//            string sInfo = "NULL";
//            MySqlConnection connection = new MySqlConnection(MyConnectionString);
//            string query = "SELECT Status from steamrep WHERE UserID =" + OtherSID.ConvertToUInt64();

//            try
//            {
//                connection.Open();
//                MySqlCommand cmd = new MySqlCommand(query, connection);
//                MySqlDataReader dataReader = cmd.ExecuteReader();
//                while (dataReader.Read())
//                {
//                    sInfo = dataReader["Status"].ToString();
//                }
//                dataReader.Close();
//            }
//            catch (Exception ex)
//            {
//                sInfo = "NULL";
//                Console.WriteLine(ex.Message);
//            }
//            finally
//            {
//                connection.Close();
//            }


//            return sInfo;
//        }

//        //Backup
//        public static void Backup()
//        {
//            try
//            {
//                DateTime Time = DateTime.Now;
//                int year = Time.Year;
//                int month = Time.Month;
//                int day = Time.Day;
//                int hour = Time.Hour;
//                int minute = Time.Minute;
//                int second = Time.Second;
//                int millisecond = Time.Millisecond;

//                //Save file to C:\ with the current date as a filename
//                string path;
//                path = "C:\\MySqlBackup" + year + "-" + month + "-" + day +
//            "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
//                StreamWriter file = new StreamWriter(path);


//                ProcessStartInfo psi = new ProcessStartInfo();
//                psi.FileName = "mysqldump";
//                psi.RedirectStandardInput = false;
//                psi.RedirectStandardOutput = true;
//                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
//                    "gamem_steam_botz", "learncomputers101", "localhost", "steambot");
//                psi.UseShellExecute = false;
//                Process process = Process.Start(psi);

//                string output;
//                output = process.StandardOutput.ReadToEnd();
//                file.WriteLine(output);
//                process.WaitForExit();
//                file.Close();
//                process.Close();
//            }
//            catch (IOException ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }

//        //Restore
//        public static void Restore()
//        {
//            try
//            {
//                //Read file from C:\
//                string path;
//                path = "C:\\MySqlBackup.sql";
//                StreamReader file = new StreamReader(path);
//                string input = file.ReadToEnd();
//                file.Close();

//                ProcessStartInfo psi = new ProcessStartInfo();
//                psi.FileName = "mysql";
//                psi.RedirectStandardInput = true;
//                psi.RedirectStandardOutput = false;
//                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
//                    "gamem_steam_botz", "learncomputers101", "localhost", "steamrep");
//                psi.UseShellExecute = false;


//                Process process = Process.Start(psi);
//                process.StandardInput.WriteLine(input);
//                process.StandardInput.Close();
//                process.WaitForExit();
//                process.Close();
//            }
//            catch (IOException ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }

//    }//class
//}//namespace
