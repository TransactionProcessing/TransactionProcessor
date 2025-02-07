namespace TransactionProcessor.Aggregates.Models;

internal record Device(Guid DeviceId, String DeviceIdentifier, Boolean IsEnabled = true);