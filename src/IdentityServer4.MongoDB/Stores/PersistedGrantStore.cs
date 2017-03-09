﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Interfaces;
using IdentityServer4.MongoDB.Mappers;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Stores
{
    public class PersistedGrantStore : IPersistedGrantStore
    {
        private readonly IPersistedGrantDbContext _context;
        private readonly ILogger _logger;

        public PersistedGrantStore(IPersistedGrantDbContext context, ILogger<PersistedGrantStore> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task StoreAsync(PersistedGrant token)
        {
            var existing = _context.PersistedGrants.AsQueryable().SingleOrDefault(x => x.Key == token.Key);
            try
            {
                if (existing == null)
                {
                    _logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                    var persistedGrant = token.ToDocument();
                    _context.PersistedGrants.Add(persistedGrant);
                }
                else
                {
                    _logger.LogDebug("{persistedGrantKey} found in database", token.Key);

                    token.UpdateEntity(existing);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
            }

            return Task.FromResult(0);
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = _context.PersistedGrants.AsQueryable().FirstOrDefault(x => x.Key == key);
            var model = persistedGrant?.ToModel();

            _logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return Task.FromResult(model);
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = _context.PersistedGrants.AsQueryable().Where(x => x.SubjectId == subjectId).ToList();
            var model = persistedGrants.Select(x => x.ToModel());

            _logger.LogDebug("{persistedGrantCount} persisted grants found for {subjectId}", persistedGrants.Count, subjectId);

            return Task.FromResult(model);
        }

        public Task RemoveAsync(string key)
        {
            var persistedGrant = _context.PersistedGrants.AsQueryable().FirstOrDefault(x => x.Key == key);
            if (persistedGrant!= null)
            {
                _logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

                try
                {

                    _context.PersistedGrants.Remove(persistedGrant);

                }
                catch(Exception ex)
                {
                    _logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
                }
            }
            else
            {
                _logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
            }

            return Task.FromResult(0);
        }

        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            var persistedGrants = _context.PersistedGrants.AsQueryable().Where(x => x.SubjectId == subjectId && x.ClientId == clientId).ToList();

            _logger.LogDebug("removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}", persistedGrants.Count, subjectId, clientId);

            try
            {
                _context.PersistedGrants.RemoveRange(persistedGrants);

            }
            catch (Exception ex)
            {
                _logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}: {error}", persistedGrants.Count, subjectId, clientId, ex.Message);
            }

            return Task.FromResult(0);
        }

        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var persistedGrants = _context.PersistedGrants.AsQueryable().Where(x =>
                x.SubjectId == subjectId &&
                x.ClientId == clientId &&
                x.Type == type).ToList();

            _logger.LogDebug("removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}", persistedGrants.Count, subjectId, clientId, type);

            try
            {
                _context.PersistedGrants.RemoveRange(persistedGrants);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("exception removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}: {error}", persistedGrants.Count, subjectId, clientId, type, ex.Message);
            }

            return Task.FromResult(0);
        }
    }
}