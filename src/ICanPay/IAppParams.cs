using System.Collections.Generic;

namespace ICanPay
{
    interface IAppParams
    {
        /// <summary>
        ///创建客户端SDK支付需要信息
        /// </summary>
        Dictionary<string, object> BuildPayParams();
    }
}
