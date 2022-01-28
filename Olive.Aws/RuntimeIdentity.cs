using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Olive.Aws
{
    /// <summary>
    /// Specifies the current application's runtime IAM identity.
    /// All AWS services, e.g. S3, etc, should use this.
    /// </summary>
    public class RuntimeIdentity
    {
        const string VARIABLE = "AWS_RUNTIME_ROLE_ARN";
        static string RoleArn;

        public static async Task Load(IConfiguration config)
        {
            RoleArn = Environment.GetEnvironmentVariable(VARIABLE);
            Environment.SetEnvironmentVariable(VARIABLE, null);

            await AssumeRole.Assume(RoleArn);
            AssumeRole.KeepRenewing();
        }
    }
}