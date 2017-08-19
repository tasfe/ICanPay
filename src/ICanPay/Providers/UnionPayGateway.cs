using com.unionpay.acp.sdk;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ICanPay.Providers
{
    /// <summary>
    /// 银联网关
    /// </summary>
    public class UnionPayGateway : GatewayBase, IPaymentForm, IWapPaymentForm, IQueryNow, IAppParams
    {
        #region 构造函数

        /// <summary>
        /// 初始化中国银联网关
        /// </summary>
        public UnionPayGateway()
        {
        }


        /// <summary>
        /// 初始化中国银联网关
        /// </summary>
        /// <param name="gatewayParameterData">网关通知的数据集合</param>
        public UnionPayGateway(List<GatewayParameter> gatewayParameterData)
            : base(gatewayParameterData)
        {
        }

        #endregion

        #region 属性
        public override GatewayType GatewayType
        {
            get
            {
                return GatewayType.UnionPay;
            }
        }

        #endregion

        #region 方法

        public Dictionary<string, object> BuildPayParams()
        {
            //组装请求报文
            Dictionary<string, string> param = new Dictionary<string, string>();
            // 版本号
            param.Add("version", "5.0.0");
            // 字符集编码 默认"UTF-8"
            param.Add("encoding", "UTF-8");
            // 签名方法 01 RSA
            param.Add("signMethod", "01");
            // 交易类型 01-消费
            param.Add("txnType", "01");
            // 交易子类型 01:自助消费 02:订购 03:分期付款
            param.Add("txnSubType", "01");
            // 业务类型
            param.Add("bizType", "000201");
            // 渠道类型，07-PC，08-手机
            param.Add("channelType", "08");
            // 前台通知地址 ，控件接入方式无作用
            param.Add("frontUrl", Merchant.ReturnUrl.ToString());
            // 后台通知地址
            param.Add("backUrl", Merchant.NotifyUrl.ToString());
            // 接入类型，商户接入填0 0- 商户 ， 1： 收单， 2：平台商户
            param.Add("accessType", "0");
            // 商户号码，请改成自己的商户号
            param.Add("merId", Merchant.Partner);
            // 商户订单号，8-40位数字字母
            param.Add("orderId", Order.OrderNo);
            // 订单发送时间，取系统时间
            param.Add("txnTime", Order.PaymentDate.ToString("yyyyMMddHHmmss"));
            // 交易金额，单位分
            param.Add("txnAmt", (Order.OrderAmount * 100).ToString());
            // 交易币种
            param.Add("currencyCode", "156");
            // 请求方保留域，透传字段，查询、通知、对账文件中均会原样出现
            // param.Add("reqReserved", "透传信息");
            // 订单描述，可不上送，上送时控件中会显示该信息
            // param.Add("orderDesc", "订单描述");

            AcpService.Sign(param, System.Text.Encoding.UTF8);

            Dictionary<String, String> resmap = AcpService.Post(param, SDKConfig.AppRequestUrl, System.Text.Encoding.UTF8);
            Dictionary<string, object> resParam = new Dictionary<string, object>();
            resParam.Add("tn", resmap["tn"]);
            return resParam;
        }

        public string BuildWapPaymentForm()
        {
            return BuildPaymentForm();
        }

        public string BuildPaymentForm()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();

            //以下信息非特殊情况不需要改动
            param["version"] = "5.0.0";//版本号
            param["encoding"] = "UTF-8";//编码方式
            param["txnType"] = "01";//交易类型
            param["txnSubType"] = "01";//交易子类
            param["bizType"] = "000201";//业务类型
            param["signMethod"] = "01";//签名方法
            param["channelType"] = "08";//渠道类型
            param["accessType"] = "0";//接入类型
            param["frontUrl"] = Merchant.NotifyUrl.ToString();  //前台通知地址      
            param["backUrl"] = Merchant.ReturnUrl.ToString();  //后台通知地址
            param["currencyCode"] = "156";//交易币种

            //TODO 以下信息需要填写
            param["merId"] = Merchant.Partner;//商户号，请改自己的测试商户号，此处默认取demo演示页面传递的参数
            param["orderId"] = Order.OrderNo;//商户订单号，8-32位数字字母，不能含“-”或“_”，此处默认取demo演示页面传递的参数，可以自行定制规则
            param["txnTime"] = Order.PaymentDate.ToString("yyyyMMddHHmmss");//订单发送时间，格式为YYYYMMDDhhmmss，取北京时间，此处默认取demo演示页面传递的参数，参考取法： DateTime.Now.ToString("yyyyMMddHHmmss")
            param["txnAmt"] = (Order.OrderAmount * 100).ToString();//交易金额，单位分，此处默认取demo演示页面传递的参数
            //param["reqReserved"] = "透传信息";//请求方保留域，透传字段，查询、通知、对账文件中均会原样出现，如有需要请启用并修改自己希望透传的数据

            //TODO 其他特殊用法请查看 pages/api_01_gateway/special_use_purchase.htm

            AcpService.Sign(param, System.Text.Encoding.UTF8);
            string html = AcpService.CreateAutoFormHtml(SDKConfig.FrontTransUrl, param, System.Text.Encoding.UTF8);// 将SDKUtil产生的Html文档写入页面，从而引导用户浏览器重定向 
            System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8; // 指定输出编码  
            return html;
        }

        public bool QueryNow(ProductSet productSet)
        {
            /**
             * 重要：联调测试时请仔细阅读注释！
             * 
             * 产品：跳转网关支付产品<br>
             * 交易：交易状态查询交易：只有同步应答 <br>
             * 日期： 2015-09<br>
             * 版本： 1.0.0 
             * 版权： 中国银联<br>
             * 说明：以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己需要，按照技术文档编写。该代码仅供参考，不提供编码性能及规范性等方面的保障<br>
             * 该接口参考文档位置：open.unionpay.com帮助中心 下载  产品接口规范  《网关支付产品接口规范》，<br>
             *              《平台接入接口规范-第5部分-附录》（内包含应答码接口规范，全渠道平台银行名称-简码对照表）<br>
             * 测试过程中的如果遇到疑问或问题您可以：1）优先在open平台中查找答案：
             * 							        调试过程中的问题或其他问题请在 https://open.unionpay.com/ajweb/help/faq/list 帮助中心 FAQ 搜索解决方案
             *                             测试过程中产生的6位应答码问题疑问请在https://open.unionpay.com/ajweb/help/respCode/respCodeList 输入应答码搜索解决方案
             *                           2） 咨询在线人工支持： open.unionpay.com注册一个用户并登陆在右上角点击“在线客服”，咨询人工QQ测试支持。
             * 交易说明： 1）对前台交易发起交易状态查询：前台类交易建议间隔（5分、10分、30分、60分、120分）发起交易查询，如果查询到结果成功，则不用再查询。（失败，处理中，查询不到订单均可能为中间状态）。也可以建议商户使用payTimeout（支付超时时间），过了这个时间点查询，得到的结果为最终结果。
             *        2）对后台交易发起交易状态查询：后台类资金类交易同步返回00，成功银联有后台通知，商户也可以发起 查询交易，可查询N次（不超过6次），每次时间间隔2N秒发起,即间隔1，2，4，8，16，32S查询（查询到03，04，05继续查询，否则终止查询）。
             *        					         后台类资金类同步返03 04 05响应码及未得到银联响应（读超时）需发起查询交易，可查询N次（不超过6次），每次时间间隔2N秒发起,即间隔1，2，4，8，16，32S查询（查询到03，04，05继续查询，否则终止查询）。
             */

            Dictionary<string, string> param = new Dictionary<string, string>();

            //以下信息非特殊情况不需要改动
            param["version"] = "5.0.0";//版本号
            param["encoding"] = "UTF-8";//编码方式
            param["signMethod"] = "01";//签名方法
            param["txnType"] = "00";//交易类型
            param["txnSubType"] = "00";//交易子类
            param["bizType"] = "000000";//业务类型
            param["accessType"] = "0";//接入类型
            param["channelType"] = "07";//渠道类型

            //TODO 以下信息需要填写
            param["orderId"] = Order.OrderNo;	//请修改被查询的交易的订单号，8-32位数字字母，不能含“-”或“_”，此处默认取demo演示页面传递的参数
            param["merId"] = Merchant.Partner;//商户代码，请改成自己的测试商户号，此处默认取demo演示页面传递的参数
            param["txnTime"] = Order.PaymentDate.ToString("yyyyMMddHHmmss");;//请修改被查询的交易的订单发送时间，格式为YYYYMMDDhhmmss，此处默认取demo演示页面传递的参数

            AcpService.Sign(param, System.Text.Encoding.UTF8);  // 签名
            string url = SDKConfig.SingleQueryUrl;

            Dictionary<String, String> rspData = AcpService.Post(param, url, System.Text.Encoding.UTF8);
        
            if (rspData.Count != 0)
            {

                if (AcpService.Validate(rspData, System.Text.Encoding.UTF8))
                {
                    string respcode = rspData["respCode"]; //其他应答参数也可用此方法获取
                    if ("00" == respcode)
                    {
                        string origRespCode = rspData["origRespCode"]; //其他应答参数也可用此方法获取
                        //处理被查询交易的应答码逻辑
                        if ("00" == origRespCode)
                        {
                            //交易成功，更新商户订单状态
                            //TODO
                            //Response.Write("交易成功。<br>\n");
                            return true;
                        }
                        else if ("03" == origRespCode ||
                            "04" == origRespCode ||
                            "05" == origRespCode)
                        {
                            //需再次发起交易状态查询交易
                            //TODO
                            //Response.Write("稍后查询。<br>\n");
                            return false;
                        }
                        else
                        {
                            //其他应答码做以失败处理
                            //TODO
                           // Response.Write("交易失败：" + rspData["origRespMsg"] + "。<br>\n");
                            return false;
                        }
                    }
                    else if ("03" == respcode ||
                            "04" == respcode ||
                            "05" == respcode)
                    {
                        //不明原因超时，后续继续发起交易查询。
                        //TODO
                        //Response.Write("处理超时，请稍后查询。<br>\n");
                        return false;
                    }
                    else
                    {
                        //其他应答码做以失败处理
                        //TODO
                        //Response.Write("查询操作失败：" + rspData["respMsg"] + "。<br>\n");
                        return false;
                    }
                }
            }
            else
            {
                //Response.Write("请求失败\n");
                return false;
            }
            return false;
        }

        public override void WriteSucceedFlag()
        {
            if (PaymentNotifyMethod == PaymentNotifyMethod.ServerNotify)
            {
                HttpContext.Current.Response.Write("success");
            }
        }

        protected override bool CheckNotifyData()
        {
            if (HttpContext.Current.Request.HttpMethod == "POST")
            {
                // 使用Dictionary保存参数
                Dictionary<string, string> resData = new Dictionary<string, string>();

                NameValueCollection coll = HttpContext.Current.Request.Form;

                string[] requestItem = coll.AllKeys;

                for (int i = 0; i < requestItem.Length; i++)
                {
                    resData.Add(requestItem[i], HttpContext.Current.Request.Form[requestItem[i]]);
                }

                // 返回报文中不包含UPOG,表示Server端正确接收交易请求,则需要验证Server端返回报文的签名
                if (AcpService.Validate(resData, Encoding.UTF8))
                {
                    Order.OrderNo = resData.ContainsKey("orderId") ? resData["orderId"] : "";
                    Order.OrderAmount = resData.ContainsKey("txnAmt") ? Convert.ToDouble(GetGatewayParameterValue("txnAmt")) * 0.01 : 0.0;
                    Order.TradeNo = resData.ContainsKey("orderId") ? resData["orderId"] : "";
                    if (resData["respMsg"].ToLower().Contains("success"))
                    {                     
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
