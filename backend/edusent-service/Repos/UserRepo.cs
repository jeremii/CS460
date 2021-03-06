﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using edusent_service.EF;
using edusent_service.Repos.Base;
using edusent_service.Models;
using edusent_service.Repos.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using edusent_service.Models.ViewModels;

namespace edusent_service.Repos
{
    public class UserRepo : IUserRepo
    {
        public readonly EdusentContext Db;
        public DbSet<User> Table { get; }
        public ISubjectRepo _subjectRepo { get; set; }

        public UserRepo(DbContextOptions<EdusentContext> options, ISubjectRepo subjectRepo)
        {
            Db = new EdusentContext(options);
            Table = Db.Set<User>();
            _subjectRepo = subjectRepo;
        }


        private bool _disposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                //Free any other managed objects here
            }
            Db.Dispose();
            _disposed = true;
        }


        public int SaveChanges()
        {
            try
            {
                return Db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task<IEnumerable<User>> FindUsers(string keyword)
        {
            IEnumerable<User> results = Table
                .Where(e =>
                       e.FirstName.ToLower().Contains(keyword.ToLower()) ||
                       e.LastName.ToLower().Contains(keyword.ToLower()));

            List<User> users = new List<User>();

            foreach (User user in results) users.Add(Get(user.Id).Result);

            return users;
        }

        public IEnumerable<User> GetAll()
        {
            IEnumerable<User> results = Table.OrderBy(x => x.LastName);
            return results;
        }
        
        public async Task<User> Get(string id)
        {
            return await Table.FindAsync(id);
        }

        public async Task<int> Update(User user, bool persist = true)
        {
            //user.ConcurrencyStamp = System.Guid.NewGuid().ToString();
            user.ConcurrencyStamp = Table.AsNoTracking()
                .Where(w => w.Id == user.Id)
                .FirstOrDefault().ConcurrencyStamp;
            Table.Update(user);
            return persist ? SaveChanges() : 0;
        }
        public async Task<string> FindIdByName(string first, string last)
        {
            User user = Table.Where(e =>
                            e.FirstName.ToLower().Equals(first.ToLower()) ||
                            e.LastName.ToLower().Equals(last.ToLower())).First();
            return user.Id;
        }
        public async Task<string> FindIdByEmail(string email)
        {
            User user = Table.Where(e =>
                            e.Email.ToLower().Equals(email.ToLower())).First();
            return user.Id;
        }
        public async Task<string> FindIdByUsername(string username)
        {
            User user = Table.Where(e =>
                            e.UserName.ToLower().Equals(username.ToLower())).First();
            return user.Id;
        }
        public async Task<string> GetUsernameByEmail(string email)
        {
            User user = Table.Where(e =>
           e.Email.ToLower().Equals(email.ToLower())
           ).First();
            return user.UserName;
        }
        public async Task<bool> UsernameExists(string username)
        {
            int count = Table.Where(e => e.UserName.ToLower().Equals(username.ToLower())).Count();

            return count > 0 ? true : false;
        }
        public async Task<bool> EmailExists(string email)
        {
            int count = Table.Where(e => e.Email.ToLower().Equals(email.ToLower())).Count();
            return count > 0 ? true : false;
        }
        public async Task<TeacherOverviewViewModel> GetTeacherOverview(string userId)
        {
            User data = await Table.Where(x => x.Id == userId).FirstAsync();

            
            TeacherOverviewViewModel teacher = new TeacherOverviewViewModel
            {
                FullName = data.FirstName + " " + data.LastName,
                Rating = data.Rating + " STARS",
                Subjects = _subjectRepo.GetSubjectsById(userId),
                UserId = userId,
            };
            return teacher;
        }
    }
}