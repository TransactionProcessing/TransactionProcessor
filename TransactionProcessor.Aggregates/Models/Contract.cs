﻿using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Aggregates.Models;

[ExcludeFromCodeCoverage]
internal record Contract
{
    public Contract(Guid ContractId, Boolean IsDeleted = false)
    {
        this.ContractId = ContractId;
        this.IsDeleted = IsDeleted;
        this.ContractProducts = new List<Guid>();
    }

    public List<Guid> ContractProducts { get; init; }
    public Guid ContractId { get; init; }
    public Boolean IsDeleted { get; init; }
}