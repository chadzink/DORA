using System.Linq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Helpers;
using Microsoft.Extensions.Configuration;
using DORA.DotAPI.Context;
using Microsoft.EntityFrameworkCore;
using DORA.DotAPI.Services;

namespace DORA.DotAPI.Common
{
    public interface IViewRepository<TEntity>
    {
        IQueryable<TEntity> FindAll();

        TEntity Find(Guid id);

        TEntity FindOneBy(Expression<Func<TEntity, bool>> criteria);

        IQueryable<TEntity> FindBy(Func<TEntity, bool>[] criteria);
    }

    public interface ICrudRepository<TEntity> : IViewRepository<TEntity>
    {
        TEntity Create(TEntity entity);

        TEntity Update(TEntity dbEntity, TEntity entity);

        TEntity SaveChanges(TEntity dbEntity);

        TEntity Delete(TEntity entity);
    }

    public interface IRepositoryView<TEntity> : IViewRepository<TEntity>
    {
        void SetUser(ClaimsPrincipal user);

        ClaimsPrincipal GetPrincipalUser();

        User CurrentUser();

        bool ReadAccess(string controllerName, IEnumerable<Claim> roleClaims);
    }

    public interface IRepository<TEntity> : ICrudRepository<TEntity>
    {
        void SetUser(ClaimsPrincipal user);

        ClaimsPrincipal GetPrincipalUser();

        User CurrentUser();

        TEntity Restore(Guid id);

        bool CreateAccess(string controllerName, IEnumerable<Claim> roleClaims);
        bool ReadAccess(string controllerName, IEnumerable<Claim> roleClaims);
        bool UpdateAccess(string controllerName, IEnumerable<Claim> roleClaims);
        bool DeleteAccess(string controllerName, IEnumerable<Claim> roleClaims);
    }

    public abstract class _Repository<TContext, TEntity> : IRepositoryView<TEntity>
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly AccessContext _userContext;
        private IConfiguration _configuration;
        private ClaimsPrincipal _user;

        public _Repository(TContext context, IConfiguration config)
        {
            this._dbContext = context;
            this._configuration = config;

            this._userContext = AccessContext.CreateContext();
        }

        public TContext dbContext
        {
            get
            {
                return this._dbContext;
            }
        }

        public AccessContext userContext
        {
            get
            {
                return this._userContext;
            }
        }

        public IConfiguration Config
        {
            get
            {
                return this._configuration;
            }
        }

        public abstract IQueryable<TEntity> FindAll();

        public abstract TEntity Find(Guid id);

        public abstract TEntity FindOneBy(Expression<Func<TEntity, bool>> criteria);

        public abstract IQueryable<TEntity> FindBy(Func<TEntity, bool>[] criteria);

        public void SetUser(ClaimsPrincipal user)
        {
            this._user = user;
        }

        public ClaimsPrincipal GetPrincipalUser()
        {
            return this._user;
        }

        public User CurrentUser()
        {
            User user = null;

            string UserIdStr = null;

            if (this.GetPrincipalUser() != null)
            {
                UserIdStr = this.GetPrincipalUser().Claims.Where(c => c.Type == ClaimTypes.PrimarySid).Select(c => c.Value).SingleOrDefault();
            }

            if (UserIdStr != null && UserIdStr != string.Empty)
            {
                Guid userGuid = new Guid(UserIdStr);

                if (Config["AppSettings:JwtIssuer"] != null
                    && Config["AppSettings:JwtExpiresMinutes"] != null
                    && Config["AppSettings:JwtRefreshExpiresDays"] != null
                )
                {
                    user = JwtToken.AddTokensToUser(
                        _userContext.Users.Where(e => e.Id == userGuid).FirstOrDefault(),
                        Config["AppSettings:Secret"],
                        Config["AppSettings:JwtIssuer"],
                        Config["AppSettings:JwtAudience"],
                        int.Parse(Config["AppSettings:JwtExpiresMinutes"]),
                        int.Parse(Config["AppSettings:JwtRefreshExpiresDays"]),
                        _userContext
                    );
                }
                else
                {
                    user = _userContext.Users.Where(e => e.Id == userGuid).FirstOrDefault();
                }
            }

            return user;
        }

        public IQueryable<TEntity> FindBy(IQueryable<TEntity> query, Func<TEntity, bool>[] criteria)
        {
            foreach (Func<TEntity, bool> criteriaFunc in criteria)
            {
                query = (IQueryable<TEntity>)query.Where(criteriaFunc);
            }

            return query;
        }

        public bool ReadAccess(string resourceCode, IEnumerable<Claim> roleClaims)
        {
            foreach (Claim roleClaim in roleClaims)
            {
                var entity = (from rra in _userContext.RoleResourcesAccesses
                              join ra in _userContext.ResourceAccesses on rra.ResourceAccessId equals ra.Id
                              join rr in _userContext.Resources on rra.ResourceId equals rr.Id
                              join r in _userContext.Roles on rra.RoleId equals r.Id
                              where
                                  r.NameCanonical == roleClaim.Value &&
                                  ra.KeyCode == "READ" &&
                                  rr.KeyCode == resourceCode
                              select rra).FirstOrDefault();
                if (entity != null) return true;
            }

            return false;
        }
    }


    public abstract class RepositoryView<TContext, TEntity> : _Repository<TContext, TEntity>, IRepositoryView<TEntity>
        where TContext : DbContext
    {
        public RepositoryView(TContext context, IConfiguration config)
            : base(context, config)
        { }
    }


    public abstract class Repository<TContext, TEntity> : _Repository<TContext, TEntity>, IRepository<TEntity>
        where TContext : DbContext
    {
        public Repository(TContext context, IConfiguration config)
            : base(context, config)
        { }

        public abstract TEntity Create(TEntity entity);

        public abstract TEntity Update(TEntity dbEntity, TEntity entity);

        public abstract TEntity SaveChanges(TEntity dbEntity);

        public abstract TEntity Delete(TEntity entity);

        public abstract TEntity Restore(Guid id);

        public bool CreateAccess(string resourceCode, IEnumerable<Claim> roleClaims)
        {
            foreach (Claim roleClaim in roleClaims)
            {
                var entity = (from rra in userContext.RoleResourcesAccesses
                                join ra in userContext.ResourceAccesses on rra.ResourceAccessId equals ra.Id
                                join rr in userContext.Resources on rra.ResourceId equals rr.Id
                                join r in userContext.Roles on rra.RoleId equals r.Id
                                where
                                    r.NameCanonical == roleClaim.Value &&
                                    ra.KeyCode == "CREATE" &&
                                    rr.KeyCode == resourceCode
                                select rra).FirstOrDefault();
                if (entity != null) return true;
            }

            return false;
        }

        public bool UpdateAccess(string resourceCode, IEnumerable<Claim> roleClaims)
        {
            foreach (Claim roleClaim in roleClaims)
            {
                var entity = (from rra in userContext.RoleResourcesAccesses
                                join ra in userContext.ResourceAccesses on rra.ResourceAccessId equals ra.Id
                                join rr in userContext.Resources on rra.ResourceId equals rr.Id
                                join r in userContext.Roles on rra.RoleId equals r.Id
                                where
                                    r.NameCanonical == roleClaim.Value &&
                                    ra.KeyCode == "UPDATE" &&
                                    rr.KeyCode == resourceCode
                              select rra).FirstOrDefault();
                if (entity != null) return true;
            }

            return false;
        }

        public bool DeleteAccess(string resourceCode, IEnumerable<Claim> roleClaims)
        {
            foreach (Claim roleClaim in roleClaims)
            {
                var entity = (from rra in userContext.RoleResourcesAccesses
                                join ra in userContext.ResourceAccesses on rra.ResourceAccessId equals ra.Id
                                join rr in userContext.Resources on rra.ResourceId equals rr.Id
                                join r in userContext.Roles on rra.RoleId equals r.Id
                                where
                                    r.NameCanonical == roleClaim.Value &&
                                    ra.KeyCode == "DELETE" &&
                                    rr.KeyCode == resourceCode
                              select rra).FirstOrDefault();
                if (entity != null) return true;
            }

            return false;
        }
    }
}
