using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        string UserPasswordHash(User user);
        string AssignUserPassword(User user, UserPassword password);
    }
    public class UserRepository : Repository<AccessContext, User>, IUserRepository
    {
        public UserRepository(AccessContext context, IConfiguration config)
            : base(context, config)
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

        public string AssignUserPassword(User user, UserPassword password)
        {
            User safeUser = this.FindAll().Where(u => u.Id == user.Id.Value).FirstOrDefault();

            // check if user password already exist historically for the user
            int hasOldPassword = (from up in dbContext.UserPasswords
                                    where
                                        up.Password == password.Password
                                        && up.UserId == user.Id.Value
                                        && up.CreatedStamp <= DateTime.Now.AddDays(90)
                                    select up).Count();

            if (hasOldPassword > 0)
                return "Error: Password used within last 90 days.";

            if (safeUser != null && password != null)
            {
                // expire any old passwords
                foreach(UserPassword oldPwd in dbContext.UserPasswords
                    .Where(up => up.UserId == safeUser.Id.Value && up.ArchivedStamp == null)
                )
                {
                    oldPwd.ArchivedStamp = DateTime.Now;
                }

                safeUser.CurrentUserPasswordId = password.Id.Value;
                dbContext.UserPasswords.Add(password);
                dbContext.SaveChanges();

                return "Success";
            }

            return "Error: User not matched in database or password object is null.";
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

        public override User Update(User current, User previous)
        {
            bool hasAccess = (from s in this.FindAll() where s.Id == previous.Id select s).FirstOrDefault() != null;

            if (hasAccess)
            {
                current.UserName = previous.UserName;
                current.DisplayName = previous.DisplayName;
                current.FirstName = previous.FirstName;
                current.LastName = previous.LastName;
                current.Email = previous.Email;
                current.Phone = previous.Phone;
                current.FirstLoginStamp = previous.FirstLoginStamp;
                current.LastLoginStamp = previous.LastLoginStamp;
                current.ExternalId = previous.ExternalId;

                // cannot set the user password in common create, need to use special AssignUserPassword above

                current.enabled = previous.enabled;
                current.LastUpdatedStamp = DateTime.Now;

                dbContext.SaveChanges();
            }

            return current;
        }

        public override User SaveChanges(User entity)
        {
            if (entity.Id.HasValue)
            {
                User existingUser = (from s in this.FindAll() where s.Id == entity.Id select s).FirstOrDefault();

                // cannot change the user password, must be the same
                if (existingUser != null && entity.CurrentUserPasswordId == existingUser.CurrentUserPasswordId)
                {
                    dbContext.Users.Attach(entity);
                    dbContext.SaveChanges();

                    return entity;
                }
            }

            return null;
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