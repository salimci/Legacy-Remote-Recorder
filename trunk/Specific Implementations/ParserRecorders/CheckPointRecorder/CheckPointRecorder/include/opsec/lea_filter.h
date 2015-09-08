#ifndef __LEA_FILTER_H
#define __LEA_FILTER_H

#include "opsec/opsec.h"
#include "opsec/opsec_export_api.h"

#ifdef	__cplusplus
extern "C" {
#endif

/*
 * 3rd-party LEA filtering header
 */

/* Filter match function */
typedef enum {
	LEA_FILTER_PRED_EQUALS,
	LEA_FILTER_PRED_BELONGS_TO,
	LEA_FILTER_PRED_EXISTS,
	LEA_FILTER_PRED_GREATER,
	LEA_FILTER_PRED_GREATER_EQUAL,
	LEA_FILTER_PRED_SMALLER,
	LEA_FILTER_PRED_SMALLER_EQUAL,
	LEA_FILTER_PRED_BELONGS_TO_RANGE,
	LEA_FILTER_PRED_BELONGS_TO_MASK,
	LEA_FILTER_PRED_CONTAINS_SUBSTRING,
	LEA_FILTER_PRED_TRUE
} eLeaFilterPredicateType;


/* Filter rule action */
typedef enum {
	LEA_FILTER_ACTION_PASS,
	LEA_FILTER_ACTION_DROP,
	LEA_FILTER_ACTION_PASS_FIELDS,
	LEA_FILTER_ACTION_DROP_FIELDS
} eLeaFilterRuleAction;

/* Filter action */
typedef enum {
	LEA_FILTER_REGISTER,
	LEA_FILTER_UNREGISTER
} eLeaFilterAction;

/* Filter return value */
typedef enum {
	LEA_FILTER_OK  = OPSEC_SESSION_OK,
	LEA_FILTER_ERR = OPSEC_SESSION_ERR,
	LEA_FILTER_NOT_SUP = 1 /* this value is returned by the LEA server, if the filter callback is not registered */
} eLeaFilterRetval;

typedef struct tagOpsecFilterPredicate LeaFilterPredicate;

typedef struct tagOpsecFilterRule LeaFilterRule;

typedef struct tagOpsecFilterRulebase LeaFilterRulebase;

typedef struct _opsec_filter_rule_iter_t lea_filter_rule_iter_t;

typedef struct _opsec_filter_predicate_iter_t lea_filter_predicate_iter_t;

/*
 * Function prototypes
 */
DLLIMP LeaFilterPredicate *
lea_filter_predicate_create(const char *attr, int dict, int nNegate, eLeaFilterPredicateType pred_type, ...);

DLLIMP void
lea_filter_predicate_destroy(LeaFilterPredicate *pred);

DLLIMP LeaFilterRule *
lea_filter_rule_create(eLeaFilterRuleAction nAction, ...);

DLLIMP void
lea_filter_rule_destroy(LeaFilterRule *rule);

DLLIMP int
lea_filter_rule_add_predicate(LeaFilterRule *rule, LeaFilterPredicate *pred);

DLLIMP lea_filter_predicate_iter_t *
lea_filter_predicate_iter_create(LeaFilterRule *rule);

DLLIMP void
lea_filter_predicate_iter_destroy(lea_filter_predicate_iter_t *iter);

DLLIMP LeaFilterPredicate *
lea_filter_predicate_iter_get_next(lea_filter_predicate_iter_t *iter);

DLLIMP LeaFilterRulebase *
lea_filter_rulebase_create();

DLLIMP void
lea_filter_rulebase_destroy(LeaFilterRulebase *filter);

DLLIMP int
lea_filter_rulebase_add_rule(LeaFilterRulebase *filter, LeaFilterRule *rule);

DLLIMP int
lea_filter_rulebase_register (OpsecSession *session, LeaFilterRulebase *filter, int *filter_id);

DLLIMP int
lea_filter_rulebase_unregister (OpsecSession *session, int filter_id);

DLLIMP lea_filter_rule_iter_t *
lea_filter_rule_iter_create(LeaFilterRulebase *filter);

DLLIMP void
lea_filter_rule_iter_destroy(lea_filter_rule_iter_t *iter);

DLLIMP LeaFilterRule *
lea_filter_rule_iter_get_next(lea_filter_rule_iter_t *iter);

#ifdef __cplusplus
}
#endif

#endif /* !__LEA_FILTER_H */

