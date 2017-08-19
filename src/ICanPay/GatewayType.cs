namespace ICanPay
{
    /// <summary>
    /// 网关类型
    /// </summary>
    public enum GatewayType
    {
        /// <summary>
        /// 未知网关类型
        /// </summary>
        None = 0,

        /// <summary>
        /// 支付宝
        /// </summary>
        Alipay = 1,

        /// <summary>
        /// 微信支付
        /// </summary>
        WeChatPayment = 2,

        /// <summary>
        /// 中国银联
        /// </summary>
        UnionPay = 3,

        /// <summary>
        /// 财付通
        /// </summary>
        Tenpay = 4,

        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 5,

    }
}