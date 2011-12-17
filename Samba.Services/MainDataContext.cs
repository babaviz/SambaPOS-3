﻿using System.Collections.Generic;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Menus;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class MainDataContext
    {
        public int AccountCount { get; set; }
        public int LocationCount { get; set; }
        public string NumeratorValue { get; set; }

        public IDepartmentService DepartmentService { get; set; }
        public IInventoryService InventoryService { get; set; }
        public IWorkPeriodService WorkPeriodService { get; set; }
        public IUserService UserService { get; set; }
        public ILocationService LocationService { get; set; }
        
        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions)); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>()); } }

        private IEnumerable<TaxTemplate> _taxTemplates;
        public IEnumerable<TaxTemplate> TaxTemplates
        {
            get { return _taxTemplates ?? (_taxTemplates = Dao.Query<TaxTemplate>()); }
        }

        private IEnumerable<ServiceTemplate> _serviceTemplates;
        public IEnumerable<ServiceTemplate> ServiceTemplates
        {
            get { return _serviceTemplates ?? (_serviceTemplates = Dao.Query<ServiceTemplate>()); }
        }

        public MainDataContext()
        {
            DepartmentService = ServiceLocator.Current.GetInstance(typeof(IDepartmentService)) as IDepartmentService;
            InventoryService = ServiceLocator.Current.GetInstance(typeof(IInventoryService)) as IInventoryService;
            WorkPeriodService = ServiceLocator.Current.GetInstance(typeof(IWorkPeriodService)) as IWorkPeriodService;
            UserService = ServiceLocator.Current.GetInstance(typeof(IUserService)) as IUserService;
            LocationService = ServiceLocator.Current.GetInstance(typeof(ILocationService)) as ILocationService;
        }

        public void ResetUserData()
        {
            DepartmentService.Reset();
            ThreadPool.QueueUserWorkItem(ResetLocationAndAccountCounts);
        }

        private void ResetLocationAndAccountCounts(object state)
        {
            AccountCount = Dao.Count<Account>(null);
            LocationCount = Dao.Count<Location>(null);
        }

        public void ResetCache()
        {
            var selectedDepartment = DepartmentService.CurrentDepartment != null ? DepartmentService.CurrentDepartment.Id : 0;
            var selectedLocationScreen = LocationService.SelectedLocationScreen != null ? LocationService.SelectedLocationScreen.Id : 0;

            LocationService.SelectedLocationScreen = null;
            DepartmentService.SelectDepartment(null);
            DepartmentService.Reset();
            InventoryService.Reset();
            WorkPeriodService.Reset();
            UserService.Reset();
            _rules = null;
            _actions = null;
            _taxTemplates = null;
            _serviceTemplates = null;

            DepartmentService.SelectDepartment(selectedDepartment);

            //if (selectedDepartment > 0 && Departments.Count(x => x.Id == selectedDepartment) > 0)
            //{
            //    SelectedDepartment = Departments.Single(x => x.Id == selectedDepartment);
            //    if (selectedLocationScreen > 0 && SelectedDepartment.PosLocationScreens.Count(x => x.Id == selectedLocationScreen) > 0)
            //        SelectedLocationScreen = SelectedDepartment.PosLocationScreens.Single(x => x.Id == selectedLocationScreen);
            //}
        }

        public TaxTemplate GetTaxTemplate(int menuItemId)
        {
            return AppServices.DataAccessService.GetMenuItem(menuItemId).TaxTemplate;
        }
    }
}
