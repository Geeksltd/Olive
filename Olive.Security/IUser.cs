using System.Collections.Generic;
using System.Linq;
using Olive.Entities;

namespace Olive.Security
{
    public interface IUser : IEntity
    {
        IEnumerable<string> GetRoles();
    }
}