using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Aws.Ses.AutoFetch
{
    class DatabaseTableService
    {
        const string CREATE_TABLE_COMMAND = @"
if not exists (select * from sysobjects where name='MailMessages' and xtype='U')
/****** Object:  Table [dbo].[MailMessages]    ******/
CREATE TABLE [dbo].[MailMessages] (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    [From] nvarchar(2000)  NULL,
    [To] nvarchar(MAX)  NULL,
    Bcc nvarchar(2000)  NULL,
    Cc nvarchar(2000)  NULL,
    Subject nvarchar(2000)  NULL,
    Sender nvarchar(2000)  NULL,
    Body nvarchar(MAX)  NULL,
    [Date] datetime2  NOT NULL,
    Processed bit  NOT NULL,
    Bucket nvarchar(200)  NULL,
    Attachments nvarchar(MAX)  NULL,
    DateDownloaded datetime2  NOT NULL,
    MessageId nvarchar(200)  NULL,
    ReplyTo nvarchar(200)  NULL
);

if not exists (select * from sysobjects where name='MailMessageAttachments' and xtype='U')
/****** Object:  Table [dbo].[MailMessageAttachments]    ******/
CREATE TABLE [dbo].[MailMessageAttachments] (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    MailMessage uniqueidentifier  NULL,
    Attachment_FileName nvarchar(1500)  NULL
);
";
        internal static Task EnsureDatabaseTable<T,A>() => Entities.Data.DataAccess.Create().ExecuteNonQuery(CREATE_TABLE_COMMAND);
    }
}
