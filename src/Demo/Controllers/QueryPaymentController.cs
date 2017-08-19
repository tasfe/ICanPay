using ICanPay;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class QueryPaymentController : Controller
    {
        // GET: QueryPayment
        public void QueryWeChatPayOrder()
        {
            PaymentSetting querySetting = new PaymentSetting(GatewayType.WeChatPayment);
            querySetting.Merchant.AppId="wx000000000000000";
            querySetting.Merchant.Partner = "000000000000000";
            querySetting.Merchant.Key = "0000000000000000000000000000000000000000";

            // 查询时需要设置订单的Id与金额，在查询结果中将会核对订单的Id与金额，如果不相符会返回查询失败。
            querySetting.Order.OrderNo = "20";
            querySetting.Order.OrderAmount = 0.01;

            if (querySetting.CanQueryNow && querySetting.QueryNow())
            {
                // 订单已支付
            }
        }
    }
}