﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Interfaces
{
    public interface IPersistedGrantRepository
    {
        IMongoCollection<Documents.PersistedGrant> PersistedGrants { get; set; }
    }
}