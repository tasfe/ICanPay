using System;
using System.Runtime.Serialization;

namespace ICanPay
{
    /// <summary>
    /// 商户数据
    /// </summary>
    [DataContract]
    [Serializable]
    public class Merchant
    {

        #region 私有字段

        string partner;
        string key;
        string email;
        string appId;
        Uri notifyUrl;
        Uri returnUrl;

        #endregion


        #region 构造函数

        public Merchant()
        {
        }


        public Merchant(string userName, string key, Uri notifyUrl, GatewayType gatewayType)
        {
            this.partner = userName;
            this.key = key;
            this.notifyUrl = notifyUrl;
            GatewayType = gatewayType;
        }

        #endregion


        #region 属性

        /// <summary>
        /// 商户帐号
        /// </summary>
        [DataMember]
        public string Partner
        {
            get
            {
                if (string.IsNullOrEmpty(partner))
                {
                    throw new ArgumentNullException("Partner", "商户帐号没有设置");
                }
                return partner;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Partner", "商户帐号不能为空");
                }
                partner = value;
            }
        }


        /// <summary>
        /// 商户密钥
        /// </summary>
        [DataMember]
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("Key", "商户密钥没有设置");
                }
                return key;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Key", "商户密钥不能为空");
                }
                key = value;
            }
        }

        /// <summary>
        /// 商户邮箱
        /// </summary>
        [DataMember]
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
            }
        }

        /// <summary>
        ///  微信支付等需要
        /// </summary>
        [DataMember]
        public string AppId
        {
            get
            {
                return appId;
            }
            set
            {
                appId = value;
            }
        }

        /// <summary>
        /// 网关回发通知URL
        /// </summary>
        [DataMember]
        public Uri NotifyUrl
        {
            get
            {
                if (notifyUrl == null)
                {
                    throw new ArgumentNullException("NotifyUrl", "网关通知Url没有设置");
                }
                return notifyUrl;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NotifyUrl", "网关通知Url不能为空");
                }
                notifyUrl = value;
            }
        }

        /// <summary>
        /// 网关主动跳转通知URL
        /// </summary>
        [DataMember]
        public Uri ReturnUrl
        {
            get
            {
                return returnUrl;
            }
            set
            {
                returnUrl = value;
            }
        }

        /// <summary>
        /// 私钥地址
        /// </summary>
        [DataMember]
        public string PrivateKeyPem { get; set; }

        /// <summary>
        /// 公钥地址
        /// </summary>
        [DataMember]
        public string PublicKeyPem { get; set; }

        /// <summary>
        /// 网关类型
        /// </summary>
        [DataMember]
        public GatewayType GatewayType { get; set; }

        #endregion

    }
}