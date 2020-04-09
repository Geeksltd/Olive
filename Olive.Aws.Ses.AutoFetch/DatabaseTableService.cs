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
CREATE TABLE [dbo].[MailMessages](
	[ID] [uniqueidentifier] NOT NULL,
	[From] [nvarchar](1000) NOT NULL,
	[Subject] [nvarchar](1000) NULL,
	[To] [nvarchar](1000) NULL,
	[Bcc] [nvarchar](1000) NULL,
	[Cc] [nvarchar](1000) NULL,
	[Date] [datetime] NOT NULL,
	[Sender] [nvarchar](300) NULL,
	[Body] [nvarchar](max) NULL,
) ON [PRIMARY]
";
        internal static Task EnsureDatabaseTable<T>() => Entities.Data.DataAccess.Create().ExecuteNonQuery(CREATE_TABLE_COMMAND);
    }
}
