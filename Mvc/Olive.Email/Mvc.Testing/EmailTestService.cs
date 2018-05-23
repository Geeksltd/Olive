﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;

namespace Olive.Email
{
    public class EmailTestService
    {
        static readonly Regex LinkPattern = new Regex("(https?://[^ ]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        HttpRequest Request => Context.Current.Request();
        HttpResponse Response => Context.Current.Response();
        string To, ReturnUrl;
        Attachment AttachmentFile;
        IEmailMessage Email;
        bool IsInitialized;

        public async Task<EmailTestService> Initialize()
        {
            To = Request.Param("to").ToStringOrEmpty().ToLower();
            ReturnUrl = Request.GetReturnUrl();
            if (Request.Has("attachmentInfo"))
                AttachmentFile = EmailService.ParseAttachment(Request.Param("attachmentInfo"));

            using (new SoftDeleteAttribute.Context(bypassSoftdelete: true))
                Email = await Request.GetOrDefault<IEmailMessage>("id");

            IsInitialized = true;

            return this;
        }

        public void ThrowIfItIsNotInitialized()
        {
            if (!IsInitialized) throw new InvalidOperationException("Initialize the instance before using it.");
        }

        void Validate()
        {
            if (Request.Has("id") && Email == null) throw new Exception("Invalid Email id specified.");
        }

        public async Task Process()
        {
            ThrowIfItIsNotInitialized();
            Validate();

            string response;
            if (AttachmentFile != null)
            {
                if (IsTextFile(AttachmentFile.Name))
                    response = "<a href='/?Web.Test.Command=testEmail&to=" + To + "'>&lt;&lt; Back to emails</a><pre>" + (await AttachmentFile.ContentStream.ReadAllText()).HtmlEncode() + "</pre>";
                else
                {
                    await Response.Dispatch(await AttachmentFile.ContentStream.ReadAllBytesAsync(), AttachmentFile.Name);
                    return;
                }
            }
            else if (Email == null)
            {
                response = await GenerateInbox();
            }
            else
            {
                response = GenerateEmailView();
            }

            await Dispatch(response);
        }

        async Task Dispatch(string response)
        {
            Response.ContentType = "text/html";
            await Response.WriteAsync("<html>");

            await Response.WriteAsync("<head>");
            await Response.WriteAsync("<style>");
            await Response.WriteAsync("body {font-family:Arial; background:#fff; }");
            await Response.WriteAsync("td, th {border:1px solid #999; padding:5px; font-size:9pt;}");
            await Response.WriteAsync("th {background:#eee;}");
            await Response.WriteAsync("a { color: blue; }");
            await Response.WriteAsync(".exit { background: #444; color:#fff; padding:4px 10px; display:inline-block; float:right; margin-top:10px; border-radius: 10px; text-decoration:none;}");
            await Response.WriteAsync(".body { background: #f0f0f0; }");
            await Response.WriteAsync(".label { color: #888; width:100px; }");
            await Response.WriteAsync("</style>");
            await Response.WriteAsync("</head>");
            await Response.WriteAsync("<body>");

            await Response.WriteAsync(response);

            await Response.WriteAsync("<a class='exit' href='{0}'>Exit Mailbox</a>".FormatWith(ReturnUrl));

            // TDD hack:
            await Response.WriteAsync("<a style='display:none;' href='/?Web.Test.Command=restart'>Restart Temp Database</a>");

            await Response.WriteAsync("</body></html>");
        }

        async Task<List<IEmailMessage>> GetEmails()
        {
            using (new SoftDeleteAttribute.Context(bypassSoftdelete: true))
            {
                var items = await Context.Current.Database()
                    .GetList<IEmailMessage>()
                    .Where(x => To.IsEmpty() || (x.To + "," + x.Cc + ", " + x.Bcc).ToLower().Contains(To))
                    .OrderByDescending(x => x.SendableDate);

                return items.Take(15).ToList();
            }
        }

        static string GetBodyHtml(string body, bool wasHtml)
        {
            if (wasHtml) return body;

            body = body.HtmlEncode().Replace("\n", "<br/>").Replace("\r", "");
            body = LinkPattern.Replace(body, "<a href=\"$1\" target=\"_parent\">$1</a>");
            return body;
        }

        async Task<string> GenerateInbox()
        {
            var r = new StringBuilder();

            var emails = await GetEmails();

            r.AppendLine("<h2>Emails sent to <u>" + To.Or("ALL") + "</u></h2>");
            r.AppendLine("<table cellspacing='0'>");
            r.AppendLine("<tr>");
            r.AppendLine("<th>Date</th>");
            r.AppendLine("<th>Time</th>");
            r.AppendLine("<th>From</th>");
            r.AppendLine("<th>ReplyTo</th>");
            r.AppendLine("<th>To</th>");
            r.AppendLine("<th>Cc</th>");
            r.AppendLine("<th>Bcc</th>");
            r.AppendLine("<th>Subject</th>");
            r.AppendLine("<th>Attachments</th>");
            r.AppendLine("</tr>");

            if (emails.None())
            {
                r.AppendLine("<tr>");
                r.AppendLine("<td colspan='8'>No emails in the system</td>");
                r.AppendLine("</tr>");
            }
            else
            {
                foreach (var item in emails)
                {
                    r.AppendLine("<tr>");
                    r.AddFormattedLine("<td>{0}</td>", item.SendableDate.ToString("yyyy-MM-dd"));
                    r.AddFormattedLine("<td>{0}</td>", item.SendableDate.ToSmallTime());
                    r.AddFormattedLine("<td>{0}</td>",
                        item.GetEffectiveFromName() + "(" + item.GetEffectiveFromAddress() + ")");
                    r.AddFormattedLine("<td>{0}</td>",
                        item.GetEffectiveReplyToName() + "(" + item.GetEffectiveReplyToAddress() + ")");
                    r.AddFormattedLine("<td>{0}</td>", item.To);
                    r.AddFormattedLine("<td>{0}</td>", item.Cc);
                    r.AddFormattedLine("<td>{0}</td>", item.Bcc);

                    r.AddFormattedLine("<td><a href='/?Web.Test.Command=testEmail&id={0}&to={1}&ReturnUrl={2}'>{3}</a></td>",
                        item.GetId(), To, ReturnUrl.UrlEncode(), item.Subject.Or("[NO SUBJECT]").HtmlEncode());

                    r.AddFormattedLine("<td>{0}</td>", GetAttachmentLinks(item));

                    r.AppendLine("</tr>");
                }
            }

            r.AppendLine("</table>");

            return r.ToString();
        }

        async Task<string> GetAttachmentLinks(IEmailMessage email)
        {
            return (await email.Attachments.OrEmpty().Split('|').Trim()
                .Select(async f => $"<form action='/?Web.Test.Command=testEmail&To={To}&ReturnUrl={ReturnUrl.UrlEncode()}' " +
                $"method='post'><input type=hidden name='attachmentInfo' value='{f.HtmlEncode()}'/><a href='#' " +
                $"onclick='this.parentElement.submit()'>{(EmailService.ParseAttachment(f))?.Name.HtmlEncode()}</a></form>")
                .AwaitAll()).ToString("");
        }

        bool IsTextFile(string fileName) => Path.GetExtension(fileName).ToLower().IsAnyOf(".txt", ".csv", ".xml");

        string GenerateEmailView()
        {
            var r = new StringBuilder();

            r.AppendLine("<a href='/?Web.Test.Command=testEmail&to=" + To + "'>&lt;&lt; Back</a>");
            r.AppendLine("<h2>Subject: <u>" + Email.Subject.Or("[NO SUBJECT]") + "</u></h2>");
            r.AppendLine("<table cellspacing='0'>");

            var body = GetBodyHtml(Email.Body.Or("[EMPTY BODY]"), Email.Html);

            var toShow = new Dictionary<string, object> {
                {"Date", Email.SendableDate.ToString("yyyy-MM-dd") +" at " + Email.SendableDate.ToString("HH:mm") },
                {"From", Email.GetEffectiveFromAddress()},
                {"ReplyTo", Email.GetEffectiveReplyToAddress()},
                {"To", Email.GetEffectiveToAddresses()},
                {"Bcc", Email.GetEffectiveBccAddresses().ToString(", ")},
                {"Cc", Email.GetEffectiveCcAddresses().ToString(", ")},
                {"Subject", Email.Subject.Or("[NO SUBJECT]").HtmlEncode().WithWrappers("<b>", "</b>")},
                {"Body", body.WithWrappers("<div class='body'>" ,"</div>") },
                {"Attachments", GetAttachmentLinks(Email) }
            };

            foreach (var item in toShow.Where(x => x.Value.ToStringOrEmpty().HasValue()))
            {
                r.AppendLine("<tr>");
                r.AddFormattedLine("<td class='label'>{0}:</td>", item.Key.HtmlEncode());
                r.AddFormattedLine("<td>{0}</td>", item.Value);

                r.AppendLine("</tr>");
            }

            r.AppendLine("</table>");

            return r.ToString();
        }
    }
}
