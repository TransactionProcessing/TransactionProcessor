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

    public static T GetProperty<T>(IDomainEvent domainEvent, String propertyName, Boolean ignoreCase=false)
    {
        try{
            PropertyInfo propertyInfo = null;
            PropertyInfo[] properties = domainEvent.GetType()
                                                   .GetProperties();
            if (ignoreCase)
            {
                propertyInfo = properties.SingleOrDefault(p => String.Compare(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase) == 0);
            }
            else{
                propertyInfo = properties.SingleOrDefault(p => p.Name == propertyName);
            }
            
            if (propertyInfo != null)
            {
                return (T)propertyInfo.GetValue(domainEvent);
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