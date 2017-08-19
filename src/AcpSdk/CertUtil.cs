using System.Numerics;
using System.IO;
using System;
using log4net;
using System.Collections.Generic;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;

namespace com.unionpay.acp.sdk
{

    public class Cert
    {
        //public X509Certificate2 cert;
        //public string certId;
        //public RSACryptoServiceProvider key;
        public AsymmetricKeyParameter key;
        public X509Certificate cert;
        public string certId;

    }

    public class CertUtil
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(CertUtil));

        private static Dictionary<string, Cert> signCerts = new Dictionary<string,Cert>();
        private static Cert encryptCert = null;
        private static Dictionary<string, Cert> cerCerts = new Dictionary<string, Cert>();

        private static void initSignCert(string certPath, string certPwd)
        {
            log.Info("读取签名证书……");


            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(certPath, FileMode.Open);

                Pkcs12Store store = new Pkcs12Store(fileStream, certPwd.ToCharArray());

                string pName = null;
                foreach (string n in store.Aliases)
                {
                    if (store.IsKeyEntry(n))
                    {
                        pName = n;
                        //break;
                    }
                }

                Cert signCert = new Cert();
                signCert.key = store.GetKey(pName).Key;
                X509CertificateEntry[] chain = store.GetCertificateChain(pName);
                signCert.cert = chain[0].Certificate;
                signCert.certId = signCert.cert.SerialNumber.ToString();

                signCerts[certPath] = signCert;
                log.Info("签名证书读取成功，序列号：" + signCert.certId);

            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }

        }
        

        /// <summary>
        /// 获取签名证书私钥
        /// </summary>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetSignKeyFromPfx()
        {
             log.Debug("取配置文件证书");
             return GetSignKeyFromPfx(SDKConfig.SignCertPath, SDKConfig.SignCertPwd);
        }

        /// <summary>
        /// 获取签名证书私钥
        /// </summary>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetSignKeyFromPfx(string certPath, string certPwd)
        {
            log.Debug("传入证书");
            if (!signCerts.ContainsKey(certPath))
            {
                initSignCert(certPath, certPwd);
            }
            return signCerts[certPath].key;
        }


        /// <summary>
        /// 获取签名证书的证书序列号
        /// </summary>
        /// <returns></returns>
        public static string GetSignCertId(string certPath, string certPwd)
        {
            log.Debug("传入证书");
            if (!signCerts.ContainsKey(certPath))
            {
                initSignCert(certPath, certPwd);
            }
            return signCerts[certPath].certId;
        }

        /// <summary>
        /// 获取签名证书的证书序列号
        /// </summary>
        /// <returns></returns>
        public static string GetSignCertId()
        {
            log.Debug("取配置文件证书");
            return GetSignCertId(SDKConfig.SignCertPath, SDKConfig.SignCertPwd);
        }
        
        private static void initEncryptCert()
        {
            log.Info("读取加密证书……");

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(SDKConfig.EncryptCert, FileMode.Open);
                X509Certificate cert = new X509CertificateParser().ReadCertificate(fileStream);

                encryptCert = new Cert();
                encryptCert.cert = cert;
                encryptCert.certId = cert.SerialNumber.ToString();
                encryptCert.key = cert.GetPublicKey();

                log.Info("加密证书读取成功，序列号：" + encryptCert.certId);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// 获取加密证书的证书序列号
        /// </summary>
        /// <returns></returns>
        public static string GetEncryptCertId()
        {
            if (encryptCert == null)
            {
                initEncryptCert();
            }
            return encryptCert.certId;
        }


        /// <summary>
        /// 获取加密证书的RSACryptoServiceProvider
        /// </summary>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetEncryptKey()
        {
            if (encryptCert == null)
            {
                initEncryptCert();
            }
            return encryptCert.key;
        }

        private static void initCerCerts()
        {
            log.Info("读取验签证书文件夹下所有cer文件……");
            DirectoryInfo directory = new DirectoryInfo(SDKConfig.ValidateCertDir);
            FileInfo[] files = directory.GetFiles("*.cer");
            if (null == files || 0 == files.Length)
            {
                log.Info("请确定[" + SDKConfig.ValidateCertDir + "]路径下是否存在cer文件");
                return;
            }
            foreach (FileInfo file in files)
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(file.DirectoryName + "\\" + file.Name, FileMode.Open);
                    X509Certificate certificate = new X509CertificateParser().ReadCertificate(fileStream);

                    Cert cert = new Cert();
                    cert.cert = certificate;
                    cert.certId = certificate.SerialNumber.ToString();
                    cert.key = certificate.GetPublicKey();
                    cerCerts[cert.certId] = cert;
                    log.Info(file.Name + "读取成功，序列号：" + cert.certId);
                }
                finally
                {
                    if(fileStream != null)
                        fileStream.Close();
                }
            }
        }

        /// <summary>
        /// 通过证书id，获取验证签名的证书
        /// </summary>
        /// <param name="certId"></param>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetValidateKeyFromPath(string certId)// 
        {
            if (cerCerts == null || cerCerts.Count <= 0)
            {
                initCerCerts();
            }
            if (cerCerts == null || cerCerts.Count <= 0)
            {
                log.Info("未读取到任何证书……");
                return null;
            }
            if (cerCerts.ContainsKey(certId))
            {
                return cerCerts[certId].key;
            }
            else
            {
                log.Info("未匹配到序列号为[" + certId + "]的证书");
                return null;
            }
        }

    }
}