using System;
using System.Collections.Generic;

namespace NatekLogService
{
    /// <summary>
    /// Sabit değerlerin yazıldığı sınıf.
    /// SQL cümlecikleri vs...
    /// </summary>
    public static class Rsc
    {
        public static String SubKey = @"SOFTWARE\NATEK\HMDB";
        public static String DalNameSql = "HMDBSql"; 

        public static String SubKey2 = @"SOFTWARE\NATEK\DAL\HMDBOra";
        public static String DalNameOra = "HMDBOra";


        public static String LogTbl = " RECORD_HMBS";
        public static String LogTbl_EventCateg = "EVENTCATEGORY";
        public static String LogTbl_WindowName = "CUSTOMSTR2";
        public static String LogTbl_DateTime = "DATE_TIME";
        public static String LogTbl_Description = "DESCRIPTION";
        public static String LogTbl_SrcName = "SOURCENAME";
        public static String LogTbl_CompName = "COMPUTERNAME";
        public static String LogTbl_UserName = "USERSID";
        public static String LogTbl_CusStr2 = "CUSTOMSTR2";
        public static String LogTbl_PreIma = "CUSTOMSTR3";
        public static String LogTbl_PostIma = "CUSTOMSTR4";
        public static String LogTbl_DataWindowName = "CUSTOMSTR5";
        public static String LogTbl_PriKeyVal = "CUSTOMSTR7";
        public static String LogTbl_RecordNumber = "RECORD_NUMBER";
        public static String LogTbl_ID= "ID";

        /// <summary>
        /// "EXTERNAL_ALERT_COLUMNS" a eklenmeyecek kolon isimleri 
        /// Kullanılmayacak yeni kolon ismi için "-" işaretinden sonra kolon ismi girin
        /// </summary>
        public static String NonUsedColNames = "-ID-EVENT_ID-RECORD_NUMBER-EVENTTYPE-PREIMAGE-POSTIMAGE-FULL_LOGS-DATE_TIME-PRIMARYKEYVALUE-" +
                                               "CUSTOMINT1-CUSTOMINT2-CUSTOMINT3-CUSTOMINT4-CUSTOMINT5-CUSTOMINT6-CUSTOMINT7-CUSTOMINT8-CUSTOMINT9-CUSTOMINT10-" +
                                               "CUSTOMSTR1-CUSTOMSTR3-CUSTOMSTR4-CUSTOMSTR6-CUSTOMSTR8-CUSTOMSTR9-CUSTOMSTR10" +
                                               "SIGN-SIGN_TIME-SEVERITY-TAXONOMY-DESCRIPTION-";

        /// <summary>
        /// "EXTERNAL_ALERT_ACTIONTYPE" tablosu 
        /// </summary>
        public static String ActionTbl = "EXTERNAL_ALERT_ACTIONTYPE";
        public static String ActionTbl_ID = "ID";
        public static String ActionTbl_ActionName = "ACTIONNAME";

        /// <summary>
        /// "EXTERNAL_ALERT_FILTERS" tablosu 
        /// </summary>
        public static String FiltersTbl = "EXTERNAL_ALERT_FILTERS";
        public static String Filters_ID = "ID";
        public static String Filters_FilterName = "FILTERNAME";
        public static String Filters_UsedFunc = "USEDFUNCTION";
        public static String Filters_ActType = "ACTIONTYPE";
        public static String Filters_RunTime = "RUNTIME";
        public static String Filters_LastRunTime = "LASTRUNTIME";
        public static String Filters_LastPos = "LASTPOSITION";
        public static String Filters_Target = "TARGET";
        public static String Filters_Desc = "DESCRIPTION";
        public static String Filters_TableName = "TABLENAME";

        /// <summary>
        /// "EXTERNAL_ALERT_COLUMNS" tablosu 
        /// </summary>
        public static String ColumnTbl = "EXTERNAL_ALERT_COLUMNS";
        public static String ColumnTbl_Name = "COLUMNNAME";
        public static String ColumnTbl_Content = "COLUMNCONTENT ";

        /// <summary>
        /// "EXTERNAL_ALERT_COLUMNS_CONSTANTS" tablosu 
        /// </summary>
        public static String Columns_ConstantsTbl = "EXTERNAL_ALERT_COLUMNS_CONSTANTS";
        public static String Columns_Constants_FilterID = "FILTERID";
        public static String Columns_Constants_ColumnName = "COLUMNNAME";
        public static String Columns_Constants_Constant = "CONSTANT";

        /// <summary>
        /// "EXTERNAL_ALERT_CONFIGURATION" tablosu 
        /// </summary>
        public static String Configuration = " EXTERNAL_ALERT_CONFIGURATION ";
        public static String Configuration_Runtime = " RUNTIME ";
        public static String Configuration_Period = " PERIOD ";
        public static String Configuration_DataBaseType = " DATABASETYPE ";

        /// <summary>
        /// "EXTERNAL_ALERT_ACTION_MSG" tablosu
        /// </summary>
        public static String ActionMsgTbl = " EXTERNAL_ALERT_ACTION_MSG ";
        public static String ActionMsg_ID = " ID ";
        public static String ActionMsg_FilterName = " FILTERNAME ";
        public static String ActionMsg_DateTime = " DATE_TIME";
        public static String ActionMsg_Category = " CATEGORY ";
        public static String ActionMsg_UserName = " USERNAME ";
        public static String ActionMsg_ComputerName = " COMPUTERNAME ";
        public static String ActionMsg_WindowName = " WINDOWNAME ";
        public static String ActionMsg_PrimaryKeyValue = " PRIMARYKEYVALUE ";
        public static String ActionMsg_DataWindowName = " DATAWINDOWNAME ";
        public static String ActionMsg_RecordNumber = " RECORDNUMBER ";
        public static String ActionMsg_AlertTime = " ALERT_TIME ";
        public static String ActionMsg_Description = " DESCRIPTION ";

        public static Int32 LabelPositionX = 30;
        public static String PanelLeft = "panel1";
        public static String PanelRight = "panel2";

        public static Int32 TimerInterval = 5000;

        public static String StoredPrcBoolVal = "RETURNVALUE";
        public static String StoredPrcDescVal = "DESCRIPTION";

    }
}
