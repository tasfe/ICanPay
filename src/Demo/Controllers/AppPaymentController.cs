using ICanPay;
using System;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class AppPaymentController : Controller
    {
        public JsonResult CreateOrder(GatewayType gatewayType)
        {
            PaymentSetting paymentSetting = new PaymentSetting(gatewayType);
            paymentSetting.Merchant.AppId = "appid000000000000000";
            paymentSetting.Merchant.Email = "yourname@address.com";
            paymentSetting.Merchant.Partner = "000000000000000";
            paymentSetting.Merchant.Key = "000000000000000000000000000000000000000000";
            paymentSetting.Merchant.PrivateKeyPem = "yourrsa_private_key.pem";
            paymentSetting.Merchant.PublicKeyPem = "yourrsa_public_key.pem";
            paymentSetting.Merchant.NotifyUrl = new Uri("http://yourwebsite.com/Notify.aspx");

            paymentSetting.Order.OrderAmount = 0.01;
            paymentSetting.Order.OrderNo = "35";
            paymentSetting.Order.Subject = "AppPayment";
            return Json(paymentSetting.BuildPayParams()) ;
        }



    }
}