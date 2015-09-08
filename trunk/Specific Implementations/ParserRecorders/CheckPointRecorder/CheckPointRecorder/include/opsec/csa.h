#ifndef _csa_h_
#define _csa_h_

#ifdef __cplusplus
extern "C" {
#endif

#define CSA_MAX_IF_NAME     32
#define CSA_MAX_ID			32
#define CSA_MAX_DESC		31

#define CSA_MEMBER_STOPPED	1
#define CSA_MEMBER_DOWN		2
#define CSA_MEMBER_STANDBY	3
#define CSA_MEMBER_ACTIVE	4

#define CSA_MEMBER_MASTER			1
#define CSA_MEMBER_NON_MASTER		2

#define CSA_OPERATION_SUCCEED        0
#define CSA_OPERATION_FAILED		-1
#define CSA_INFORMATION_NOT_FOUND	-2

#define CSA_MAX_PNOTE_DESC			16
#define CSA_PNOTE_OK                0
#define CSA_PNOTE_PROBLEM        	3

#define CSA_TRUE			1
#define CSA_FALSE			0

typedef struct csa_member_info *csa_member_info_ptr_t;


DLLIMP int csa_get_cluster_size ( unsigned int *num );

DLLIMP int csa_get_member_info_size (unsigned int *size);

DLLIMP int csa_get_member_info(unsigned int index, csa_member_info_ptr_t  members_info);

DLLIMP int csa_get_id_from_member_info ( csa_member_info_ptr_t  members_info, 
							unsigned int *id );
DLLIMP int csa_get_my_id( unsigned int *id );

DLLIMP int csa_get_status_from_member_info ( csa_member_info_ptr_t  members_info, 
								unsigned int *status);

DLLIMP int csa_get_role_from_member_info (	csa_member_info_ptr_t  members_info,
								unsigned int *role );

DLLIMP int csa_get_sync_ip_from_member_info ( 	csa_member_info_ptr_t  members_info, 
									unsigned int * primary_sync_ip, 
									unsigned int *secondary_sync_ip, 
									unsigned int * third_sync_ip );

DLLIMP int csa_get_cluster_ip_from_member_if_name ( 	const char * interface_name,
										unsigned int *cluster_ip, 
										unsigned int *cluster_netmask );

DLLIMP int csa_uses_same_if_names ( void );  

DLLIMP int csa_if_has_same_name_on_all_cluster_members	(const char * interface_name);

DLLIMP int csa_get_member_if_name_from_cluster_ip (   	unsigned int cluster_ip, 
										unsigned int cluster_netmask, 
										char *interface_name);
DLLIMP int csa_register_status_updates ( const char * name, int pid, int sig );

DLLIMP int csa_unregister_status_updates ( const char * name );

DLLIMP int csa_pnote_register(char *name, unsigned int timeout, int initial_state);

DLLIMP int csa_pnote_report(char *name,  int state);

DLLIMP int csa_pnote_unregister(char *name);

#ifdef __cplusplus
}
#endif

#endif /* _csa_h_ */
