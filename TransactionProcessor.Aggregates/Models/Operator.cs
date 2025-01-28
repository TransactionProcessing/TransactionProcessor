using System;

namespace TransactionProcessor.Aggregates.Models
{
    internal record Operator(bool IsDeleted = false);
}