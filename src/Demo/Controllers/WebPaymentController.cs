using ICanPay;
using System;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class WebPaymentController : Controller
    {
        public void CreateOrder(GatewayType gatewayType)
        {
            PaymentSetting paymentSetting = new PaymentSetting(gatewayType);
            paymentSetting.Merchant.AppId = "appid000000000000000";
            paymentSetting.Merchant.Email = "yourname@address.com";
            paymentSetting.Merchant.Partner = "000000000000000";
            paymentSetting.Merchant.Key = "000000000000000000000000000000000000000000";
            paymentSetting.Merchant.NotifyUrl = new Uri("http://yourwebsite.com/Notify.aspx");
            paymentSetting.Merchant.ReturnUrl = new Uri("http://yourwebsite.com/Return.aspx");

            paymentSetting.Order.OrderAmount = 0.01;
            paymentSetting.Order.OrderNo = "35";
            paymentSetting.Order.Subject = "WebPayment";
            paymentSetting.Payment();
        }


    }
}