namespace TransactionProcessor.ProjectionEngine.Common;

using System.Reflection;
using Shared.DomainDrivenDesign.EventSourcing;

public static class DomainEventHelper
{
    public static Boolean HasProperty(IDomainEvent domainEvent,
                                      String propertyName)
    {
        PropertyInfo propertyInfo = domainEvent.GetType()
                                               .GetProperties()
                                               .SingleOrDefault(p => p.Name == propertyName);

        return propertyInfo != null;
    }

    public static T GetPropertyIgnoreCase<T>(IDomainEvent domainEvent, String propertyName)
    {
        try
        {
            var f = domainEvent.GetType()
                               .GetProperties()
                               .SingleOrDefault(p => String.Compare(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase) == 0);

            if (f != null)
            {
                return (T)f.GetValue(domainEvent);
            }
        }
        catch
        {
            // ignored
        }

        return default(T);
    }

    public static T GetProperty<T>(IDomainEvent domainEvent, String propertyName)
    {
        try
        {
            var f = domainEvent.GetType()
                               .GetProperties()
                               .SingleOrDefault(p => p.Name == propertyName);

            if (f != null)
            {
                return (T)f.GetValue(domainEvent);
            }
        }
        catch
        {
            // ignored
        }

        return default(T);
    }

    public static Guid GetEstateId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "EstateId");

    
    public static Guid GetMerchantId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "MerchantId");
    
}