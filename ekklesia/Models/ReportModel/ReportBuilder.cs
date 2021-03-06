﻿using ekklesia.Models.EventModel;
using ekklesia.Models.TransactionModel;
using ekklesia.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ekklesia.Models.ReportModel
{
    public interface IReportBuilder
    {
        Task<GroupBasedReportViewModel> FilloutGroupReport(GroupBasedReportViewModel model);
    }

    public class ReportBuilder : IReportBuilder
    {
        private readonly ApplicationContext applicationContext;

        public ReportBuilder(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }


        public async Task<GroupBasedReportViewModel> FilloutGroupReport(GroupBasedReportViewModel model)
        {
            FilloutBaseReport(model);

            var cults = applicationContext.Occasions.OfType<Cult>()
                .Where(o => o.Date > DateTime.Today.AddMonths(-1))
                .Where(c => c.CultType.ToString() == model.ReportType.ToString());

            //ATIVIDADES PARA EVENTOS
            model.ExternalCults = await cults.CountAsync(c => c.Internal == false);

            model.CellsNumber = await applicationContext.Occasions.OfType<Cell>().CountAsync();

            model.Baptized = await applicationContext.Occasions.OfType<Baptism>()
                .Where(b => b.Date > DateTime.Today.AddMonths(-1))
                 .Include(b => b.Baptizeds)
                 .ThenInclude(oc => oc.Member)
                 .CountAsync();

            model.Reunions = await applicationContext.Occasions.OfType<Reunion>()
                .Where(r => r.Date > DateTime.Today.AddMonths(-1))
                .Where(r => r.ReunionType.Equals(ReunionType.LIDERANÇA))
                .CountAsync();

            model.MeetingsWithTheCoordination = await applicationContext.Occasions.OfType<Reunion>()
                .Where(b => b.Date > DateTime.Today.AddMonths(-1))
                 .Where(r => r.ReunionType.Equals(ReunionType.PEDAGÓGICA))
                .CountAsync();

            return model;
        }

        public async void FilloutBiblicalReport(CellBasedReportViewModel model)
        {
            //FilloutBaseReport(model);

            var cells = applicationContext.Occasions.OfType<Cell>();

            model.CoordenationMeatings = await applicationContext.Occasions.OfType<Reunion>()
                .Where(r => r.ReunionType.Equals(ReunionType.LIDERANÇA))
                .CountAsync();

            model.NumberOfBoardMembers = await applicationContext.Members
                .Where(m => m.Position == Position.Lider)
                .CountAsync();

        }

        public async void FilloutCellReport(BiblicalBasedReportViewModel model)
        {
            //FilloutBaseReport(model);

            var biblicalschool = applicationContext.Occasions.OfType<SundaySchool>();

            model.Bibles = await biblicalschool.SumAsync(bs => bs.NumberOfBibles);

            model.Visitants = await biblicalschool.SumAsync(bs => bs.Visitants);

        }

        private void FilloutBaseReport(ReportCreateViewModel model)
        {
            Task[] tasks = {
                Task.Factory.StartNew(() => FillOutEventBaseReport(model)),
                Task.Factory.StartNew(() => FillOutMembersBaseReport(model)),
                Task.Factory.StartNew(() => FillOutTransactionBaseReport(model))
            };

            Task.WaitAll(tasks);

        }

        private async Task FillOutMembersBaseReport(ReportCreateViewModel model)
        {
            model.AllMembers = await GetAllMembersAsSelectList();
        }

        private async Task FillOutEventBaseReport(ReportCreateViewModel model)
        {
            model.Reunions = await applicationContext.Occasions.OfType<Reunion>()
                .Where(o => o.Date > DateTime.Today.AddMonths(-1))
                .CountAsync();
            model.Reunions = await applicationContext.Occasions.OfType<Cell>()
                .Where(o => o.Date > DateTime.Today.AddMonths(-1))
                .CountAsync();


            var occasions = applicationContext.Occasions.OfType<Cult>()
                .Where(o => o.Date > DateTime.Today.AddMonths(-1))
                 .Where(c => c.CultType.ToString() == model.ReportType.ToString());

            model.Convertions = await occasions.SumAsync(c => c.Convertions);

        }

        private async Task FillOutTransactionBaseReport(ReportCreateViewModel model)
        {
            var cults = applicationContext.Occasions
                             .OfType<Cult>()
                             .Where(o => o.Date > DateTime.Today.AddMonths(-1))
                             .Where(c => c.CultType.ToString() == model.ReportType.ToString());

            var trasactions = from c in cults
                              join t in applicationContext.Transactions on c.Id equals t.OccasionId
                              where t.Date > DateTime.Today.AddMonths(-1)
                              select t;


            //Fill out income
            var income = await trasactions
                .Where(t => t.TransactionType == TransactionType.RECEITA)
                .SumAsync(t => t.Value);

            model.Income = income;

            //Fill out expense
            var expense = await trasactions
                .Where(t => t.TransactionType == TransactionType.DESPESA)
                .SumAsync(t => t.Value);

            model.Expense = expense;

            //Fill out tenth

            trasactions = from c in cults
                          join t in applicationContext.Transactions.OfType<Revenue>()
                          .Where(t => t.RevenueType == RevenueType.DÍZIMO)
                          on c.Id equals t.OccasionId
                          where t.Date > DateTime.Today.AddMonths(-1)
                          select t;

            model.Tenth = await trasactions.SumAsync(t => t.Value);

            //Fill out balance
            model.Balance = income - expense;

        }

        private async Task<HashSet<SelectListItem>> GetAllMembersAsSelectList()
        {
            var asyncmembers = await applicationContext.Members.ToListAsync(); ;

            var memberList = asyncmembers
                            .OrderBy(m => m.Name)
                            .ToList();


            HashSet<SelectListItem> members = new HashSet<SelectListItem>();
            foreach (var item in memberList)
            {
                members.Add(new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Name

                });
            }

            return members;
        }
    }
}
