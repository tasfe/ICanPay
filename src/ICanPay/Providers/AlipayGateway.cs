using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;


namespace ICanPay.Providers
{
    /// <summary>
    /// 支付宝网关
    /// </summary>
    public sealed class AlipayGateway : GatewayBase, IPaymentForm, IPaymentUrl, IQueryNow, IAppParams
    {

        #region 私有字段

        const string payGatewayUrl = "https://mapi.alipay.com/gateway.do";
        const string openapiGatewayUrl = "https://openapi.alipay.com/gateway.do";
        const string emailRegexString = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        Encoding pageEncoding;

        #endregion


        #region 构造函数

        /// <summary>
        /// 初始化支付宝网关
        /// </summary>
        public AlipayGateway()
        {
            pageEncoding = Encoding.GetEncoding(Charset);
        }


        /// <summary>
        /// 初始化支付宝网关
        /// </summary>
        /// <param name="gatewayParameterData">网关通知的数据集合</param>
        public AlipayGateway(List<GatewayParameter> gatewayParameterData)
            : base(gatewayParameterData)
        {
            pageEncoding = Encoding.GetEncoding(Charset);
        }

        #endregion


        #region 属性

        public override GatewayType GatewayType
        {
            get
            {
                return GatewayType.Alipay;
            }
        }

        #endregion


        #region 方法

        public string BuildPaymentForm()
        {        
            InitOrderParameter("MD5");
            ValidatePaymentOrderParameter();
            return GetFormHtml(payGatewayUrl);
        }


        /// <summary>
        /// 初始化订单参数
        /// </summary>
        private void InitOrderParameter(string sign_type)
        {
            SetGatewayParameterValue("seller_email", Merchant.Email);
            SetGatewayParameterValue("service", "create_direct_pay_by_user");
            SetGatewayParameterValue("partner", Merchant.Partner);
            SetGatewayParameterValue("notify_url", Merchant.NotifyUrl.ToString());
            SetGatewayParameterValue("return_url", Merchant.ReturnUrl.ToString());
            SetGatewayParameterValue("sign_type", sign_type);
            SetGatewayParameterValue("subject", Order.Subject);
            SetGatewayParameterValue("out_trade_no", Order.OrderNo);
            SetGatewayParameterValue("total_fee", Order.OrderAmount.ToString());
            SetGatewayParameterValue("payment_type", "1");
            SetGatewayParameterValue("_input_charset", Charset);
            SetGatewayParameterValue("sign", GetOrderSign());    // 签名需要在最后设置，以免缺少参数。
        }


        public string BuildPaymentUrl()
        {         
            InitOrderParameter("MD5");
            ValidatePaymentOrderParameter();
            return string.Format("{0}?{1}", payGatewayUrl, GetPaymentQueryString());
        }


        private string GetPaymentQueryString()
        {
            StringBuilder urlBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> item in GetSortedGatewayParameter())
            {
                urlBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
            }

            return urlBuilder.ToString().TrimEnd('&');
        }


        /// <summary>
        /// 获得用于签名的参数字符串
        /// </summary>
        private string GetSignParameter()
        {
            StringBuilder signBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> item in GetSortedGatewayParameter())
            {
                if (string.Compare("sign", item.Key) != 0 && string.Compare("sign_type", item.Key) != 0)
                {
                    signBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
                }
            }

            return signBuilder.ToString().TrimEnd('&');
        }



        /// <summary>
        /// 验证支付订单的参数设置
        /// </summary>
        private void ValidatePaymentOrderParameter()
        {
            if (string.IsNullOrEmpty(GetGatewayParameterValue("seller_email")))
            {
                throw new ArgumentNullException("seller_email", "订单缺少seller_email参数，seller_email是卖家支付宝账号的邮箱。" +
                                                "你需要使用PaymentSetting<T>.SetGatewayParameterValue(\"seller_email\", \"youname@email.com\")方法设置卖家支付宝账号的邮箱。");
            }

            if (!IsEmail(GetGatewayParameterValue("seller_email")))
            {
                throw new ArgumentException("Email格式不正确", "seller_email");
            }
        }

        protected override bool CheckNotifyData()
        {
            if (GetGatewayParameterValue("sign_type").ToUpper() != "RSA")
            {
                if (ValidateAlipayNotify() && ValidateAlipayNotifySign())
                {
                    return ValidateTrade();
                }
            }
            else
            {  
                if (ValidateAlipayNotifyRSASign())
                {
                    return ValidateTrade();
                }
            }
            return false;
        }

        private bool ValidateTrade()
        {
            var orderAmount = GetGatewayParameterValue("total_amount");
            orderAmount = string.IsNullOrEmpty(orderAmount) ? GetGatewayParameterValue("total_fee") : orderAmount;
            Order.OrderAmount = double.Parse(orderAmount);
            Order.OrderNo = GetGatewayParameterValue("out_trade_no");
            Order.TradeNo = GetGatewayParameterValue("trade_no");
            // 支付状态是否为成功。TRADE_FINISHED（普通即时到账的交易成功状态，TRADE_SUCCESS（开通了高级即时到账或机票分销产品后的交易成功状态）
            if (string.Compare(GetGatewayParameterValue("trade_status"), "TRADE_FINISHED") == 0 ||
                string.Compare(GetGatewayParameterValue("trade_status"), "TRADE_SUCCESS") == 0)
            {              
                return true;
            }
            return false;
        }

        /// <summary>
        /// 验证支付宝通知的签名
        /// </summary>
        private bool ValidateAlipayNotifyRSASign()
        {
            bool checkSign = AlipaySignature.RSACheckV1(GetSortedGatewayParameter(), Merchant.PublicKeyPem, Charset);
            if (checkSign)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 验证支付宝通知的签名
        /// </summary>
        private bool ValidateAlipayNotifySign()
        {
            // 验证通知的签名
            if (string.Compare(GetGatewayParameterValue("sign"), GetOrderSign()) == 0)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 将网关参数的集合排序
        /// </summary>
        /// <param name="coll">原网关参数的集合</param>
        private SortedList<string, string> GatewayParameterDataSort(ICollection<GatewayParameter> coll)
        {
            SortedList<string, string> list = new SortedList<string, string>();
            foreach (GatewayParameter item in coll)
            {
                list.Add(item.Name, item.Value);
            }

            return list;
        }

        /// <summary>
        /// 获得订单的签名。
        /// </summary>
        private string GetOrderRSASign()
        {
            return AlipaySignature.RSASign(GetSignParameter(), Merchant.PrivateKeyPem, Charset, "RSA");
        }

        /// <summary>
        /// 获得订单的签名。
        /// </summary>
        private string GetOrderSign()
        {
            // 获得MD5值时需要使用GB2312编码，否则主题中有中文时会提示签名异常，并且MD5值必须为小写。
            return Utility.GetMD5(GetSignParameter() + Merchant.Key, pageEncoding).ToLower();
        }


        public override void WriteSucceedFlag()
        {
            if (PaymentNotifyMethod == PaymentNotifyMethod.ServerNotify)
            {
                HttpContext.Current.Response.Write("success");
            }
        }


        /// <summary>
        /// 验证网关的通知Id是否有效
        /// </summary>
        private bool ValidateAlipayNotify()
        {
            // 浏览器自动返回的通知Id会在验证后1分钟失效，
            // 服务器异步通知的通知Id则会在输出标志成功接收到通知的success字符串后失效。
            if (string.Compare(Utility.ReadPage(GetValidateAlipayNotifyUrl()), "true") == 0)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 获得验证支付宝通知的Url
        /// </summary>
        private string GetValidateAlipayNotifyUrl()
        {
            return string.Format("{0}?service=notify_verify&partner={1}&notify_id={2}", payGatewayUrl, Merchant.Partner,
                                 GetGatewayParameterValue("notify_id"));
        }


        /// <summary>
        /// 是否是正确格式的Email地址
        /// </summary>
        /// <param name="emailAddress">Email地址</param>
        public bool IsEmail(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return false;
            }

            return Regex.IsMatch(emailAddress, emailRegexString);
        }


        public Dictionary<string, object> BuildPayParams()
        {
            SetGatewayParameterValue("app_id", Merchant.AppId);
            SetGatewayParameterValue("method", "alipay.trade.app.pay");
            SetGatewayParameterValue("charset", Charset);
            SetGatewayParameterValue("sign_type", "RSA");
            SetGatewayParameterValue("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            SetGatewayParameterValue("version", "1.0");
            SetGatewayParameterValue("notify_url", Merchant.NotifyUrl);
            SetGatewayParameterValue("biz_content",
                JsonConvert.SerializeObject(new
                {
                    subject = Order.Subject,
                    out_trade_no = Order.OrderNo,
                    total_amount = Order.OrderAmount,
                    product_code = "QUICK_MSECURITY_PAY"
                }));

            SetGatewayParameterValue("sign", AopUtils.SignAopRequest(GetSortedGatewayParameter(), Merchant.PrivateKeyPem, Charset, true, "RSA"));

            StringBuilder signBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> item in GetSortedGatewayParameter())
            {
                signBuilder.AppendFormat("{0}={1}&", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8));
            }
            Dictionary<string, object> resParam = new Dictionary<string, object>();
            resParam.Add("body", signBuilder.ToString().TrimEnd('&'));
            return resParam;
        }

        #region QueryNow
        public bool QueryNow(ProductSet productSet)
        {
            if (productSet == ProductSet.Web)
            {
                //InitQueryParameter();
                //ReadResultXml(Utility.ReadPage(string.Format("{0}?{1}", payGatewayUrl, GetPaymentQueryString()), pageEncoding));
                //if (ValidateNotifyParameter() && ValidateOrder())
                //{
                //    return true;
                //}
                return false;
            }
            else
            {
                IAopClient client = new DefaultAopClient(openapiGatewayUrl, Merchant.AppId, Merchant.PrivateKeyPem, true);
                AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
                request.BizContent = JsonConvert.SerializeObject(
                    new
                    {
                        out_trade_no = Order.OrderNo
                    });
                AlipayTradeQueryResponse response = client.Execute(request);
                if (((string.Compare(response.TradeStatus, "TRADE_FINISHED") == 0 || string.Compare(response.TradeStatus, "TRADE_SUCCESS") == 0)))
                {
                    var orderAmount = double.Parse(response.TotalAmount);
                    if (Order.OrderAmount == orderAmount && string.Compare(Order.OrderNo, response.OutTradeNo) == 0)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        /// <summary>
        /// 读取结果的XML
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private void ReadResultXml(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var node = xmlDocument.ChildNodes[1].ChildNodes[0];
            SetGatewayParameterValue(node.Name, node.InnerText);
            foreach (XmlNode rootNode in xmlDocument.ChildNodes)
            {
                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    SetGatewayParameterValue(item.Name, item.InnerText);
                }
            }
        }

        /// <summary>
        /// 初始化查询订单参数
        /// </summary>
        private void InitQueryParameter(string sign_type)
        {
            SetGatewayParameterValue("service", "single_trade_query");
            SetGatewayParameterValue("partner", Merchant.Partner);
            SetGatewayParameterValue("_input_charset", Charset);
            SetGatewayParameterValue("sign_type", sign_type);
            SetGatewayParameterValue("out_trade_no", Order.OrderNo);
            if (!string.IsNullOrEmpty(Order.TradeNo))
            {
                SetGatewayParameterValue("trade_no", Order.TradeNo);
            }
            SetGatewayParameterValue("sign", GetOrderSign());    // 签名需要在最后设置，以免缺少参数。
        }


        /// <summary>
        /// 检查通知签名是否正确、货币类型是否为RMB、是否支付成功。
        /// </summary>
        /// <returns></returns>
        private bool ValidateNotifyParameter()
        {
            //string.Compare(GetGatewayParameterValue("sign"), GetOrderSign()) == 0 
            if (string.Compare(GetGatewayParameterValue("is_success"), "T") == 0
               && ((string.Compare(GetGatewayParameterValue("trade_status"), "TRADE_FINISHED") == 0 ||
                    string.Compare(GetGatewayParameterValue("trade_status"), "TRADE_SUCCESS") == 0))
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 验证订单金额、订单号是否与之前的通知的金额、订单号相符
        /// </summary>
        /// <returns></returns>
        private bool ValidateOrder()
        {
            if (Order.OrderAmount == Convert.ToDouble(GetGatewayParameterValue("total_fee")) &&
               string.Compare(Order.OrderNo, GetGatewayParameterValue("out_trade_no")) == 0)
            {
                return true;
            }

            return false;
        }

   
        #endregion

        #endregion

    }
}