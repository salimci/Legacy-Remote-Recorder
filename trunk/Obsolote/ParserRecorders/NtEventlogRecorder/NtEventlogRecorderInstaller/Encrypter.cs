using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Log;

namespace NtEventlogRecorderInstaller
{
    class Encrypter
    {
        public static string Eyncrypt(string Password, byte f, byte s, byte t)
        {
            try
            {
                DES DESalg = DES.Create("DES");

                /******* initializing keys with he predefined values *******/
                byte[] Key = { 216, 250, 130, 174, 71, 152, 5, 160 };
                byte[] IV =  { f, s, t, 53, 74, 233, 137, 18 };

                DESalg.Key = Key;
                DESalg.IV = IV;

                MemoryStream mMemStr = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mMemStr,
                    DESalg.CreateEncryptor(Key, IV),
                    CryptoStreamMode.Write);

                StreamWriter mStr = new StreamWriter(cStream, Encoding.GetEncoding(1254));
                mStr.Write(Password);
                mStr.Flush();
                cStream.FlushFinalBlock();

                byte[] mBytes = new byte[mMemStr.Length];

                mMemStr.Position = 0;
                mMemStr.Read(mBytes, 0, Convert.ToInt32(mMemStr.Length));

                //string pass = Encoding.GetEncoding(1252).GetString(mBytes);
                string pass = Convert.ToBase64String(mBytes);

                mMemStr.Close();
                mStr.Close();
                cStream.Close();
                return pass;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string Decyrpt(string password, byte f, byte s, byte t)
        {
            /******* initializing keys with he predefined values *******/
            byte[] Key = { 216, 250, 130, 174, 71, 152, 5, 160 };
            byte[] IV =  { f, s, t, 53, 74, 233, 137, 18 };
            byte[] mBytes;
            MemoryStream mMemStr = null;
            CryptoStream cStream = null;
            StreamReader sReader = null;
            DES DESalg = null;
            try
            {
                DESalg = DES.Create("DES");

                DESalg.Key = Key;
                DESalg.IV = IV;
                /******* initializing keys with he predefined values *******/
                mMemStr = new MemoryStream();
                mBytes = Convert.FromBase64String(password);
                //mBytes = Encoding.GetEncoding(1252).GetBytes(password);
                mMemStr.Write(mBytes, 0, mBytes.Length);
                mMemStr.Flush();
                mMemStr.Position = 0;

                cStream = new CryptoStream(mMemStr,
                    DESalg.CreateDecryptor(Key, IV),
                    CryptoStreamMode.Read);

                // Read the data from the stream 
                // to decrypt it.
                sReader = new StreamReader(cStream, Encoding.GetEncoding(1254));
                string data = sReader.ReadToEnd();
                return data;
            }
            catch (Exception er)
            {
                Logger.Log(LogType.EVENTLOG, LogLevel.DEBUG, "Encrypt Error " + er.Message);
                return null;
            }
            finally
            {
                sReader.Close();
                cStream.Close();
                mMemStr.Close();
                IV = null; Key = null; mBytes = null;
                DESalg.Clear();
            }
        }
    }
}
