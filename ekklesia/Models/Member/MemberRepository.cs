﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ekklesia.Models.Member
{
    public interface IMemberRepository
    {
        Member GetMember(int id);
        IEnumerable<Member> GetMembers();
        Member Add(Member member);
        Member Update(Member member);
        Member Delete(int id);

    }
    public class MemberRepository : IMemberRepository
    {
        private readonly ApplicationContext applicationContext;

        public MemberRepository(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public Member Add(Member member)
        {
            applicationContext.Members.Add(member);
            applicationContext.SaveChanges();
            return member;
        }

        public Member Delete(int id)
        {
            Member member = applicationContext.Members.Find(id);
            if (member != null)
            {
                applicationContext.Members.Remove(member);
                applicationContext.SaveChanges();
            }
            return member;

        }

        public Member GetMember(int id)
        {
            return applicationContext.Members.Find(id);
        }

        public IEnumerable<Member> GetMembers()
        {
            return applicationContext.Members;
        }

        public Member Update(Member alteredMember)
        {
            var member = applicationContext.Members.Attach(alteredMember);
            member.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            applicationContext.SaveChanges();
            return alteredMember;

        }
    }
}
