using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Merchant;

public record Device(Guid DeviceId, String DeviceIdentifier, Boolean IsEnabled = true);