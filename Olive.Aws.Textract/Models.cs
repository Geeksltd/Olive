using Amazon.Textract;
using Amazon.Textract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Aws.Comprehend
{
    public class TextDetectionBlockResults
    {
        internal TextDetectionBlockResults(GetDocumentTextDetectionResponse response)
        {
            Blocks = response.Blocks;
            NextToken= response.NextToken;
            JobStatus= response.JobStatus;
        }
        public IEnumerable<Block> Blocks { get; set; }
        public string NextToken { get; set; }
        public JobStatus JobStatus { get; set; }
    }
    public class TextDetectionTextResults
    {
        internal TextDetectionTextResults(GetDocumentTextDetectionResponse response)
        {
            Text = response.Blocks.Select(x=>x.Text).ToString("\n");
            NextToken = response.NextToken;
            JobStatus = response.JobStatus;
        }

        public string Text { get; set; }
        public string NextToken { get; set; }
        public JobStatus JobStatus { get; set; }

    }
}
