using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;

namespace ZilLion.Core.Unities.UnitiesMethods.SMS
{
    public class MailHelper
    {
        private readonly SmtpClient _smtpClient = null;   //设置SMTP协议
        private MailAddress _mailAddressFrom = null; //设置发信人地址  当然还需要密码
        private readonly MailMessage _mailMessageMail = null;
        #region 设置Smtp服务器信息
        public MailHelper(string serverHost, int port = 25)
        {
            //指定SMTP服务名  例如QQ邮箱为 smtp.qq.com 新浪cn邮箱为 smtp.sina.cn等
            //指定端口号
            //超时时间
            _smtpClient = new SmtpClient { Host = serverHost, Port = port, Timeout = 0, EnableSsl = true };
            _mailMessageMail = new MailMessage();
        }
        #endregion


        #region 设置发件人信息

        /// <summary>
        /// 设置发件人信息
        /// </summary>
        /// <param name="mailAddress">发件邮箱地址</param>
        /// <param name="mailPwd">邮箱密码</param>
        /// <param name="displayName">显示初始化名称</param>
        public void SetAddressform(string mailAddress, string mailPwd, string displayName = null)
        {
            //创建服务器认证
            //var networkCredentialMy = new NetworkCredential(mailAddress, mailPwd);
            //实例化发件人地址
            _mailAddressFrom = new System.Net.Mail.MailAddress(mailAddress, displayName);
            //指定发件人信息  包括邮箱地址和邮箱密码
            _smtpClient.Credentials = new System.Net.NetworkCredential(_mailAddressFrom.Address, mailPwd);
        }

        #endregion

        /// <summary>
        /// 向指定接收者发送邮件
        /// </summary>
        /// <param name="receivers">接收者列表</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="sendCompleted"> </param>
        /// <param name="attachmentPath">附件，默认为空</param>
        public void SendMail(string[] receivers, string subject, string body, Action sendCompleted = null, List<string> attachmentPath = null)
        {
            //清空历史发送信息 以防发送时收件人收到的错误信息(收件人列表会不断重复)
            _mailMessageMail.To.Clear();
            //添加收件人邮箱地址
            foreach (var receiver in receivers)
                _mailMessageMail.To.Add(new MailAddress(receiver));

            //发件人邮箱
            _mailMessageMail.From = _mailAddressFrom;
            //邮件主题
            _mailMessageMail.Subject = subject;
            _mailMessageMail.SubjectEncoding = System.Text.Encoding.UTF8;
            //邮件正文
            _mailMessageMail.Body = body;
            _mailMessageMail.BodyEncoding = System.Text.Encoding.UTF8;
            //清空历史附件  以防附件重复发送
            _mailMessageMail.Attachments.Clear();
            if (attachmentPath != null)
            {
                foreach (var attachment in attachmentPath)
                {
                    if (!string.IsNullOrEmpty(attachment) && File.Exists(attachment) && (new FileInfo(attachment).Length != 0))
                        _mailMessageMail.Attachments.Add(new Attachment(attachment, MediaTypeNames.Application.Octet));
                }
            }
            //开始发送邮件
            _smtpClient.Send(_mailMessageMail);
        }
    }
}
