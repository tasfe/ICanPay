using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;


namespace ICanPay.Providers
{
    /// <summary>
    /// 微信支付网关
    /// </summary>
    /// <remarks>
    /// 使用模式二实现微信支付
    /// </remarks>
    public sealed class WeChatPaymentGataway : GatewayBase, IPaymentQRCode, IQueryNow,IWapPaymentUrl, IAppParams
    {

        #region 私有字段

        const string payGatewayUrl = "https://api.mch.weixin.qq.com/pay/unifiedorder";
        const string queryGatewayUrl = "https://api.mch.weixin.qq.com/pay/orderquery";

        #endregion


        #region 构造函数

        /// <summary>
        /// 初始化微信支付网关
        /// </summary>
        public WeChatPaymentGataway()
        {
        }


        /// <summary>
        /// 初始化微信支付网关
        /// </summary>
        /// <param name="gatewayParameterData">网关通知的数据集合</param>
        public WeChatPaymentGataway(List<GatewayParameter> gatewayParameterData)
            : base(gatewayParameterData)
        {
        }

        #endregion


        public override GatewayType GatewayType
        {
            get { return GatewayType.WeChatPayment; }
        }

        protected override bool CheckNotifyData()
        {
            ReadNotifyOrderParameter();
            if (IsSuccessResult())
            {
                return true;
            }

            return false;
        }

        public string GetPaymentQRCodeContent()
        {
            InitPaymentOrderParameter();
            return GetWeixinPaymentUrl(PostOrder(ConvertGatewayParameterDataToXml(), payGatewayUrl));
        }

        public bool QueryNow(ProductSet productSet)
        {
            InitQueryOrderParameter();
            return CheckQueryResult(PostOrder(ConvertGatewayParameterDataToXml(), queryGatewayUrl));
        }


        /// <summary>
        /// 初始化支付订单的参数
        /// </summary>
        private void InitPaymentOrderParameter(string trade_type = "NATIVE")
        {
            SetGatewayParameterValue("appid", Merchant.AppId);
            SetGatewayParameterValue("mch_id", Merchant.Partner);
            SetGatewayParameterValue("nonce_str", GenerateNonceString());
            SetGatewayParameterValue("body", Order.Subject);
            SetGatewayParameterValue("out_trade_no", Order.OrderNo);
            SetGatewayParameterValue("total_fee", (Order.OrderAmount * 100).ToString());
            SetGatewayParameterValue("spbill_create_ip", "127.0.0.1");
            SetGatewayParameterValue("notify_url", Merchant.NotifyUrl.ToString());
            SetGatewayParameterValue("trade_type", trade_type);
            SetGatewayParameterValue("sign", GetSign());    // 签名需要在最后设置，以免缺少参数。
        }


        private void ReadNotifyOrderParameter()
        {
            Order.OrderNo = GetGatewayParameterValue("out_trade_no");
            Order.OrderAmount = Convert.ToDouble(GetGatewayParameterValue("total_fee")) * 0.01;
            Order.TradeNo = GetGatewayParameterValue("transaction_id");
        }


        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <returns></returns>
        private string GenerateNonceString()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }


        /// <summary>
        /// 将网关数据转换成XML
        /// </summary>
        /// <returns></returns>
        private string ConvertGatewayParameterDataToXml()
        {
            StringBuilder xmlBuilder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(xmlBuilder, settings))
            {
                writer.WriteStartElement("xml");
                foreach (var item in GetSortedGatewayParameter())
                {
                    writer.WriteElementString(item.Key, item.Value);
                }
                writer.WriteEndElement();
                writer.Flush();
            }

            return xmlBuilder.ToString();
        }


        /// <summary>
        /// 获得签名
        /// </summary>
        /// <returns></returns>
        private string GetSign()
        {
            StringBuilder signBuilder = new StringBuilder();
            foreach (var item in GetSortedGatewayParameter())
            {
                // 空值的参数与sign参数不参与签名
                if (!string.IsNullOrEmpty(item.Value) && string.Compare("sign", item.Key) != 0)
                {
                    signBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
                }
            }

            signBuilder.Append("key=" + Merchant.Key);
            return Utility.GetMD5(signBuilder.ToString()).ToUpper();
        }


        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="orderXml">订单的XML内容</param>
        /// <param name="gatewayUrl">网关URL</param>
        /// <returns></returns>
        private string PostOrder(string orderXml, string gatewayUrl)
        {
            byte[] dataByte = Encoding.UTF8.GetBytes(orderXml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(gatewayUrl);
            request.Method = "POST";
            request.ContentType = "text/xml";
            request.ContentLength = dataByte.Length;

            try
            {
                using (Stream outStream = request.GetRequestStream())
                {
                    outStream.Write(dataByte, 0, dataByte.Length);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        if (reader != null)
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                request.Abort();
            }

            return string.Empty;
        }


        /// <summary>
        /// 获得微信支付的URL
        /// </summary>
        /// <param name="resultXml">创建订单返回的数据</param>
        /// <returns></returns>
        private string GetWeixinPaymentUrl(string resultXml)
        {
            // 需要先清除之前创建订单的参数，否则会对接收到的参数造成干扰。
            ClearGatewayParameterData();
            ReadResultXml(resultXml);
            if (IsSuccessResult())
            {
                return GetGatewayParameterValue("code_url");
            }

            return string.Empty;
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
            SortedDictionary<string, string> parma = new SortedDictionary<string, string>();
            if (xmlDocument.FirstChild != null && xmlDocument.FirstChild.ChildNodes != null)
            {
                foreach (XmlNode item in xmlDocument.FirstChild.ChildNodes)
                {
                    SetGatewayParameterValue(item.Name, item.InnerText);
                }
            }
        }


        /// <summary>
        /// 是否是成功的结果
        /// </summary>
        /// <param name="parma"></param>
        /// <returns></returns>
        private bool IsSuccessResult()
        {
            if (ValidateResult() && ValidateSign())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 验证返回的结果
        /// </summary>
        /// <returns></returns>
        private bool ValidateResult()
        {
            if (string.Compare(GetGatewayParameterValue("return_code"), "SUCCESS") == 0 &&
                string.Compare(GetGatewayParameterValue("result_code"), "SUCCESS") == 0)
            {
                return true;
            }

            return false;

        }


        /// <summary>
        /// 验证签名
        /// </summary>
        /// <returns></returns>
        private bool ValidateSign()
        {
            if (string.Compare(GetGatewayParameterValue("sign"), GetSign()) == 0)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 检查查询结果
        /// </summary>
        /// <param name="resultXml">查询结果的XML</param>
        /// <returns></returns>
        private bool CheckQueryResult(string resultXml)
        {
            // 需要先清除之前查询订单的参数，否则会对接收到的参数造成干扰。
            ClearGatewayParameterData();
            ReadResultXml(resultXml);
            if (IsSuccessResult())
            {
                if (!string.IsNullOrEmpty(GetGatewayParameterValue("total_fee")))
                {
                    if (string.Compare(Order.OrderNo, GetGatewayParameterValue("out_trade_no")) == 0 &&
                         Order.OrderAmount == int.Parse(GetGatewayParameterValue("total_fee")) / 100.0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 初始化查询订单参数
        /// </summary>
        private void InitQueryOrderParameter()
        {
            SetGatewayParameterValue("appid", Merchant.AppId);
            SetGatewayParameterValue("mch_id", Merchant.Partner);
            SetGatewayParameterValue("out_trade_no", Order.OrderNo);
            SetGatewayParameterValue("nonce_str", GenerateNonceString());
            SetGatewayParameterValue("sign", GetSign());    // 签名需要在最后设置，以免缺少参数。
        }


        /// <summary>
        /// 清除网关的数据
        /// </summary>
        private void ClearGatewayParameterData()
        {
            GatewayParameterData.Clear();
        }


        /// <summary>
        /// 初始化表示已成功接收到支付通知的数据
        /// </summary>
        private void InitProcessSuccessParameter()
        {
            SetGatewayParameterValue("return_code", "SUCCESS");
        }


        public override void WriteSucceedFlag()
        {
            // 需要先清除之前接收到的通知的参数，否则会对生成标志成功接收到通知的XML造成干扰。
            ClearGatewayParameterData();
            InitProcessSuccessParameter();
            HttpContext.Current.Response.Write(ConvertGatewayParameterDataToXml());
        }

        public Dictionary<string, object> BuildPayParams()
        {
            InitPaymentOrderParameter("APP");
            GetWeixinPaymentUrl(PostOrder(ConvertGatewayParameterDataToXml(), payGatewayUrl));

            var prepayid = GetGatewayParameterValue("prepay_id");
            ClearGatewayParameterData();

            SetGatewayParameterValue("appid", Merchant.AppId);
            SetGatewayParameterValue("partnerid", Merchant.Partner);
            SetGatewayParameterValue("prepayid", prepayid);
            SetGatewayParameterValue("package", "Sign=WXPay");
            SetGatewayParameterValue("noncestr", GenerateNonceString());
            SetGatewayParameterValue("timestamp", DateTime.Now.TimeStamp());
            SetGatewayParameterValue("sign", GetSign());

            Dictionary<string, object> resParam = new Dictionary<string, object>();
            resParam.Add("prepayid", prepayid);
            resParam.Add("noncestr", GetGatewayParameterValue("noncestr"));
            resParam.Add("sign", GetGatewayParameterValue("sign"));
            resParam.Add("timestamp", GetGatewayParameterValue("timestamp"));
            resParam.Add("partnerid", GetGatewayParameterValue("partnerid"));
            return resParam;
        }

        public string BuildWapPaymentUrl()
        {
            InitPaymentOrderParameter("MWEB");
            return string.Format("{0}&redirect_url={1}", GetWeixinPaymentUrl(PostOrder(ConvertGatewayParameterDataToXml(), payGatewayUrl)), HttpUtility.UrlEncode(Merchant.ReturnUrl.ToString(), Encoding.UTF8));
        }
    }
}
