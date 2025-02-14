using System;

namespace TransactionProcessor.Models.Merchant;

public record Device(Guid DeviceId, String DeviceIdentifier, Boolean IsEnabled = true);