using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Natek.IO.Readers;
using Natek.Recorders.Remote;
using Natek.Remote.Recorder;
using Natek.Remote.Recorders.Unified;

namespace RemoteRecorderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //RunCreateWslogdb70(args);
            //RunDrop(args);
            //RunBufferedReaderTest(args);
            //RunWebSenseV7_0_1RecorderTest(args);
            //RunWebsenseV_7_7_2SyslogRecorderTest(args);
            //RunFortigateUnifiedSyslogRecorderTest(args);
            //RunNatekMdmApacheRecorderTest(args);
            //RunNatekMdmStagingRecorderTest(args);
          //  RunDhcpRecorderTest(args);
           RunIisUnifiedRecorderTest(args);
            Console.Write("Press any key to terminate...");
            Console.ReadKey();
        }

        private static void RunIisUnifiedRecorderTest(string[] args)
        {
            var outFile = @"C:\Users\yusuf.aykac\Desktop\output2.txt";
            var fs = new StreamWriter(outFile, false);
            fs.Close();
            var recorder = new IisUnifiedRecorder();
            recorder.GetInstanceListService()["Security Manager Remote Recorder"] = new MockSecurityManagerRemoteRecorder
            {
                OutputEnabled = true,
                OutputFile = outFile
            };
            recorder.SetConfigData(1, @"E:\hazine_log", "", "0", "", "", false, 100000, "", "", "", 1000, 3, "FP=^u_ex[0-9]+\\.log$,T=iisV6", 0, "", "", 0); ;
            recorder.Init();
            recorder.Start();
        }
        /*
        private static void RunIisUnifiedRecorderTest(string[] args)
        {
            try
            {
                var outFile = @"o:\tmp\hazine_log\output.txt";
                var fs = new StreamWriter(outFile, false);
                fs.Close();
                var recorder = new IISUnifiedRecorder();
                recorder.GetInstanceListService()["Security Manager Remote Recorder"] =
                    new MockSecurityManagerRemoteRecorder()
                        {
                            OutputEnabled = true,
                            OutputFile = outFile
                        };
                recorder.SetConfigData(1, @"o:\tmp\hazine_log", "", "0", "", "", false, 100000, "", "", "", 1000, 3, "E=utf-8,FP=^u_ex[0-9]+\\.log$", 0, "", "", 0);
                recorder.Init();
                recorder.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        */
        private static void RunDhcpRecorderTest(string[] args)
        {
            try
            {
                var outFile = @"C:\Users\yusuf.aykac\Desktop\output.txt";
                var fs = new StreamWriter(outFile, false);
                fs.Close();
                var recorder = new DhcpUnifiedRecorder();
                recorder.GetInstanceListService()["Security Manager Remote Recorder"] = new MockSecurityManagerRemoteRecorder
                    {
                        OutputEnabled = true,
                        OutputFile = outFile
                    };
                recorder.SetConfigData(1, @"E:\logfiles", "", "0", "", "", false, 100000, "", "", "", 1000, 3, "IFP=DhcpSrvLog-(.*).log", 0, "", "", 0); ;
                recorder.Init();
                recorder.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void RunCreateWslogdb70(string[] args)
        {
            try
            {
                using (var fs = new StreamReader(@"o:\tmp\insert\insert.sql", Encoding.UTF8))
                {
                    var line = string.Empty;
                    var count = 0;
                    using (var ofs = new StreamWriter(@"o:\tmp\insert\insert5000.sql"))
                    {
                        while ((line = fs.ReadLine()) != null)
                        {
                            ofs.WriteLine(line);
                            if (++count == 5000)
                                return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }



        private static void RunBufferedReaderTest(string[] args)
        {
            using (var fs = new FileStream(@"o:\tmp\test.log", FileMode.Open, FileAccess.Read))
            {
                using (var of = new StreamWriter(@"o:\tmp\tmp.txt", false, Encoding.GetEncoding(1254)))
                {
                    var lr = new BufferedLineReader(fs);
                    var line = string.Empty;
                    int nl = 0;
                    int cnt = 0;
                    while ((line = lr.ReadLine(Encoding.GetEncoding(1254), ref nl)) != null)
                    {
                        Console.Write("\b\b\b\b\b\b\b\b\b\b{0:D10}", ++cnt);
                        of.WriteLine(line);
                        of.Flush();
                    }
                }
            }
        }

        private static void RunDrop(string[] args)
        {
            var tables = new[]
                {
"WsSQLAgentStart","WsSetProtocols",
"WsSetIncomingBuffer","WsSetI18NString",
"WsSetGroup","WsSetDisposition",
"WsSetDirectoryObj","WsInsertDelAdminValue",
"WsGetStoredProceduresVersion","WsGetModelDBVersion",
"WsGetKeywordID","WsGetI18NStringForLanguageIdAndStringId",
"WsGetDomainID","WsGetDispositionIdByDescription",
"WsGetCategoryNameForCategoryId","WsGetAllProtocols",
"WsGetAllI18NStrings","WsGetAllDispositions",
"WsGetAllDirObjectsMappings","WsGetAllDirObjects",
"WsGetAllCategories","WsDeleteAdminValues",
"WsCreateAllDelAdminViews","WsConstructAdminMapping",
"WsCheckDashboardVersion","usp_wtg_uninstall",
"usp_wtg_summary_task","usp_wtg_incomingbuffer_add_record",
"usp_wtg_get_config","usp_wtg_etl_process",
"usp_wtg_etl_get_perf","usp_wtg_etl",
"usp_wtg_dbload_validate","usp_wtg_activate_reports",
"usp_wse_trend_user_summary_task","usp_wse_trend_update_daily_ibt",
"usp_wse_trend_task","usp_wse_trend_summary_task",
"usp_wse_trend_manage","usp_wse_task_get_history",
"usp_wse_summary_task","usp_wse_setup_jobs",
"usp_wse_partition_add_time_index","usp_wse_get_config",
"usp_wse_etl_process","usp_wse_etl_get_perf",
"usp_wse_dbload_validate","usp_wse_config",
"usp_webcatcher_update_max","usp_webcatcher_update_flag",
"usp_webcatcher_insert_flag","usp_webcatcher_cleanup",
"usp_user_report_get_info","usp_user_permission_verify_logserver",
"usp_user_permission_verify","usp_user_get_info",
"usp_user_data_transfer","usp_update_wtg_partition_def",
"usp_update_wtg_etl_buffersize","usp_update_views",
"usp_update_user_rollover_permission","usp_update_explorer_info",
"usp_ua_usage_group_detail_by_trend","usp_ua_usage_group_detail",
"usp_ua_usage_detail_by_user","usp_ua_usage_detail_by_trend",
"usp_ua_usage_detail_by_token","usp_ua_usage_detail_by_group",
"usp_ua_trend_task","usp_ua_trend_manage",
"usp_ua_token_map_run","usp_ua_token_map_register",
"usp_ua_test_user_agent_string","usp_ua_template_token_map",
"usp_ua_template_group_summary","usp_ua_template_get_records",
"usp_ua_template_get_details","usp_ua_template_category_summary_trend",
"usp_ua_template_category_summary","usp_ua_set_template_token",
"usp_ua_set_template","usp_ua_set_max_storage_day",
"usp_ua_set_buffer","usp_ua_search_by_id",
"usp_ua_search","usp_ua_report_get_info",
"usp_ua_get_new_templates","usp_ua_get_latest_template_count",
"usp_ua_get_config","usp_ua_etl_process",
"usp_ua_etl_get_perf","usp_ua_etl_driver",
"usp_ua_etl","usp_ua_dashboard_tracking",
"usp_ua_config","usp_trend_driver",
"usp_template_report_new","usp_task_get_history",
"usp_system_info_set_baseline","usp_system_info_collector",
"usp_system_get_info","usp_system_config",
"usp_swap_buffer","usp_sql_job_get_exec_status",
"usp_space_reserved","usp_set_user_role",
"usp_set_true_file_code","usp_set_severity_category",
"usp_set_severity","usp_set_direction",
"usp_set_category_reason","usp_schema_audit_track",
"usp_scheduler_update_history","usp_scheduler_save_history",
"usp_scheduler_get_history","usp_scheduler_delete_job_content",
"usp_scheduler_delete_job","usp_scheduler_add_job_content",
"usp_scheduler_add_job","usp_sb_scheduled_job_modify_starttime",
"usp_sb_scheduled_job_modify_interval","usp_sb_scheduled_job_delete",
"usp_run_sql","usp_report_user_filter_get_source",
"usp_report_system_get_info","usp_report_spec_virtualizer_init",
"usp_report_spec_virtualizer_get_data","usp_report_spec_virtualizer_config_set_value",
"usp_report_spec_virtualizer_config_get_value","usp_report_spec_virtualizer_cleanup",
"usp_report_spec_user_search","usp_report_spec_user_domain_search",
"usp_report_spec_url_search","usp_report_spec_update_info",
"usp_report_spec_trend_get_data","usp_report_spec_setup",
"usp_report_spec_set_url_flag","usp_report_spec_set_title",
"usp_report_spec_set_time_range_id","usp_report_spec_set_template_name",
"usp_report_spec_set_name","usp_report_spec_set_filter_value_type",
"usp_report_spec_set_filter_data_type","usp_report_spec_set_field_id",
"usp_report_spec_set_display_name","usp_report_spec_set_desc",
"usp_report_spec_set_app_resource","usp_report_spec_remove_field",
"usp_report_spec_remove_all_fields","usp_report_spec_new",
"usp_report_spec_group_search","usp_report_spec_get_sql",
"usp_report_spec_get_newname","usp_report_spec_filter_remove_user_data",
"usp_report_spec_delete","usp_report_spec_copy_with_name",
"usp_report_spec_copy","usp_report_spec_content_update",
"usp_report_owner_locate_id","usp_report_ir_update_user_info",
"usp_report_ir_set_schedule_favorite_fields","usp_report_ir_delete_favorite",
"usp_report_ir_add_owner","usp_report_ir_add_favorite",
"usp_report_field_data_content_update","usp_report_favorite_delete_report",
"usp_report_favorite_add_report","usp_report_catalog_set_app_source",
"usp_report_catalog_new","usp_report_catalog_delete_report",
"usp_report_catalog_delete","usp_report_catalog_content_update",
"usp_report_catalog_add_report","usp_read_errorlog",
"usp_random_name","usp_product_feature_run_task",
"usp_partition_verify_product","usp_log_with_buffer",
"usp_log","usp_ir_tracking_get_info",
"usp_ir_add_tracking_info","usp_ip_country_update_code",
"usp_ip_country_spec_config_j","usp_ip_country_spec_config_i",
"usp_ip_country_spec_config_h","usp_ip_country_spec_config_g",
"usp_ip_country_spec_config_f","usp_ip_country_spec_config_e",
"usp_ip_country_spec_config_d","usp_ip_country_spec_config_c",
"usp_ip_country_spec_config_b","usp_ip_country_spec_config_a",
"usp_ip_country_spec_config","usp_install_background_jobs",
"usp_ibt_driver","usp_hosted_uninstall",
"usp_hosted_incomingbuffer_add_record","usp_hosted_get_config",
"usp_hosted_etl_process","usp_hosted_etl_get_perf",
"usp_hosted_dbload_validate","usp_event_log",
"usp_etl_get_perf","usp_etl_common_normalize",
"usp_etl","usp_error_log_info",
"usp_erpt_run_unit_tests","usp_erpt_get_web_protocol_activity",
"usp_erpt_get_top_wtg_realtime_threats_by_severity","usp_erpt_get_top_web_threats_by_severity",
"usp_erpt_get_top_urls_by_top_categories_in_realtime","usp_erpt_get_top_url_in_data_loss",
"usp_erpt_get_top_url_by_top_category_in_web20","usp_erpt_get_top_url_by_hits_in_web20",
"usp_erpt_get_top_ua_platforms_by_install","usp_erpt_get_top_ua_browsers_by_install",
"usp_erpt_get_top_ua_browser_version_by_install","usp_erpt_get_top_threat_urls_by_severity",
"usp_erpt_get_top_security_categories_by_url_count","usp_erpt_get_top_risk_classes_by_hits",
"usp_erpt_get_top_protocols_by_bandwidth","usp_erpt_get_top_infected_device_summary",
"usp_erpt_get_top_client_ips_by_severity_counts","usp_erpt_get_top_client_ips_by_hits_in_social_web",
"usp_erpt_get_top_client_ip_by_top_category_in_bandwidth_loss","usp_erpt_get_top_category_in_legal_liability",
"usp_erpt_get_top_category_in_data_loss","usp_erpt_get_top_category_in_bandwidth_loss",
"usp_erpt_get_top_category_by_hits","usp_erpt_get_top_category_by_client_ips_in_social_web",
"usp_erpt_get_time_period","usp_erpt_get_threat_severity_by_count",
"usp_erpt_get_threat_event_total","usp_erpt_get_source_ip_count",
"usp_erpt_get_policy_by_client_ip_count_in_social_web","usp_erpt_get_percent_ips_in_security_events",
"usp_erpt_get_percent_ips_in_legal_liabilty","usp_erpt_get_percent_ips_in_data_loss",
"usp_erpt_get_percent_ips_in_bandwidth_loss","usp_erpt_get_percent_client_ip_in_social_web",
"usp_erpt_get_network_risk_activity","usp_erpt_get_mobile_device_count",
"usp_erpt_get_infected_device_count","usp_erpt_get_hits_total",
"usp_erpt_get_bandwidth_total","usp_delete_partitions",
"usp_delete_job","usp_delete_background_jobs",
"usp_db_verify_wtg_content","usp_db_remove_prod",
"usp_db_delete_job_id","usp_db_delete_job",
"usp_db_add_prod_feature","usp_db_add_prod",
"usp_db_add_job","usp_dashboard_set_active",
"usp_dashboard_run_by_id","usp_dashboard_item_time_check",
"usp_dashboard_item_config","usp_dashboard_item_5",
"usp_dashboard_item_48","usp_dashboard_item_47",
"usp_dashboard_item_46","usp_dashboard_item_45",
"usp_dashboard_item_44","usp_dashboard_item_43",
"usp_dashboard_item_33","usp_dashboard_get_info",
"usp_dashboard_get_config","usp_dashboard_add",
"usp_create_wsibtsummary","USP_CREATE_WsIBTProcessUserBatch",
"USP_CREATE_WsIBTGetRunParameters","USP_CREATE_WsIBTDailyJob",
"usp_create_view_wtg_summary_urls","usp_create_view_wtg_incoming",
"usp_create_view_summary_url","usp_create_view_incoming",
"usp_create_trend_driver","usp_create_partition_ibt_reset",
"usp_create_maintenance_job","usp_create_etl_job",
"usp_create_background_jobs","usp_create_amt_etl_job",
"usp_country_update_values","usp_country_spec_config",
"usp_core_setup_jobs","usp_config_get_info",
"usp_checkEmptyString","usp_build_backup_script",
"usp_amt_update_views","usp_amt_trend_task",
"usp_amt_trend_manage","usp_amt_swap_buffer",
"usp_amt_siem_etl","usp_amt_setup",
"usp_amt_set_max_day","usp_amt_set_max_dashboard_day",
"usp_amt_set_buffer_siem","usp_amt_set_buffer_dss",
"usp_amt_set_buffer_core","usp_amt_set_buffer_analytics",
"usp_amt_partition_update_date","usp_amt_partition_new",
"usp_amt_partition_data_clean","usp_amt_partition_add_tables",
"usp_amt_load_demo_data","usp_amt_get_table_summary",
"usp_amt_get_table_detail_dump","usp_amt_get_table_detail",
"usp_amt_get_table_count","usp_amt_get_severity_count",
"usp_amt_get_scanid","usp_amt_get_global_count",
"usp_amt_get_dss_data","usp_amt_get_direction_count",
"usp_amt_get_config","usp_amt_get_category_severity",
"usp_amt_get_category_count","usp_amt_get_analytics_data",
"usp_amt_get_action_count","usp_amt_generate_demo_data",
"usp_amt_etl_summary","usp_amt_etl_process",
"usp_amt_etl_get_perf","usp_amt_etl_driver",
"usp_amt_etl_core_summary","usp_amt_dss_etl",
"usp_amt_core_trend_etl","usp_amt_core_etl",
"usp_amt_config","usp_amt_analytics_etl",
"usp_amt_add_job","usp_admin_trending_set_info",
"usp_admin_task_set_info","usp_admin_set_log_group_data",
"usp_admin_set_data_group_data","usp_admin_partition_unlimit",
"usp_admin_partition_rule_set_info","usp_admin_partition_read_only",
"usp_admin_partition_offline","usp_admin_partition_get_time_info",
"usp_admin_partition_get_size","usp_admin_ibt_set_info",
"usp_admin_file_group_update","usp_admin_db_setup_set_maintenance_info",
"usp_admin_db_setup_set_info","usp_admin_db_get_range",
"usp_addmin_partition_deleted","count_users"
                };
            var t = 0;
            do
            {
                t = 0;
                using (var con = new SqlConnection(@"User _id=sa;Password=@aa11aa!; Initial Catalog=master;Server=localhost"))
                {
                    con.Open();
                    for (var i = 0; i < tables.Length; i++)
                    {
                        try
                        {
                            if (tables[i] == null) continue;
                            using (var cmd = new SqlCommand("DROP PROCEDURE " + tables[i], con))
                            {
                                cmd.ExecuteNonQuery();
                                ++t;
                                tables[i] = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("does not exist"))
                            {
                                ++t;
                                tables[i] = null;
                            }
                        }
                    }
                }
            } while (t > 0);
        }
        /*
        private static void RunWebSenseV7_0_1RecorderTest(string[] args)
        {
            var recorder = new WebSenseV7_0_1Recorder();
            recorder.GetInstanceListService()["Security Manager Remote Recorder"] = new MockSecurityManagerRemoteRecorder();
            recorder.SetConfigData(1, "", "", "0", "wslogdb70", "", false, 1200, "sa", "@aa11aa!", "localhost", 1000, 3, "", 0, "", "", 0);
            recorder.Init();
            recorder.Start();

        }
         * */
    }
}
