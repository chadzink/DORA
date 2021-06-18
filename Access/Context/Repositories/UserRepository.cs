using System;
using System.Linq;
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

        public override User Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override IQueryable<User> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<User> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override User CopyEntity(User current, User updates)
        {
            current.UserName = updates.UserName;
            current.DisplayName = updates.DisplayName;
            current.FirstName = updates.FirstName;
            current.LastName = updates.LastName;
            current.Email = updates.Email;
            current.Phone = updates.Phone;
            current.FirstLoginStamp = updates.FirstLoginStamp;
            current.LastLoginStamp = updates.LastLoginStamp;
            current.ExternalId = updates.ExternalId;
            // cannot set the user password in common create, need to use special AssignUserPassword above
            current.enabled = updates.enabled;
            current.LastUpdatedStamp = DateTime.Now;

            return current;
        }

        public override User[] JoinAllAndSort(User[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }

        public override User[] Delete(User[] entity)
        {
            entity = this.JoinAllAndSort(entity);

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