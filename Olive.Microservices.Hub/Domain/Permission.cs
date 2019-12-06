namespace Domain
{
    using Olive;

    partial class Permission
    {
        internal static Permission GetOrCreate(string name)
        {
            var result = FindByName(name).RiskDeadlockAndAwaitResult();
            if (result == null)
            {
                result = new Permission { Name = name };
                Context.Current.Database().Save(result).RiskDeadlockAndAwaitResult();
            }

            return result;
        }
    }
}