using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DORA.Access.Context.Repositories
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

        public override IQueryable<User> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<User> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override User Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override User FindOneBy(Expression<Func<User, bool>> criteria)
        {
            return (from r in dbContext.Users select r).Where(criteria).FirstOrDefault();
        }

        public override IQueryable<User> FindBy(Expression<Func<User, bool>> criteria)
        {
            IQueryable<User> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override User[] Create(User[] entity)
        {
            foreach (User e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();

                e.LastUpdatedStamp = DateTime.Now;
                e.CreatedStamp = DateTime.Now;
            }

            dbContext.Users.AddRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override User[] Update(User[] current, User[] previous)
        {
            if (current.Length != previous.Length)
                return null;

            // filter & sort out entities that the user doe not have access to
            current = (
                from e in this.FindAll().ToList()
                join c in current on e.Id equals c.Id.Value
                select c
            ).OrderBy(c => c.Id).ToArray();

            // filter and sort
            previous = (
                from c in current
                join p in previous on c.Id.Value equals p.Id.Value
                select p
            ).OrderBy(c => c.Id).ToArray();

            for (int e = 0; e < current.Length; e++)
            {
                current[e].UserName = previous[e].UserName;
                current[e].DisplayName = previous[e].DisplayName;
                current[e].FirstName = previous[e].FirstName;
                current[e].LastName = previous[e].LastName;
                current[e].Email = previous[e].Email;
                current[e].Phone = previous[e].Phone;
                current[e].FirstLoginStamp = previous[e].FirstLoginStamp;
                current[e].LastLoginStamp = previous[e].LastLoginStamp;
                current[e].ExternalId = previous[e].ExternalId;
                // cannot set the user password in common create, need to use special AssignUserPassword above
                current[e].enabled = previous[e].enabled;
                current[e].LastUpdatedStamp = DateTime.Now;
            }

            dbContext.Users.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override User[] SaveChanges(User[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                // cannot change the user password, must be the same
                where e.CurrentUserPasswordId == p.CurrentUserPasswordId
                select p
            ).ToArray();

            dbContext.Users.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override User[] Delete(User[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            foreach (User dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
                dbEntity.LastUpdatedStamp = DateTime.Now;
            }

            dbContext.Users.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override User[] Restore(Guid[] id)
        {
            User[] entity = (
                from e in dbContext.Users
                where id.Contains(e.Id.Value)
                select e
            ).ToArray();

            foreach (User e in entity)
            {
                e.ArchivedStamp = null;
                e.LastUpdatedStamp = DateTime.Now;
            }

            dbContext.SaveChanges();

            return entity;
        }
    }
}