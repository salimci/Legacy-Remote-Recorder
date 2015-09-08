using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using System.IO.Compression;
using Parser;
using Log;
using CustomTools;
using Microsoft.Win32;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Threading;

namespace Parser
{
    public class MicrosoftIASServerRecorder : Parser
    {
        public MicrosoftIASServerRecorder()
        : base()
        {
            LogName = "MicrosoftIASServerRecorder";
            enc = Encoding.UTF8;
        }

        public override void Init()
        {
            GetFiles();
        }

        public MicrosoftIASServerRecorder(String fileName)
            : base(fileName)
        {

        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {   
            dayChangeTimer.Enabled = false;
            if (remoteHost == "")
            {
                String fileLast = FileName;
                ParseFileName();
                if (FileName != fileLast)
                {
                    Stop();
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "  MicrosoftIASServerRecorder in dayChangeTimer_Elapsed() -->> File Changed, New File Is : " + FileName);
                }
                else
                {
                    FileInfo fi = new FileInfo(FileName);
                    if (fi.Length - 1 > Position)
                    {
                        Stop();
                        Start();
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in dayChangeTimer_Elapsed() -->> Day Change Timer File Is : " + FileName);
                }
                dayChangeTimer.Enabled = true;
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {
            base.SetConfigData(Identity, Location, LastLine, LastPosition, LastFile, LastKeywords, FromEndOnLoss
            , MaxLineToWait, User, Password, RemoteHost, SleepTime, TraceLevel, CustomVar1, CustomVar2, virtualhost
            , dal, Zone);
            FileName = LastFile;
        }

        public override void GetFiles()
        {
            try
            {
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception e)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Using Registry is " + usingRegistry.ToString());
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: reg is not null " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }

        public override void Start()
        {
            base.Start();
        }

        public override bool ParseSpecific(String line, bool dontSend) // string parçalama
        {   
            line = line.Trim();
            line = line.Replace("\0", "");

            if (line == "" || line == " ")
                return true;

            if (!dontSend)
            {
                try
                {
                    Rec rRec = new Rec();
                    rRec.LogName = LogName;
                    rRec = str_Paracala(line, rRec);
                    SetRecordData(rRec);
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "    MicrosoftIASServerRecorder In ParseSpecific() -->> " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "    MicrosoftIASServerRecorder In ParseSpecific() -->> " + e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "    MicrosoftIASServerRecorder In ParseSpecific() -->> " + " Line : " + line);
                    return true;
                }
            }

            return true;
        }
         
        public string returnAttribute(string id,string value) 
        {
            // 107 ile 255 arasý eklenmedi
            int index;
            string attribute = "";
            string attributeName = "";
            string attributeValue = "";

            string[] ID = {"1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","18","19","20","22","23","24","25",
                           "26","27","28","29","30","31","32","33","34","35","36","37", "38","39","40","41","42","43","44","45","46","47","48","49","50", 
                           "51","52","53","55","60","61","62","63","64","65","66","67","68","69","70","71","72","73","74","75","76","77","78","79","80",	
                           "81","82","83","85","86","87","88","90","91","95","96","97","98","99","100","4108","4116", "4121","4127","4128","4129","4130",
                           "4132","4136","4142","4149"
                           };

            string[] Attributes = { "User-Name","User-Password","CHAP-Password","NAS-IP-Address","NAS-Port",
                                    "Service-Type,1:Login,2:Framed,3:Callback Login,4:Callback Framed,5:Outbound,6:Administrative,7:NAS Prompt,8:Authenticate Only,9:Callback NAS Prompt",
                                    "Framed-Protocol,1:PPP,2:SLIP,3:AppleTalk Remote Access Protocol (ARAP),4:Gandalf Proprietary SingleLink/MultiLink protocol,5:Xylogics proprietary IPX/SLIP,6:X.75 Synchronous,257:EURAW,258:EUUI,259:X25,260:COMB,261:FR",
                                    "Framed-IP-Address","Framed-IP-Netmask","Framed-Routing,0:None,1:Send,2:Listen,3:Send-Listen","Filter-ID","Framed-MTU",
                                    "Framed-Compression,0:None,1:Van Jacobson TCP/IP header compression,2:IPX Header compression,3:Stac-LZS compression",
                                    "Login-IP-Host", 
                                    "Login-Service,0:Telnet,1:Rlogin,2:TCP Clear,3:Portmaster (proprietary),4:LAT,5:X25-PAD,6:X25-T3POS,8:TCP Clear Quiet (suppresses any NAS-generated connect string)", 
                                    "Login-TCP-Port","Reply-Message","Callback-Number","Callback-ID","Framed-Route","Framed-IPX-Network","State","Class",	
                                    "Vendor-Specific","Session-Timeout","Idle-Timeout","Termination-Action,0:Default,1:RADIUS-Request","Called-Station-ID","Calling-Station-ID", 	 
                                    "NAS-Identifier","Proxy-State","Login-LAT-Service","Login-LAT-Node", "Login-LAT-Group","Framed-AppleTalk-Link","Framed-AppleTalk-Network",	
                                    "Framed-AppleTalk-Zone", 	
                                    "Acct-Status-Type,1:Start,2:Stop,3:Interim Update,7:Accounting-On,8:Accounting-Off,9:Tunnel-Start,10:Tunnel-Stop,11:Tunnel-Reject,12:Tunnel-Link-Start,13:Tunnel-Link-Stop,14:Tunnel-Link-Reject,15:Failed", 	 
                                    "Acct-Delay-Time","Acct-Input-Octets","Acct-Output-Octets","Acct-Session-ID","Acct-Authentic,0:None,1:RADIUS,2:Local,3:Remote", "Acct-Session-Time",	
                                    "Acct-Input-Packets","Acct-Output-Packets", 	
                                    "Acct-Terminate-Cause,1:User-Request,2:Lost-Carrier,3:Lost-Service,4:Idle-Timeout,5:Session-Timeout,6:Admin-Reset,7:Admin-Reboot,8:Port-Error,9:NAS-Error,10:NAS-Request,11:NAS-Reboot,12:Port-Unneeded,13:Port-Preempted,14:Port-Suspended,15:Service-Unavailable,16:Callback,17:User-Error,18:Host-Request,19:Supplicant-Restart,20:Reauthentication-Failure,21:Port-Reinit,22:Port-Disabled", 	
                                    "Acct-Multi-Session-ID", 		
                                    "Acct-Link-Count","Acct-Input-Gigawords","Acct-Output-Gigawords","Event-Timestamp","CHAP-Challenge",	
                                    "NAS-Port-Type,0:Async (Modem),1:Sync (T1 Line),2:ISDN Sync,3:ISDN Async V.120,4:ISDN Async V.110,5:Virtual (VPN),6:PIAFS,7:HDLC Clear Channel,8:X.25,9:X.75,10:G.3 Fax,11:SDSL - Symmetric DSL,12:ADSL-CAP - Asymmetric DSL Carrierless Amplitude Phase Modulation,13:ADSL-DMT - Asymmetric DSL Discrete Multi-Tone,14:IDSL - ISDN Digital Subscriber Line,15:Ethernet,16:xDSL - Digital Subscriber Line of unknown type,17:Cable,18:Wireless - Other,19:Wireless - IEEE 802.11,20:Token Ring,21:FDDI",
                                    "Port-Limit","Login-LAT-Port", 	
                                    "Tunnel-Type,1:Point-to-Point Tunneling Protocol (PPTP),2:Layer Two Forwarding (L2F),3:Layer Two Tunneling Protocol (L2TP),4:Ascend Tunnel Management Protocol (ATMP),5:Virtual Tunneling Protocol (VTP),6:IP Authentication Header in the Tunnel-mode (AH),7:IP-in-IP Encapsulation (IP-IP),8:Minimal IP-in-IP Encapsulation (MIN-IP-IP),9:IP Encapsulating Security Payload in the Tunnel-mode (ESP),10:Generic Route Encapsulation (GRE),11:Bay Dial Virtual Services (DVS),12:IP-in-IP Tunneling,13:Virtual LANs (VLAN)",
                                    "Tunnel-Medium-Type,1:IP (IP version 4),2:IP6 (IP version 6),3:NSAP,4:HDLC (8-bit multidrop),5:BBN 1822,6:802 (includes all 802 media plus Ethernet canonical format),7:E.163 (POTS),8:E.164 (SMDS Frame Relay ATM),9:F.69 (Telex),10:X.121 (X.25 Frame Relay),11:IPX,12:Appletalk,13:Decnet IV,14:Banyan Vines,15:E.164 with NSAP format subaddress",
                                    "Tunnel-Client-Endpt", "Tunnel-Server-Endpt","Acct-Tunnel-Connection","Tunnel-Password","ARAP-Password","ARAP-Features",	
                                    "ARAP-Zone-Access,1:Only allow access to default zone,2:Use zone filter inclusively,3:(not used),4:Use zone filter exclusively", 	
                                    "ARAP-Security","ARAP-Security-Data","Password-Retry","Prompt,0:No Echo,1:Echo","Connect-Info","Configuration-Token","EAP-Message","Signature",
                                    "Tunnel-Pvt-Group-ID","Tunnel-Assignment-ID","Tunnel-Preference","Acct-Interim-Interval","Acct-Tunnel-Packets-Lost",
                                    "NAS-Port-Id","Framed-Pool","Tunnel-Client-Auth-ID","Tunnel-Server-Auth-ID","NAS-IPv6-Address","Framed-Interface-Id",
                                    "Framed-IPv6-Prefix","Login-IPv6-Host","Framed-IPv6-Route","Framed-IPv6-Pool",
                                    "Client-IP-Address","Client-Vendor","MS-CHAP-Error",	
                                    "Authentication-Type,1:PAP,2:CHAP,3:MS-CHAP v1,4:MS-CHAP v2,5:EAP,6:ARAP,7:Unauthenticated,8:Extension,9:MS-CHAP v1 CPW,10:MS-CHAP v2 CPW",	
                                    "Client-Friendly-Name","SAM-Account-Name","Fully-Qualified-User-Name","EAP-Friendly-Name",	
                                    "Packet-Type,1:Accept-Request,2:Access-Accept,3:Access-Reject,4:Accounting-Request",	
                                    "Reason-Code," +
                                    "0:IAS_SUCCESS,1:IAS_INTERNAL_ERROR,2:IAS_ACCESS_DENIED,3:IAS_MALFORMED_REQUEST,4:IAS_GLOBAL_CATALOG_UNAVAILABLE,5:IAS_DOMAIN_UNAVAILABLE,6:IAS_SERVER_UNAVAILABLE," +
                                    "7:IAS_NO_SUCH_DOMAIN,8:IAS_NO_SUCH_USER,9:The request was discarded by a third-party extension DLL file, 16:IAS_AUTH_FAILURE,17:IAS_CHANGE_PASSWORD_FAILURE," +
                                    "18:IAS_UNSUPPORTED_AUTH_TYPE,19:No reversibly encrypted password is stored for the user account,20:Lan Manager Authentication is not enabled," +
                                    "22:The client could not be authenticated because the EAP type cannot be processed by the server,23:Unexpected error. Possible error in server or client configuration,32:IAS_LOCAL_USERS_ONLY," +
                                    "33:IAS_PASSWORD_MUST_CHANGE,34:IAS_ACCOUNT_DISABLED,35:IAS_ACCOUNT_EXPIRED,36:IAS_ACCOUNT_LOCKED_OUT,37:IAS_INVALID_LOGON_HOURS,38:IAS_ACCOUNT_RESTRICTION,48:IAS_NO_POLICY_MATCH," + 
                                    "49:Did not match connection request policy,64:IAS_DIALIN_LOCKED_OUT,65:IAS_DIALIN_DISABLED,66:IAS_INVALID_AUTH_TYPE,67:IAS_INVALID_CALLING_STATION,68:IAS_INVALID_DIALIN_HOURS," +
                                    "69:IAS_INVALID_CALLED_STATION,70:IAS_INVALID_PORT_TYPE,71:IAS_INVALID_RESTRICTION,72:The user cannot change his or her password because the change password option is not enabled for the matching remote access policy," +
                                    "80:IAS_NO_RECORD,96:IAS_SESSION_TIMEOUT,97:IAS_UNEXPECTED_REQUEST,112:The remote RADIUS server did not process the authentication request," + 
                                    "258:The revocation function was unable to check revocation for the certificate,260:The message supplied for verification has been altered,262:The supplied message is incomplete. The signature was not verified,266:The message received was unexpected or badly formatted",	
                                    "NP-Policy-Name" };

            index = Array.IndexOf(ID ,id);

            if (index > 0)
            {
                attribute = Attributes[index];

                if (attribute.Contains(","))
                {
                    string[] partsofattribute = attribute.Split(',');
                    attributeName = partsofattribute[0];
                    for (int i = 1; i < partsofattribute.Length; i++)
                    {
                        if (value == partsofattribute[i].Split(':')[0])
                        {
                            attributeValue = partsofattribute[i].Split(':')[1];
                        }
                    }
                }
                else 
                {
                    attributeName = attribute;
                    attributeValue = value;
                }
                return attributeName + " is : " + attributeValue ;
            }
            else 
            {
                return "ID "+ id +" is not found in the ID Array";
            }
        }

        public Rec str_Paracala(string sLine, Rec rRec)
        {
            string[] specialID = {"46","42","43","47","48","8","5","61","4136","25","6"," 64","4142"};
            int controlindex;

            string[] fields = sLine.Split(',');

            if (fields[0].Contains("?"))
            {
                fields[0] = fields[0].Replace("?", "");
            }

            string nas_ip_address = "";
            string user_name = "";
            string service_name = "";
            string computer_name = "";
            string date = "";
            string time = "";
            string date_time = "";
            DateTime controlDate = DateTime.Today;


            nas_ip_address = fields[0];
            user_name = fields[1];
            service_name = fields[4];
            computer_name = fields[5];
            
            date = fields[2].Trim();
            time = fields[3].Trim();
            string[] dateArray = { "","","" };

            char specialcharforDate = '/';

            if (date.Contains("/")) 
            {
                specialcharforDate = '/';
                dateArray = date.Split('/');
            }
            else if (date.Contains("."))
            {
                specialcharforDate = '.';
                dateArray = date.Split('.');
            }
            else if (date.Contains("-")) 
            {
                specialcharforDate = '-';
                dateArray = date.Split('-');
            }

            date = dateArray[1] + specialcharforDate + dateArray[0] + specialcharforDate + dateArray[2];

            date_time = date + " " + time;
            
            bool kontrol = true;
            
            try
            {
                controlDate = Convert.ToDateTime(date_time);   
            }
            catch (Exception)
            {
                kontrol = false;
            }

            if (!kontrol)
                date_time = DateTime.Now.ToString();
            
            rRec.LogName = LogName; 
            rRec.Datetime = date_time;
            rRec.UserName = user_name;
            rRec.ComputerName = computer_name;
            rRec.CustomStr2 = nas_ip_address; // string customstr2
            rRec.CustomStr6 = service_name; // string customstr6

            string id = "";
            string value = "";
            int acct_session_time = 0;
            int acct_input_octets = 0;
            int acct_output_octets = 0;
            int act_input_packets = 0;
            int acct_output_packets = 0;
            string framed_ip = "";
            long nas_port = 0;
            string nas_port_type = "";
            string packet_type = "";
            string _class ="";
            string service_type = "";
            string tunnel_type = "";
            string reason_code = "";
            string description = "";

            string returnedvalue = "";
            for (int i = 6; i < fields.Length; i = i+2)
            {
                   id = fields[i].Trim();
                value = fields[i + 1].Trim();
                controlindex = Array.IndexOf(specialID,id);
                if (controlindex >= 0)
                {
                    switch (specialID[controlindex])
                    {
                        case "46":
                            acct_session_time = Convert.ToInt32(value); // int customint1
                            break;
                        case "42":
                            acct_input_octets = Convert.ToInt32(value); // int customint2
                            break;
                        case "43":
                            acct_output_octets = Convert.ToInt32(value); // int customint3
                            break;
                        case "47":
                            act_input_packets = Convert.ToInt32(value); // int customint4
                            break;
                        case "48":
                            acct_output_packets = Convert.ToInt32(value); //int customint5
                            break;
                        case "8":
                            framed_ip = value; //string customstr1
                            break;
                        case "5":
                            nas_port = Convert.ToInt64(value); // long customint6
                            break;
                        case "61":
                            returnedvalue = returnAttribute(id,value);
                            nas_port_type = returnedvalue.Split(':')[1].Trim(); // string and has attribute value customstr3
                            break;
                        case "4136":
                            returnedvalue = returnAttribute(id, value);
                            packet_type = returnedvalue.Split(':')[1].Trim(); // string and has attribute value customstr4
                            break;
                        case "25":
                            _class = value; // string customstr5
                            break;
                        case "6":
                            returnedvalue = returnAttribute(id, value);
                            service_type = returnedvalue.Split(':')[1].Trim(); // string and has attribute value customstr7
                            break;
                        case "64":
                            returnedvalue = returnAttribute(id, value);
                            tunnel_type = returnedvalue.Split(':')[1].Trim(); // string and has attribute value customstr8
                            break;
                        case "4142":
                            returnedvalue = returnAttribute(id, value);
                            reason_code = returnedvalue.Split(':')[1].Trim(); // string and has attribute value customstr9
                            break;
                        default:
                            break;
                    }
                }
                else 
                {
                    description = description +" | "+ returnAttribute(id, value);
                }
            }
             
            description = description.Trim();
            description = description.Trim('|');

            rRec.CustomInt1 = acct_session_time;
            rRec.CustomInt2 = acct_input_octets;
            rRec.CustomInt3 = acct_output_octets;
            rRec.CustomInt4 = act_input_packets;
            rRec.CustomInt5 = acct_output_packets;
            rRec.CustomStr1 = framed_ip;
            rRec.CustomInt6 = nas_port;
            rRec.CustomStr3 = nas_port_type;
            rRec.CustomStr4 = packet_type;
            rRec.CustomStr5 = _class;
            rRec.CustomStr7 = service_type;
            rRec.CustomStr8 = tunnel_type;
            rRec.CustomStr9 = reason_code;

            int numberofCharecter = description.Length;
            int numberofcharecteroverflow;
            if (numberofCharecter > 900)
            {
                rRec.Description = description.Substring(0, 900);
                numberofcharecteroverflow = numberofCharecter - 900;
                rRec.CustomStr10 = description.Substring(900, numberofcharecteroverflow);
            }
            else
            {
                rRec.Description = description;
            }

            return rRec;
        }

        protected override void ParseFileNameLocal() // last file ve position
        {   
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Enter The Function ");

            if (dayChangeTimer == null)
            {
                dayChangeTimer = new System.Timers.Timer(60000);
                dayChangeTimer.Elapsed += new System.Timers.ElapsedEventHandler(dayChangeTimer_Elapsed);
                dayChangeTimer.Start();
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->>Day Change Timer Is Started in IAS");
            }

            FileStream fs = null;
            BinaryReader br = null;
            Int64 currentPosition = Position;

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Searching In Directory: " + Dir);

                ArrayList filenameList = new ArrayList();

                foreach (String file in Directory.GetFiles(Dir))
                {
                    filenameList.Add(Path.GetFileName(file));
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> " + filenameList.Count.ToString() + " File Found ");

                string[] fileName = new string[filenameList.Count];
                long[] permanentfileName = new long[filenameList.Count];
                string[] fullfileName = new string[filenameList.Count];

                for (int i = 0; i < filenameList.Count; i++)
                {
                    fileName[i] = filenameList[i].ToString().Split('.')[0];
                    permanentfileName[i] = Convert.ToInt64(fileName[i].Substring(2,fileName[i].Length-2)); 
                    fullfileName[i] = filenameList[i].ToString();
                }

                Array.Sort(permanentfileName, fullfileName);

                if (String.IsNullOrEmpty(lastFile))
                {   
                    if (fullfileName.Length > 0)
                    {
                        FileName = Dir + fullfileName[0].ToString();
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Last File Is Null So Last File Is Setted The First File : " + lastFile);
                    }
                    else 
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> No File Found");
                    }
                }
                else
                {
                    if (File.Exists(lastFile))
                    {
                        fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        br = new BinaryReader(fs, enc);
                        br.BaseStream.Seek(Position, SeekOrigin.Begin);
                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;

                        Char c = ' ';
                        while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                            c = br.ReadChar();

                            if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                            }
                        }

                        if (br.BaseStream.Position == br.BaseStream.Length || br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            for (int i = 0; i < fullfileName.Length; i++)
                            {
                                if (Dir + fullfileName[i].ToString() == lastFile)
                                {
                                    if (i + 1 == fullfileName.Length)
                                    {
                                        FileName = lastFile;
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> There Is No New Fýle and Waiting For New Record");
                                        break;
                                    }
                                    else
                                    {
                                        FileName = Dir + fullfileName[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Reading Of The File " + fullfileName[i] + " Finished And Continiu With New File " + fullfileName[i + 1]);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Continiu Reading The Last File : " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Last File Not Found : " + lastFile);
                          string _fileName = Path.GetFileName(lastFile).ToString();

                          string firstpartoffilename =  _fileName.ToString().Split('.')[0];
                          long  intpartoffilename =  Convert.ToInt64(firstpartoffilename.Substring(2, firstpartoffilename.Length - 2));
                          bool isthereFile = false;
                          int _index = 0; 
                          
                          for (int i = 0; i < permanentfileName.Length; i++)
                          {
                            if(intpartoffilename <= permanentfileName[i]) 
                            {
                                isthereFile = true;
                                _index = i;
                                break;
                            }  
                          }
                          
                          if (isthereFile)
                          {
                              FileName = Dir + fullfileName[_index].ToString();
                              lastFile = FileName;
                              Position = 0;   
                          }
                          else 
                          {
                              Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> There Is No New File ; Waiting For a New File");
                          }
                    }
                }
            }
            else
                FileName = Dir;

            if (br != null && fs != null)
            {
                br.Close();
                fs.Close();
            }
            lastFile = FileName;
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Filename is: " + FileName);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  MicrosoftIASServerRecorder in ParseFileNameLocal() -->> Exit The Function");
        }
    }
}
