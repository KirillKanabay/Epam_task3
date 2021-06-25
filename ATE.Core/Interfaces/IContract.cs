﻿using ATE.Core.Entities;
using ATE.Core.Entities.Users;

namespace ATE.Core.Interfaces
{
    public interface IContract
    {
        string PhoneNumber { get; }
        Tariff Tariff { get; }
        User User { get; }
    }
}