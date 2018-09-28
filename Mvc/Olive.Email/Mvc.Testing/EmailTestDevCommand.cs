using Microsoft.AspNetCore.Http;
using Olive.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Olive.Email
{
    public class EmailTestDevCommand : IDevCommand
    {
        IEmailAttachmentSerializer AttachmentSerializer;
        IMailMessageCreator MessageCreator;
        Attachment AttachmentFile;

        static readonly Regex LinkPattern = new Regex("(https?://[^ ]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public EmailTestDevCommand(IEmailAttachmentSerializer attachmentSerializer,
            IMailMessageCreator messageCreator)
        {
            AttachmentSerializer = attachmentSerializer;
            MessageCreator = messageCreator;
        }

        HttpRequest Request => Context.Current.Request();

        HttpResponse Response => Context.Current.Response();

        async Task<IEmailMessage> Email()
        {
            if (!Request.Has("id")) return default(IEmailMessage);
            else using (new SoftDeleteAttribute.Context(bypassSoftdelete: true))
                    return await Request.Get<IEmailMessage>("id");
        }

        string To => Request.Param("to").ToStringOrEmpty().ToLower();

        string ReturnUrl => Request.GetReturnUrl();

        public string Name => "outbox";

        public string Title => "Outbox...";

        public async Task<string> Run()
        {
            await Initialize();
            return await Process();
        }

        public Task Initialize()
        {
            if (Request.Has("attachmentInfo"))
                AttachmentFile = AttachmentSerializer.Parse(Request.Param("attachmentInfo"));

            return Task.CompletedTask;
        }

        public async Task<string> Process()
        {
            string response;
            if (AttachmentFile != null)
            {
                if (IsTextFile(AttachmentFile.Name))
                    response = $"<a href='/cmd/{Name}?to={To}'>&lt;&lt; Back to emails</a><pre>" +
                        (await AttachmentFile.ContentStream.ReadAllText()).HtmlEncode() + "</pre>";
                else
                {
                    await Response.Dispatch(await AttachmentFile.ContentStream.ReadAllBytesAsync(),
                        AttachmentFile.Name);
                    return default(string);
                }
            }
            else if (await Email() == null)
            {
                response = await GenerateInbox();
            }
            else
            {
                response = await GenerateEmailView();
            }

            return Dispatch(response);
        }

        string Dispatch(string response)
        {
            var r = new StringBuilder();
            r.AppendLine("<html>");
            r.AppendLine("<head>");
            r.AppendLine("<style>");
            r.AppendLine("body {font-family:Arial; background:#fff; }");
            r.AppendLine("td, th {border:1px solid #999; padding:5px; font-size:9pt;}");
            r.AppendLine("th {background:#eee;}");
            r.AppendLine("a { color: blue; }");
            r.AppendLine(".exit { background: #444; color:#fff; padding:4px 10px; display:inline-block; float:right; margin-top:10px; border-radius: 10px; text-decoration:none;}");
            r.AppendLine(".body { background: #f0f0f0; }");
            r.AppendLine(".label { color: #888; width:100px; }");
            r.AppendLine("</style>");
            r.AppendLine("</head>");
            r.AppendLine("<body>");

            r.AppendLine(response);

            r.AppendLine("<a class='exit' href='{0}'>Exit Mailbox</a>".FormatWith(ReturnUrl));

            // TDD hack:
            r.AppendLine("<a style='display:none;' href='/cmd/db-restart'>Restart Temp Database</a>");

            r.AppendLine("</body></html>");
            return r.ToString();
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
            return LinkPattern.Replace(body, "<a href=\"$1\" target=\"_parent\">$1</a>");
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
                    var mail = await MessageCreator.Create(item);

                    r.AppendLine("<tr>");
                    r.AddFormattedLine("<td>{0}</td>", item.SendableDate.ToString("yyyy-MM-dd"));
                    r.AddFormattedLine("<td>{0}</td>", item.SendableDate.ToSmallTime());
                    r.AddFormattedLine("<td>{0}</td>", mail.From.DisplayName + "(" + mail.From.Address + ")");
                    r.AddFormattedLine("<td>{0}</td>",
                        mail.ReplyToList.First().DisplayName + "(" + mail.ReplyToList.First().Address + ")");
                    r.AddFormattedLine("<td>{0}</td>", item.To);
                    r.AddFormattedLine("<td>{0}</td>", item.Cc);
                    r.AddFormattedLine("<td>{0}</td>", item.Bcc);

                    var text = item.Subject.Or("[NO SUBJECT]").HtmlEncode();
                    var url = $"/cmd/{Name}?id={item.GetId()}&to={To}&ReturnUrl={ReturnUrl.UrlEncode()}";
                    r.AppendLine($"<td><a href='{url}'>{text}</a></td>");

                    r.AddFormattedLine("<td>{0}</td>", GetAttachmentLinks(item));

                    r.AppendLine("</tr>");
                }
            }

            r.AppendLine("</table>");

            return r.ToString();
        }

        string GetAttachmentLinks(IEmailMessage email)
        {
            return email.Attachments.OrEmpty().Split('|').Trim()
                .Select(f =>
              {
                  var att = AttachmentSerializer.Parse(f)?.Name.HtmlEncode();

                  return $"<form action='/cmd/{Name}?To={To}&ReturnUrl={ReturnUrl.UrlEncode()}' " +
                     $"method='post'><input type=hidden name='attachmentInfo' value='{f.HtmlEncode()}'/><a href='#' " +
                     $"onclick='this.parentElement.submit()'>{att}</a></form>";
              }).ToString("");
        }

        bool IsTextFile(string fileName) => Path.GetExtension(fileName).ToLower().IsAnyOf(".txt", ".csv", ".xml");

        async Task<string> GenerateEmailView()
        {
            var email = await Email();
            var r = new StringBuilder();

            var mail = await MessageCreator.Create(email);

            r.AppendLine($"<a href='/cmd/{Name}?to={To}'>&lt;&lt; Back</a>");
            r.AppendLine("<h2>Subject: <u>" + mail.Subject + "</u></h2>");
            r.AppendLine("<table cellspacing='0'>");

            var body = GetBodyHtml(email.Body.Or("[EMPTY BODY]"), email.Html);

            var toShow = new Dictionary<string, object> {
                {"Date", email.SendableDate.ToString("yyyy-MM-dd") +" at " +email.SendableDate.ToString("HH:mm") },
                {"From", mail.From?.Address},
                {"ReplyTo", mail.ReplyToList.FirstOrDefault()?.Address},
                {"To", mail.To.Select(x=>x.Address).ToString(", ")},
                {"Cc", mail.CC.Select(x=>x.Address).ToString(", ")},
                { "Bcc", mail.Bcc.Select(x=>x.Address).ToString(", ")},
                {"Subject", mail.Subject.HtmlEncode().WithWrappers("<b>", "</b>")},
                {"Body", body.WithWrappers("<div class='body'>" ,"</div>") },
                {"Attachments", GetAttachmentLinks(email) }
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

        public bool IsEnabled() => true;
    }
}