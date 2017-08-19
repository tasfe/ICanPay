using ICanPay;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class NotifyController : Controller
    {
        /// <summary>
        /// 商户数据
        /// </summary>
        List<Merchant> merchantList;

        /// <summary>
        /// 订阅支付通知事件
        /// </summary>
        PaymentNotify notify;

        public NotifyController()
        {
            merchantList = new List<Merchant>();
            Merchant alipayMerchant = new Merchant();
            alipayMerchant.GatewayType = GatewayType.Alipay;
            alipayMerchant.Partner = "000000000000000";
            alipayMerchant.Key = "000000000000000000000000000000000000000000";

            Merchant unionPayMerchant = new Merchant();
            unionPayMerchant.GatewayType = GatewayType.UnionPay;
            unionPayMerchant.Partner = "000000000000000";
            unionPayMerchant.Key = "000000000000000000000000000000000000000000";

            Merchant weChatPaymentMerchant = new Merchant();
            weChatPaymentMerchant.GatewayType = GatewayType.WeChatPayment;
            weChatPaymentMerchant.AppId = "wx000000000000000";
            weChatPaymentMerchant.Partner = "000000000000000";
            weChatPaymentMerchant.Key = "000000000000000000000000000000000000000000";

            // 添加到商户数据集合
            notify = new PaymentNotify(merchantList);
            merchantList.Add(alipayMerchant);
            merchantList.Add(unionPayMerchant);
            merchantList.Add(weChatPaymentMerchant);


            notify.PaymentSucceed += new PaymentSucceedEventHandler(notify_PaymentSucceed);
            notify.PaymentFailed += new PaymentFailedEventHandler(notify_PaymentFailed);
            notify.UnknownGateway += new UnknownGatewayEventHandler(notify_UnknownGateway);
        }

        public void ServerNotify()
        {          
            // 接收并处理支付通知
            notify.Received(PaymentNotifyMethod.ServerNotify);
        }

        public void AutoReturn()
        {
            // 接收并处理支付通知
            notify.Received(PaymentNotifyMethod.AutoReturn);
        }

        private void notify_PaymentSucceed(object sender, PaymentSucceedEventArgs e)
        {
            // 支付成功时时的处理代码
            if (e.PaymentNotifyMethod == PaymentNotifyMethod.AutoReturn)
            {
                // 当前是用户的浏览器自动返回时显示充值成功页面
            }
        }

        private void notify_PaymentFailed(object sender, PaymentFailedEventArgs e)
        {
            // 支付失败时的处理代码
        }

        private void notify_UnknownGateway(object sender, UnknownGatewayEventArgs e)
        {
            // 无法识别支付网关时的处理代码
        }
    }

}