using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Export
{
    // The following piece of code is copied here due to compile error in data project. 
    // I will decide about it later.
    public static class ExtensionMethods
    {
        public static async Task<FileInfo> ToCsvFile(this DataAccessProfiler.ReportRow[] lines)
        {
            var exporter = new Exporter("Sql.Profile.Report");

            exporter.AddColumn("Command");
            exporter.AddColumn("Calls");
            exporter.AddColumn("Total ms");
            exporter.AddColumn("Longest ms");
            exporter.AddColumn("Average ms");
            exporter.AddColumn("Median ms");

            foreach (var line in lines.OrderByDescending(x => x.Total))
                exporter.AddRow(line.Command, line.Calls, line.Total, line.Longest, line.Average, line.Median);

            var result = exporter.Generate(Format.Csv);

            var file = AppDomain.CurrentDomain.WebsiteRoot().GetOrCreateSubDirectory("--Sql-Profiler")
                .GetFile(LocalTime.Now.ToOADate() + ".csv");

            await file.WriteAllTextAsync(result);

            return file;
        }
    }
}
