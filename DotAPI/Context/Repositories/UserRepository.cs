using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        string UserPasswordHash(User user);
        bool AssignUserPassword(User user, UserPassword password);
    }
    public class UserRepository : Repository<AccessContext, User>, IUserRepository
    {
        //private ClaimsPrincipal _currentUser { get; set; }

        public UserRepository(AccessContext context, IConfiguration configuration)
            : base(context, context, configuration)
        {
        }

        public string UserPasswordHash(User user)
        {
            User safeUser = this.FindAll().Where(u => u.Id == user.Id.Value).FirstOrDefault();

            if (safeUser == null)
                return null;
                
            return (from s in dbContext.UserPasswords
                   where
                    s.UserId == safeUser.Id.Value
                    && s.ArchivedStamp == null
                   select s.Password).FirstOrDefault();
        }

        public bool AssignUserPassword(User user, UserPassword password)
        {
            User safeUser = this.FindAll().Where(u => u.Id == user.Id.Value).FirstOrDefault();

            // TO DO: add check if the new password matches an old password and determine if it is allowed, may need config setting
            
            if (safeUser != null && password != null)
            {
                if (user.CurrentUserPasswordId == password.Id) {

                    // expire any old passwords
                    foreach(UserPassword oldPwd in dbContext.UserPasswords
                        .Where(up => up.UserId == user.Id.Value && up.ArchivedStamp == null)
                    )
                    {
                        oldPwd.ArchivedStamp = DateTime.Now;
                    }

                    dbContext.UserPasswords.Add(password);
                    dbContext.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public override IQueryable<User> FindAll()
        {
            return from s in dbContext.Users
                   where s.ArchivedStamp == null
                   select s;
        }

        public override User Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override User FindOneBy(Expression<Func<User, bool>> criteria)
        {
            return (from r in dbContext.Users select r).Where(criteria).FirstOrDefault();
        }

        public override IQueryable<User> FindBy(Func<User, bool>[] criteria)
        {
            IQueryable<User> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override User Create(User entity)
        {
            User currentUser = this.CurrentUser();

            if (!entity.Id.HasValue)
                entity.Id = Guid.NewGuid();

            entity.LastUpdatedStamp = DateTime.Now;
            entity.CreatedStamp = DateTime.Now;

            dbContext.Users.Add(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override User Update(User current, User entity)
        {
            User currentUser = this.CurrentUser();

            current.UserName = entity.UserName;
            current.DisplayName = entity.DisplayName;
            current.FirstName = entity.FirstName;
            current.LastName = entity.LastName;
            current.Email = entity.Email;
            current.Phone = entity.Phone;
            current.FirstLoginStamp = entity.FirstLoginStamp;
            current.LastLoginStamp = entity.LastLoginStamp;
            current.ExternalId = entity.ExternalId;
            
            if (currentUser != null)
            {
                if (currentUser.Id == current.Id)
                {
                    // TO DO: Upade user password with new hashed value
                    //current.UserPassword = HASHED PASSWORD GENERATOR(entity.UserPassword);
                }
            }
            current.enabled = entity.enabled;
            current.LastUpdatedStamp = DateTime.Now;

            dbContext.SaveChanges();

            return current;
        }

        public override User Delete(User entity)
        {
            User dbEntity = this.Find(entity.Id.Value);
            User currentUser = this.CurrentUser();

            if (dbEntity != null)
            {
                dbEntity.LastUpdatedStamp = DateTime.Now;
                dbEntity.ArchivedStamp = DateTime.Now;
                dbContext.SaveChanges();

                return dbEntity;
            }

            return entity;
        }

        public override User Restore(Guid id)
        {
            User entity = (from s in dbContext.Users where s.Id == id select s).First();
            User currentUser = this.CurrentUser();

            if (entity != null)
            {
                entity.ArchivedStamp = null;
                entity.LastUpdatedStamp = System.DateTime.Now;
                dbContext.SaveChanges();
            }

            return entity;
        }
    }
}