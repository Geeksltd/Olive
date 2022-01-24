using LocalStack.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Olive.Entities.Data
{
    public static class DynamoRegistrationExtensions
    {
        public static IServiceCollection AddDynamoDB(this IServiceCollection @this, IConfiguration config)
        {
            @this.AddDatabase(config);
            @this.AddLocalStack(config);
            @this.AddAwsService<Amazon.DynamoDBv2.IAmazonDynamoDB>();

            return @this;
        }
    }
}