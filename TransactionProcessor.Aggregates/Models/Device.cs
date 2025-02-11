using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Aggregates.Models;

[ExcludeFromCodeCoverage]
internal record Device(Guid DeviceId, String DeviceIdentifier, Boolean IsEnabled = true);