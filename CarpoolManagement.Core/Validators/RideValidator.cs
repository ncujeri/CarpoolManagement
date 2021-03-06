﻿using CarpoolManagement.Core.Services;
using CarpoolManagement.Core.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace CarpoolManagement.Core.Validators
{
    public class RideValidator : AbstractValidator<RideViewModel>
    {
        private readonly IRideService rideService;
        public RideValidator(IRideService rideService)
        {
            this.rideService = rideService;
            RuleFor(x => x.StartLocation).NotEmpty().WithMessage("Start location is required.").NotEqual(x => x.EndLocation).WithMessage("Start location can not be the same as end location.");
            RuleFor(x => x.EndLocation).NotEmpty().WithMessage("End location is required");
            RuleFor(x => x.StartDate).Must(x => !x.Equals(default(DateTime))).WithMessage(x => "Start date is required.").NotEqual(x => x.EndDate).WithMessage("Start date can not be the same as end date.");
            RuleFor(x => x.EndDate).Must(x => !x.Equals(default(DateTime))).WithMessage("End date is required.");
            RuleFor(x => x.CarId).NotNull().WithMessage("Please select the car.").Must((x,y)=> CarDateCheck(x.Id, y?.Value??"0",x.StartDate,x.EndDate)).WithMessage("This car is already used on selected period.")
                .MustAsync((x,y,z)=> CarSeatsCheck(y?.Value??"0",x.EmployeeIds?.Length ?? 0)).WithMessage("Not enough space in this car.");
            RuleFor(x => x.EmployeeIds).NotEmpty().WithMessage("Please select at least one employee")
                .Must(x => EmployeeLicenceCheck(x ?? new SelectListItem[] { })).WithMessage("At least one employee must have a driving licence.")
                .Must((x,y)=> EmployeeDateCheck(x.Id,y ?? new SelectListItem[] { }, x.StartDate, x.EndDate)).WithMessage("One of selected employees is unavailable on selected period.");
        }

        private bool CarDateCheck(long rideId, string id, DateTime startDate, DateTime endDate) =>
            rideService.GetRidesByCarIdAndDates(rideId, id, startDate, endDate).Count == 0;

        private async Task<bool> CarSeatsCheck(string id, int peopleCount) =>
            await rideService.CheckCarSeats(id, peopleCount);

        private bool EmployeeLicenceCheck(SelectListItem[] ids) =>
            rideService.CheckEmployeesDrivingLicence(ids);

        private bool EmployeeDateCheck(long rideId, SelectListItem[] ids, DateTime startDate, DateTime endDate) =>
            rideService.GetRidesByEmployeeIdsAndDates(rideId, ids, startDate, endDate).Count == 0;
    }
}
