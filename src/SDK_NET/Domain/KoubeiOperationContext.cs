using System;
using System.Xml.Serialization;

namespace Aop.Api.Domain
{
    /// <summary>
    /// KoubeiOperationContext Data Structure.
    /// </summary>
    [Serializable]
    public class KoubeiOperationContext : AopObject
    {
        /// <summary>
        /// 如果是商户自己操作，请传入MERCHANT；如果是isv代操作，请传入ISV；如果是其他角色（服务商、服务商员工、商户员工）操作，不需填写
        /// </summary>
        [XmlElement("op_role")]
        public string OpRole { get; set; }
    }
}
