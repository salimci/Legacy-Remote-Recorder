using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using CustomTools;
using DAL;
//using Natek.Recorders.Remote.Unified.Database.Oracle;


namespace RemoteRecorderTest
{
    static class SqlQueries
    {
        public static void InsertDefaultRemoteRecorderArgs()
        {
            using (var connection = new SqlConnection(Database.GetConnection(false, "Natekdb").ConnectionString))
            {
                connection.Open();

                var command = new SqlCommand(null, connection)
                {
                    CommandText = "INSERT INTO REMOTE_RECORDER " +
                                  "(SERVICENAME, RECORDERNAME, LOCATION, LASTLINE, LASTPOSITION, LASTKEYWORDS, FROMEND, MAXRECORDSEND," +
                                  "USERNAME, PASSWORD, REMOTEHOST, SLEEPTIME, TRACELEVEL, VIRTUALHOST, LASTUPDATED, MAXRESPONDTIME, EMAIL," +
                                  "CUSTOMVAR1, CUSTOMVAR2, DAL, STATUS, RELOAD, LASTRECDATE, LASTFILE, TIMEGAP, CHECKTIMESYNC, LASTRELOAD," +
                                  "TIMEZONE, TIMERANGE, LASTMAILDATE, MAILSUPRESS) " +
                                  "VALUES " + " (@serviceName, @recorderName, @location,@lastLine,@lastPosition, @lastKeywords, @fromEnd, @maxRecordSend," +
                                  "@user, @password, @remoteHost, @sleeptime, @traceLevel, @virtualhost, @lastUpdated, @maxRespondTime, @email," +
                                  "@custVar1, @custVar2, @dal, @status, @reload, @lastRecDate, @lastFile, @timeGap, @checktimeSync, @lastReload," +
                                  "@timeZone, @timeRange, @lastMailDate, @mailSupress)"
                };

                // Create and prepare an SQL statement. 
                var serviceNameParam = new SqlParameter("@serviceName", SqlDbType.VarChar, 250);
                var recorderNameParam = new SqlParameter("@recorderName", SqlDbType.VarChar, 250);
                var locationParam = new SqlParameter("@location", SqlDbType.VarChar, 250);
                var lastLineParam = new SqlParameter("@lastLine", SqlDbType.VarChar, 2000);
                var lastPositionParam = new SqlParameter("@lastPosition", SqlDbType.VarChar, 250);
                var lastKeywordParam = new SqlParameter("@lastKeywords", SqlDbType.VarChar, 500);
                var fromEndParam = new SqlParameter("@fromEnd", SqlDbType.Int);
                var maxRecorSentParam = new SqlParameter("@maxRecordSend", SqlDbType.Int);
                var userParam = new SqlParameter("@user", SqlDbType.VarChar, 250);
                var passwordParam = new SqlParameter("@password", SqlDbType.VarChar, 250);
                var remoteHostParam = new SqlParameter("@remoteHost", SqlDbType.VarChar, 250);
                var sleepTimeParam = new SqlParameter("@sleeptime", SqlDbType.Int);
                var traceLevelParam = new SqlParameter("@traceLevel", SqlDbType.Int);
                var virtualHostParam = new SqlParameter("@virtualHost", SqlDbType.VarChar, 250);
                var lastUpdatedParam = new SqlParameter("@lastUpdated", SqlDbType.DateTime);
                var maxRespondTimeParam = new SqlParameter("@maxRespondTime", SqlDbType.Int);
                var emailParam = new SqlParameter("@email", SqlDbType.VarChar, 500);
                var custVar1Param = new SqlParameter("@custVar1", SqlDbType.VarChar, 250);
                var custvar2Param = new SqlParameter("@custVar2", SqlDbType.Int);
                var dalParam = new SqlParameter("@dal", SqlDbType.VarChar, 250);
                var statusParam = new SqlParameter("@status", SqlDbType.Int);
                var reloadParam = new SqlParameter("@reload", SqlDbType.Int);
                var lastRecDateParam = new SqlParameter("@lastRecDate", SqlDbType.DateTime);
                var lastFileParam = new SqlParameter("@lastFile", SqlDbType.VarChar, 500);
                var timeGapParam = new SqlParameter("@timeGap", SqlDbType.Int);
                var checkTimeSyncParam = new SqlParameter("@checktimeSync", SqlDbType.Int);
                var lastReloadParam = new SqlParameter("@lastReload", SqlDbType.DateTime);
                var timeZoneParam = new SqlParameter("@timeZone", SqlDbType.Int);
                var timeRangeParam = new SqlParameter("@timeRange", SqlDbType.VarChar, 900);
                var lastMailDateParam = new SqlParameter("@lastMailDate", SqlDbType.DateTime);
                var mailSupressParam = new SqlParameter("@mailSupress", SqlDbType.Int);

                serviceNameParam.Value = Environment.MachineName;
                command.Parameters.Add(serviceNameParam);

               // recorderNameParam.Value = typeof(OracleUnifiedRecorder).Name;
                command.Parameters.Add(recorderNameParam);

              //  locationParam.Value = Path.Combine(@"C:\Users\burcu.coskun\Documents\testtmp\exchange\", typeof(OracleUnifiedRecorder).Name);
                locationParam.Value = "ORCL";
                command.Parameters.Add(locationParam);

                lastLineParam.Value = "";
                command.Parameters.Add(lastLineParam);

                lastPositionParam.Value = "0";
                command.Parameters.Add(lastPositionParam);

                lastKeywordParam.Value = "QUERY_1=\"SELECT ID AS CUSTOMINT6, ID AS RECORDNUM, FIRST_NAME AS CUSTOMSTR1, LAST_NAME AS CUSTOMSTR2, GATE_ID AS CUSTOMINT1, PASS_DATE AS DATE_TIME FROM KAPI_GECIS WHERE ID > @RECORDNUM_1\"";
                command.Parameters.Add(lastKeywordParam);

                fromEndParam.Value = 0;
                command.Parameters.Add(fromEndParam);

                maxRecorSentParam.Value = 1;
                command.Parameters.Add(maxRecorSentParam);

                userParam.Value = "testuser";
                command.Parameters.Add(userParam);

                passwordParam.Value = "Password12345";
                command.Parameters.Add(passwordParam);

                remoteHostParam.Value = "10.6.1.63";
                command.Parameters.Add(remoteHostParam);

                sleepTimeParam.Value = 30000;
                command.Parameters.Add(sleepTimeParam);

                traceLevelParam.Value = 4;
                command.Parameters.Add(traceLevelParam);

                virtualHostParam.Value = "test";
                command.Parameters.Add(virtualHostParam);

                lastUpdatedParam.Value = DateTime.Now;
                command.Parameters.Add(lastUpdatedParam);

                maxRespondTimeParam.Value = 1000000;
                command.Parameters.Add(maxRespondTimeParam);

                emailParam.Value = "email@email.com";
                command.Parameters.Add(emailParam);

                custVar1Param.Value = "";
                command.Parameters.Add(custVar1Param);

                custvar2Param.Value = "0";
                command.Parameters.Add(custvar2Param);

                dalParam.Value = "natekdb";
                command.Parameters.Add(dalParam);

                statusParam.Value = 1;
                command.Parameters.Add(statusParam);

                reloadParam.Value = 0;
                command.Parameters.Add(reloadParam);

                lastRecDateParam.Value = DateTime.Now;
                command.Parameters.Add(lastRecDateParam);

                lastFileParam.Value = "last file";
                command.Parameters.Add(lastFileParam);

                timeGapParam.Value = 0;
                command.Parameters.Add(timeGapParam);

                checkTimeSyncParam.Value = 0;
                command.Parameters.Add(checkTimeSyncParam);

                lastReloadParam.Value = DateTime.Now;
                command.Parameters.Add(lastReloadParam);

                timeZoneParam.Value = 0;
                command.Parameters.Add(timeZoneParam);

                timeRangeParam.Value = "time range";
                command.Parameters.Add(timeRangeParam);

                lastMailDateParam.Value = DateTime.Now;
                command.Parameters.Add(lastMailDateParam);

                mailSupressParam.Value = 0;
                command.Parameters.Add(mailSupressParam);

                // Call Prepare after setting the Commandtext and Parameters.
                command.Prepare();
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        public static void SetRecorderArgsValues(RecorderArgs pArgs)
        {
            using (var connection = new SqlConnection(Database.GetConnection(false, "Natekdb").ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand(null, connection)
                {
                    CommandText = "SELECT SERVICENAME, RECORDERNAME, LOCATION, LASTLINE, LASTPOSITION, LASTKEYWORDS, FROMEND, MAXRECORDSEND," +
                                  "USERNAME, PASSWORD, REMOTEHOST, SLEEPTIME, TRACELEVEL, VIRTUALHOST, LASTUPDATED, MAXRESPONDTIME, EMAIL," +
                                  "CUSTOMVAR1, CUSTOMVAR2, DAL, STATUS, RELOAD, LASTRECDATE, LASTFILE, TIMEGAP, CHECKTIMESYNC, LASTRELOAD," +
                                  "TIMEZONE, TIMERANGE, LASTMAILDATE, MAILSUPRESS " +
                                  "FROM REMOTE_RECORDER WHERE STATUS = 1 ORDER BY STATUS ASC"
                };

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        pArgs.ServiceName = dataReader.GetString(0);
                        pArgs.RecorderName = dataReader.GetString(1);
                        pArgs.Location = dataReader.GetString(2);
                        pArgs.LastLine = dataReader.GetString(3);
                        pArgs.LastPosition = dataReader.GetString(4);
                        pArgs.LastKeywords = dataReader.GetString(5);
                        pArgs.FromEndOnLoss = Convert.ToBoolean(dataReader.GetInt32(6));
                        pArgs.MaxRecordSend = dataReader.GetInt32(7);
                        pArgs.MaxLineToWait = dataReader.GetInt32(7);
                        pArgs.User = dataReader.GetString(8);
                        pArgs.Password = dataReader.GetString(9);
                        pArgs.RemoteHost = dataReader.GetString(10);
                        pArgs.SleepTime = dataReader.GetInt32(11);
                        pArgs.TraceLevel = dataReader.GetInt32(12);
                        pArgs.VirtualHost = dataReader.GetString(13);
                        pArgs.LastUpdated = dataReader.GetDateTime(14);
                        pArgs.MaxRespondTime = dataReader.GetInt32(15);
                        pArgs.Email = dataReader.GetString(16);
                        pArgs.CustomVar1 = dataReader.GetString(17);
                        pArgs.CustomVar2 = dataReader.GetInt32(18);
                        pArgs.Dal = dataReader.GetString(19);
                        pArgs.Status = dataReader.GetInt32(20);
                        pArgs.Reload = dataReader.GetInt32(21);
                        pArgs.LastRecDate = dataReader.GetDateTime(22);
                        pArgs.LastFile = dataReader.GetString(23);
                        pArgs.TimeGap = dataReader.GetInt32(24);
                        pArgs.CheckTimeSync = dataReader.GetInt32(25);
                        pArgs.LastReload = dataReader.GetDateTime(26);
                        pArgs.TimeZone = dataReader.GetInt32(27);
                        pArgs.TimeRange = dataReader.GetString(28);
                        pArgs.LastMailDate = dataReader.GetDateTime(29);
                        pArgs.MailSupress = dataReader.GetInt32(30);
                    }
                }
                connection.Close();
            }

        }

        public static void InsertOutput(CustomBase.Rec rec)
        {
            using (var connection = new SqlConnection(Database.GetConnection(false, "Natekdb").ConnectionString))
            {
                connection.Open();

                var command = new SqlCommand(null, connection)
                {
                    CommandText = "INSERT INTO RECORD " +
                                  "(EVENT_ID, RECORD_NUMBER, EVENTTYPE, EVENTCATEGORY, DATE_TIME, DESCRIPTION, SOURCENAME," +
                                  "COMPUTERNAME, USERSID, LOG_NAME, CUSTOMINT1, CUSTOMINT2, CUSTOMINT3, CUSTOMINT4, CUSTOMINT5, CUSTOMINT6," +
                                  "CUSTOMINT7, CUSTOMINT8, CUSTOMINT9, CUSTOMINT10, CUSTOMSTR1, CUSTOMSTR2, CUSTOMSTR3, CUSTOMSTR4, CUSTOMSTR5, CUSTOMSTR6," +
                                  "CUSTOMSTR7, CUSTOMSTR8, CUSTOMSTR9, CUSTOMSTR10) " +
                                  "VALUES " + " (@eventId, @recordNumber, @eventType, @eventCategory, @dateTime, @description, @sourcename," +
                                  "@computerName, @usersId, @logName, @customInt1, @customInt2, @customInt3, @customInt4, @customInt5, @customInt6," +
                                  "@customInt7, @customInt8, @customInt9, @customInt10, @customStr1, @customStr2, @customStr3, @customStr4, @customStr5, @customStr6," +
                                  "@customStr7, @customStr8, @customStr9, @customStr10)"
                };

                // Create and prepare an SQL statement. 
                var eventIdParam = new SqlParameter("@eventId", SqlDbType.BigInt);
                var recordNumberParam = new SqlParameter("@recordNumber", SqlDbType.BigInt);
                var eventTypeParam = new SqlParameter("@eventType", SqlDbType.VarChar, 900);
                var eventCategoryParam = new SqlParameter("@eventCategory", SqlDbType.VarChar, 900);
                var dateTimeParam = new SqlParameter("@dateTime", SqlDbType.DateTime);
                var descriptionParam = new SqlParameter("@description", SqlDbType.VarChar, 4000);
                var sourcenameParam = new SqlParameter("@sourcename", SqlDbType.VarChar, 900);
                var computerNameParam = new SqlParameter("@computerName", SqlDbType.VarChar, 900);
                var usersIdParam = new SqlParameter("@usersId", SqlDbType.VarChar, 900);
                var logNameParam = new SqlParameter("@logName", SqlDbType.VarChar, 900);
                var customInt1Param = new SqlParameter("@customInt1", SqlDbType.Int);
                var customInt2Param = new SqlParameter("@customInt2", SqlDbType.Int);
                var customInt3Param = new SqlParameter("@customInt3", SqlDbType.Int);
                var customInt4Param = new SqlParameter("@customInt4", SqlDbType.Int);
                var customInt5Param = new SqlParameter("@customInt5", SqlDbType.Int);
                var customInt6Param = new SqlParameter("@customInt6", SqlDbType.BigInt);
                var customInt7Param = new SqlParameter("@customInt7", SqlDbType.BigInt);
                var customInt8Param = new SqlParameter("@customInt8", SqlDbType.BigInt);
                var customInt9Param = new SqlParameter("@customInt9", SqlDbType.BigInt);
                var customInt10Param = new SqlParameter("@customInt10", SqlDbType.BigInt);
                var customStr1Param = new SqlParameter("@customStr1", SqlDbType.VarChar, 900);
                var customStr2Param = new SqlParameter("@customStr2", SqlDbType.VarChar, 900);
                var customStr3Param = new SqlParameter("@customStr3", SqlDbType.VarChar, 900);
                var customStr4Param = new SqlParameter("@customStr4", SqlDbType.VarChar, 900);
                var customStr5Param = new SqlParameter("@customStr5", SqlDbType.VarChar, 900);
                var customStr6Param = new SqlParameter("@customStr6", SqlDbType.VarChar, 900);
                var customStr7Param = new SqlParameter("@customStr7", SqlDbType.VarChar, 900);
                var customStr8Param = new SqlParameter("@customStr8", SqlDbType.VarChar, 900);
                var customStr9Param = new SqlParameter("@customStr9", SqlDbType.VarChar, 900);
                var customStr10Param = new SqlParameter("@customStr10", SqlDbType.VarChar, 900);

                eventIdParam.Value = rec.EventId;
                command.Parameters.Add(eventIdParam);

                recordNumberParam.Value = rec.Recordnum;
                command.Parameters.Add(recordNumberParam);

                eventTypeParam.Value = rec.EventType ?? "";
                command.Parameters.Add(eventTypeParam);

                eventCategoryParam.Value = rec.EventCategory ?? "";
                command.Parameters.Add(eventCategoryParam);

                DateTime parsedDate = DateTime.ParseExact(rec.Datetime, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                dateTimeParam.Value = parsedDate;
                command.Parameters.Add(dateTimeParam);

                descriptionParam.Value = rec.Description ?? "";
                command.Parameters.Add(descriptionParam);

                sourcenameParam.Value = rec.SourceName ?? "";
                command.Parameters.Add(sourcenameParam);

                computerNameParam.Value = rec.ComputerName ?? "";
                command.Parameters.Add(computerNameParam);

                usersIdParam.Value = rec.UserName ?? "";
                command.Parameters.Add(usersIdParam);

                logNameParam.Value = rec.LogName ?? "";
                command.Parameters.Add(logNameParam);

                customInt1Param.Value = rec.CustomInt1;
                command.Parameters.Add(customInt1Param);

                customInt2Param.Value = rec.CustomInt2;
                command.Parameters.Add(customInt2Param);

                customInt3Param.Value = rec.CustomInt3;
                command.Parameters.Add(customInt3Param);

                customInt4Param.Value = rec.CustomInt4;
                command.Parameters.Add(customInt4Param);

                customInt5Param.Value = rec.CustomInt5;
                command.Parameters.Add(customInt5Param);

                customInt6Param.Value = rec.CustomInt6;
                command.Parameters.Add(customInt6Param);

                customInt7Param.Value = rec.CustomInt7;
                command.Parameters.Add(customInt7Param);

                customInt8Param.Value = rec.CustomInt8;
                command.Parameters.Add(customInt8Param);

                customInt9Param.Value = rec.CustomInt9;
                command.Parameters.Add(customInt9Param);

                customInt10Param.Value = rec.CustomInt10;
                command.Parameters.Add(customInt10Param);

                customStr1Param.Value = rec.CustomStr1 ?? "";
                command.Parameters.Add(customStr1Param);

                customStr2Param.Value = rec.CustomStr2 ?? "";
                command.Parameters.Add(customStr2Param);

                customStr3Param.Value = rec.CustomStr3 ?? "";
                command.Parameters.Add(customStr3Param);

                customStr4Param.Value = rec.CustomStr4 ?? "";
                command.Parameters.Add(customStr4Param);

                customStr5Param.Value = rec.CustomStr5 ?? "";
                command.Parameters.Add(customStr5Param);

                customStr6Param.Value = rec.CustomStr6 ?? "";
                command.Parameters.Add(customStr6Param);

                customStr7Param.Value = rec.CustomStr7 ?? "";
                command.Parameters.Add(customStr7Param);

                customStr8Param.Value = rec.CustomStr8 ?? "";
                command.Parameters.Add(customStr8Param);

                customStr9Param.Value = rec.CustomStr9 ?? "";
                command.Parameters.Add(customStr9Param);

                customStr10Param.Value = rec.CustomStr10 ?? "";
                command.Parameters.Add(customStr10Param);

                // Call Prepare after setting the Commandtext and Parameters.
                command.Prepare();
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        public static void DeleteAllRows(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException("tableName");
            using (var connection = new SqlConnection(Database.GetConnection(false, "Natekdb").ConnectionString))
            {
                connection.Open();

                var command = new SqlCommand(null, connection)
                {
                    CommandText = "DELETE FROM " + tableName
                };
                // Call Prepare after setting the Commandtext and Parameters.
                command.Prepare();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static bool IsTableEmpty(string tableName)
        {
            var isEmpty = true;
            var count = 0;

            if (tableName == null) throw new ArgumentNullException("tableName");
            using (var connection = new SqlConnection(Database.GetConnection(false, "Natekdb").ConnectionString))
            {
                connection.Open();

                var command = new SqlCommand(null, connection)
                {
                    CommandText = "SELECT COUNT(*) FROM " + tableName
                };

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        count = dataReader.GetInt32(0);
                    }

                    if (count > 0)
                        isEmpty = false;
                }
                // Call Prepare after setting the Commandtext and Parameters.

                connection.Close();
            }

            return isEmpty;
        }

        public static void SetReg(int Identity, string LastPosition, string LastLine, string LastFile,
            string LastKeywords, string LastRecDate)
        {
            using (var connection = new SqlConnection(Database.GetConnection(false, "Natekdb").ConnectionString))
            {
                connection.Open();

                using (var cmd = new SqlCommand(@"UPDATE REMOTE_RECORDER 
                                                SET LASTPOSITION = @lastPosition ,LASTLINE = @lastLine, 
                                                LASTFILE = @lastFile, LASTKEYWORDS = @lastKeywords, LASTRECDATE = @lastRecDate 
                                                WHERE ID = @identity", connection))
                {
                    cmd.Parameters.Add("@lastPosition", SqlDbType.VarChar, int.MaxValue).Value = LastPosition;
                    cmd.Parameters.Add("@lastLine", SqlDbType.VarChar, int.MaxValue).Value = LastLine;
                    cmd.Parameters.Add("@lastFile", SqlDbType.VarChar, int.MaxValue).Value = LastFile;
                    cmd.Parameters.Add("@lastKeywords", SqlDbType.VarChar, int.MaxValue).Value = LastKeywords;
                    if (String.IsNullOrEmpty(LastRecDate))
                        cmd.Parameters.Add("@lastRecDate", SqlDbType.DateTime).Value = DBNull.Value;
                    else
                        cmd.Parameters.Add("@lastRecDate", SqlDbType.DateTime).Value = DateTime.ParseExact(LastRecDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@identity", SqlDbType.BigInt).Value = Identity;

                    cmd.ExecuteNonQuery();

                }

            }

        }
    }
}
