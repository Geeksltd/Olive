using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain
{
    public class ReferenceData : IReferenceData
    {
        IDatabase Database;
        public ReferenceData(IDatabase database) => Database = database;

        async Task<T> Create<T>(T item) where T : IEntity
        {
            await Database.Save(item, SaveBehaviour.BypassAll);
            return item;
        }

        public Task Create() => CreatePermissions();

        async Task CreatePermissions()
        {
            var roles = await "https://dashboard.geeksltd.co.uk/@services/people.ashx".AsUri().Download()
             .Get(x => x.To<XElement>().Elements()).Select(n => n.GetValue<string>("Role")).Distinct();

            await roles.SelectAsync(s => Create(new Permission { Name = s }));
        }
    }
}