using System;
using System.Collections.Generic;
using System.Text;

namespace SiteProtector20Recorder
{
    class ChildLogFormat
    {

        public ChildLogFormat(object attrType, object attr)
        {
            attributeType = attrType;
            attribute = attr; 
        }

        private object attributeType;

        public object AttributeType
        {
            get { return attributeType; }
            set { attributeType = value; }
        }

        private object attribute;

        public object Attribute
        {
            get { return attribute; }
            set { attribute = value; }
        }
    }
}
