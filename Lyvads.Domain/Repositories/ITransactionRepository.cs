﻿

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface ITransactionRepository
{
    IQueryable<Transaction> GetAllPayments();
}