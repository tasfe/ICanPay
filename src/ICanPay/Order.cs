using System;

namespace ICanPay
{
    /// <summary>
    /// 订单的金额、编号
    /// </summary>
    public class Order
    {

        #region 私有字段
        double orderAmount;
        string orderNo;
        string tradeNo;
        string subject;
        DateTime paymentDate;

        #endregion


        #region 构造函数

        public Order()
        {
        }


        public Order(string orderNo, double orderAmount, string subject, DateTime paymentDate)
        {
            this.orderNo = orderNo;
            this.orderAmount = orderAmount;
            this.subject = subject;
            this.paymentDate = paymentDate;
        }

        #endregion


        #region 属性

        /// <summary>
        /// 订单总金额，以元为单位。例如：1.00，1元人民币。0.01，1角人民币。因为支付网关要求的最低支付金额为0.01元，所以OrderAmount最低为0.01。
        /// </summary>
        public double OrderAmount
        {
            get
            {
                if (orderAmount < 0.01)
                {
                    throw new ArgumentOutOfRangeException("OrderAmount", "订单金额没有设置");
                }

                return orderAmount;
            }

            set
            {
                if (value < (double)0.01)
                {
                    throw new ArgumentOutOfRangeException("OrderAmount", "订单金额必须大于或等于0.01");
                }

                orderAmount = value;
            }
        }


        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo
        {
            get
            {
                if (string.IsNullOrEmpty(orderNo))
                {
                    throw new ArgumentNullException("OrderNo", "订单订单编号没有设置");
                }

                return orderNo;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("OrderNo", "订单订单编号不能为空");
                }

                orderNo = value;
            }
        }

        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TradeNo
        {
            get
            {
                if (string.IsNullOrEmpty(tradeNo))
                {
                    throw new ArgumentNullException("TradeNo", "交易流水号不能为空");
                }

                return tradeNo;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("TradeNo", "交易流水号不能为空");
                }

                tradeNo = value;
            }
        }


        /// <summary>
        /// 订单主题，订单主题为空时将使用订单orderNo作为主题
        /// </summary>
        public string Subject
        {
            get
            {
                if(string.IsNullOrEmpty(subject))
                {
                    return orderNo;
                }

                return subject;
            }

            set
            {
                subject = value;
            }
        }

        /// <summary>
        /// 订单支付时间
        /// </summary>
        public DateTime PaymentDate
        {
            get
            {
                if (paymentDate == DateTime.MinValue)
                {
                    throw new ArgumentNullException("PaymentDate", "订单创建时间未赋值");
                }

                return paymentDate;
            }

            set
            {
                if (value == DateTime.MinValue)
                {
                    throw new ArgumentNullException("PaymentDate", "订单创建时间未赋值");
                }
                paymentDate = value;
            }
        }

       

        #endregion

    }
}
