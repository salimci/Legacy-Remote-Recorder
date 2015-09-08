using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    class Fields
    {
        private string date;
        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        private string user_name;
        public string User_Name
        {
            get { return user_name; }
            set { user_name = value; }
        }
        
        private int nas_port;
        public int Nas_Port
        {
            get { return nas_port; }
            set { nas_port = value; }
        }
        
        private string nas_ip_address;
        public string Nas_Ip_Address
        {
            get { return nas_ip_address; }
            set { nas_ip_address = value; }
        }

        private string framed_ip_address;
        public string Framed_Ip_Address
        {
            get { return framed_ip_address; }
            set { framed_ip_address = value; }
        }

        private string nas_identifier;
        public string Nas_Identifier
        {
            get { return nas_identifier; }
            set { nas_identifier = value; }
        }
        
        private int airespace_wlan_id;
        public int Airespace_Wlan_Id
        {
            get { return airespace_wlan_id; }
            set { airespace_wlan_id = value; }
        }
        
        private string acct_session_id;
        public string Acct_Session_Id
        {
            get { return acct_session_id; }
            set { acct_session_id = value; }
        }
        
        private string acct_authentic;
        public string Acct_Authentic
        {
            get { return acct_authentic; }
            set { acct_authentic = value; }
        }
        
        private string tunnel_type;
        public string Tunnel_Type
        {
            get { return tunnel_type; }
            set { tunnel_type = value; }
        }
        
        private string tunnel_medium_type;
        public string Tunnel_Medium_Type
        {
            get { return tunnel_medium_type; }
            set { tunnel_medium_type = value; }
        }
        
        private int tunnel_private_group_id;
        public int Tunnel_Private_Group_Id
        {
            get { return tunnel_private_group_id; }
            set { tunnel_private_group_id = value; }
        }
        
        private string acct_status_type;
        public string Acct_Status_Type
        {
            get { return acct_status_type; }
            set { acct_status_type = value; }
        }
        
        private int acct_input_octets;
        public int Acct_Input_Octets
        {
            get { return acct_input_octets; }
            set { acct_input_octets = value; }
        }
        
        private long acct_output_octets;
        public long Acct_Output_Octets
        {
            get { return acct_output_octets; }
            set { acct_output_octets = value; }
        }

        private int acct_input_packets;
        public int Acct_Input_Packets
        {
            get { return acct_input_packets; }
            set { acct_input_packets = value; }
        }

        private int acct_output_packets;
        public int Acct_Output_Packets
        {
            get { return acct_output_packets; }
            set { acct_output_packets = value; }
        }

        private int acct_session_time;
        public int Acct_Session_Time
        {
            get { return acct_session_time; }
            set { acct_session_time = value; }
        }

        private int acct_delay_time;
        public int Acct_Delay_Time
        {
            get { return acct_delay_time; }
            set { acct_delay_time = value; }
        }

        private string calling_station_id;
        public string Calling_Station_Id
        {
            get { return calling_station_id; }
            set { calling_station_id = value; }
        }

        private string called_station_id;
        public string Called_Station_Id
        {
            get { return called_station_id; }
            set { called_station_id = value; }
        }

        private string acct_unique_session_id;
        public string Acct_Unique_Session_Id
        {
            get { return acct_unique_session_id; }
            set { acct_unique_session_id = value; }
        }

        private string realm;
        public string Realm
        {
            get { return realm; }
            set { realm = value; }
        }

        private Int64 timestamp;
        public Int64 Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        private string request_authenticator;
        public string Request_Authenticator
        {
            get { return request_authenticator; }
            set { request_authenticator = value; }
        }

        public Fields()
        {
            date = "";
            user_name="";
            nas_port=0;
            nas_ip_address="";
            framed_ip_address="";
            nas_identifier="";
            airespace_wlan_id=0;
            acct_session_id="";
            acct_authentic="";
            tunnel_type="";
            tunnel_medium_type="";
            tunnel_private_group_id=0;
            acct_status_type="";
            acct_input_octets=0;
            acct_output_octets=0;
            acct_input_packets=0;
            acct_output_packets=0;
            acct_session_time=0;
            acct_delay_time=0;
            calling_station_id="";
            called_station_id="";
            acct_unique_session_id="";
            realm="";
            timestamp=0;
            request_authenticator="";
        }

        public void ClearObject()
        {
            date = "";
            user_name = "";
            nas_port = 0;
            nas_ip_address = "";
            framed_ip_address = "";
            nas_identifier = "";
            airespace_wlan_id = 0;
            acct_session_id = "";
            acct_authentic = "";
            tunnel_type = "";
            tunnel_medium_type = "";
            tunnel_private_group_id = 0;
            acct_status_type = "";
            acct_input_octets = 0;
            acct_output_octets = 0;
            acct_input_packets = 0;
            acct_output_packets = 0;
            acct_session_time = 0;
            acct_delay_time = 0;
            calling_station_id = "";
            called_station_id = "";
            acct_unique_session_id = "";
            realm = "";
            timestamp = 0;
            request_authenticator = "";
        }
    }   
}
