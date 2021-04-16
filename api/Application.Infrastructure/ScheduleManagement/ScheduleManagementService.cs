﻿using Application.Core;
using Application.Core.Data;
using Application.Infrastructure.Models;
using Application.Infrastructure.SubjectManagement;
using LMPlatform.Data.Repositories;
using LMPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.ScheduleManagement
{
	public class ScheduleManagementService : IScheduleManagementService
	{

		private readonly LazyDependency<ISubjectManagementService> _subjectManagementService = new LazyDependency<ISubjectManagementService>();

		public ISubjectManagementService SubjectManagementService => _subjectManagementService.Value;

		public bool CheckIfAllowed(DateTime date, TimeSpan startTime, TimeSpan endTime, string building, string audience)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer())
			{
				var lecturesSchedule = repositoriesContainer.RepositoryFor<LecturesScheduleVisiting>().GetAll(new Query<LecturesScheduleVisiting>(x => x.Date == date))
					.ToList()
					.Select(LectureScheduleToModel)
					.ToList();

				var practicalsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionPractical>().GetAll(new Query<ScheduleProtectionPractical>(x => x.Date == date))
					.ToList()
					.Select(PracticalScheduleToModel)
					.ToList();

				var labsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionLabs>().GetAll(new Query<ScheduleProtectionLabs>(x => x.Date == date))
					.ToList()
					.Select(LabScheduleToModel)
					.ToList();

				return !lecturesSchedule.Concat(labsSchedule).Concat(practicalsSchedule)
					.Where(x => x.Start.HasValue && x.End.HasValue)
					.Any(x =>
					((x.Start <= startTime && x.End >= endTime) ||
					(x.Start <= startTime && x.End <= endTime && startTime <= x.End) ||
					(x.Start >= startTime && endTime >= x.Start && endTime <= x.End) ||
					(x.Start >= startTime && x.Start <= endTime && startTime <= x.End && x.End <= endTime))
					&& x.Audience.Trim().ToLower() == audience.Trim().ToLower());
			}
		}

		public IEnumerable<ScheduleModel> GetScheduleBetweenDates(DateTime startDate, DateTime endDate)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer())
			{
				var lecturesSchedule = repositoriesContainer.RepositoryFor<LecturesScheduleVisiting>().GetAll(new Query<LecturesScheduleVisiting>(x => x.Date >= startDate && x.Date <= endDate)
					.Include(x => x.Subject.LecturesScheduleVisitings.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group)))
					.ToList()
					.Select(x => LectureScheduleToModel(x))
					.ToList();

				var practicalsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionPractical>().GetAll(new Query<ScheduleProtectionPractical>(x => x.Date >= startDate && x.Date <= endDate)
					.Include(x => x.Subject.ScheduleProtectionPracticals.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group)))
					.ToList()
					.Select(PracticalScheduleToModel)
					.ToList();

				var labsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionLabs>().GetAll(new Query<ScheduleProtectionLabs>(x => x.Date >= startDate && x.Date <= endDate)
					.Include(x => x.Subject.ScheduleProtectionLabs.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.SubGroups)))
					.ToList()
					.Select(x => LabScheduleToModel(x))
					.ToList();

				return lecturesSchedule.Concat(practicalsSchedule).Concat(labsSchedule).OrderBy(x => x.Date);
			}
		}

		public IEnumerable<ScheduleModel> GetScheduleForDate(DateTime date)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer())
			{
                var lecturesSchedule = repositoriesContainer.RepositoryFor<LecturesScheduleVisiting>().GetAll(new Query<LecturesScheduleVisiting>(x => x.Date == date)
                    .Include(x => x.Subject.LecturesScheduleVisitings.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group)))
                    .ToList()
                    .Select(x => LectureScheduleToModel(x))
					.ToList();

                var practicalsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionPractical>().GetAll(new Query<ScheduleProtectionPractical>(x => x.Date == date)
					.Include(x => x.Subject.ScheduleProtectionPracticals.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group)))
					.ToList()
					.Select(PracticalScheduleToModel)
					.ToList();


                var labsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionLabs>().GetAll(new Query<ScheduleProtectionLabs>(x => x.Date == date)
                    .Include(x => x.Subject.ScheduleProtectionLabs.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.SubGroups)))
                    .ToList()
                    .Select(x => LabScheduleToModel(x))
					.ToList();

                return lecturesSchedule.Concat(practicalsSchedule).Concat(labsSchedule).OrderBy(x => x.Date);
            }



		}

		public LecturesScheduleVisiting SaveDateLectures(LecturesScheduleVisiting lecturesScheduleVisiting)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer()) {
				repositoriesContainer.RepositoryFor<LecturesScheduleVisiting>().Save(lecturesScheduleVisiting);
				repositoriesContainer.ApplyChanges();
				return lecturesScheduleVisiting;

			}

		}

		public ScheduleProtectionPractical SaveDatePractical(ScheduleProtectionPractical scheduleProtectionPractical)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer())
			{
				repositoriesContainer.RepositoryFor<ScheduleProtectionPractical>().Save(scheduleProtectionPractical);
				repositoriesContainer.ApplyChanges();
				return scheduleProtectionPractical;
			}

		}

		public ScheduleProtectionLabs SaveScheduleProtectionLabsDate(ScheduleProtectionLabs scheduleProtectionLabs)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer())
			{
				repositoriesContainer.RepositoryFor<ScheduleProtectionLabs>().Save(scheduleProtectionLabs);
				repositoriesContainer.ApplyChanges();
				return scheduleProtectionLabs;
			}

		}

		private ScheduleModel LectureScheduleToModel(LecturesScheduleVisiting lecturesSchedule)
		{
			return new ScheduleModel
			{
				Audience = lecturesSchedule.Audience,
				Building = lecturesSchedule.Building,
				Color = lecturesSchedule.Subject?.Color ?? string.Empty,
				End = lecturesSchedule.EndTime,
				Start = lecturesSchedule.StartTime,
				Name = lecturesSchedule.Subject?.Name ?? string.Empty,
				ShortName = lecturesSchedule.Subject?.ShortName ?? string.Empty,
				Teacher = SubjectManagementService.GetSubjectOwner(lecturesSchedule.SubjectId),
				SubjectId = lecturesSchedule.SubjectId,
				Type = ClassType.Lecture,
				Date = lecturesSchedule.Date,
				Id = lecturesSchedule.Id,
				Notes = lecturesSchedule?.Subject?.LecturesScheduleVisitings?.FirstOrDefault(x => x.Id == lecturesSchedule.Id)?.Notes ?? Enumerable.Empty<Note>()
			};
		}

		private ScheduleModel PracticalScheduleToModel(ScheduleProtectionPractical practicalSchedule)
        {
			var group = practicalSchedule.Subject?.SubjectGroups?.FirstOrDefault(x => x.SubjectId == practicalSchedule.SubjectId)?.Group;
			return new ScheduleModel
			{
				Audience = practicalSchedule.Audience,
				Building = practicalSchedule.Building,
				Color = practicalSchedule.Subject?.Color ?? string.Empty,
				End = practicalSchedule.EndTime,
				Start = practicalSchedule.StartTime,
				Name = practicalSchedule.Subject?.Name ?? string.Empty,
				ShortName = practicalSchedule.Subject?.ShortName ?? string.Empty,
				Teacher = SubjectManagementService.GetSubjectOwner(practicalSchedule.SubjectId),
				SubjectId = practicalSchedule.SubjectId,
				Type = ClassType.Practical,
				Date = practicalSchedule.Date,
				Id = practicalSchedule.Id,
				GroupId = practicalSchedule.Group == null ? new int?() : practicalSchedule.GroupId,
				GroupName = practicalSchedule.Group == null ? string.Empty : practicalSchedule.Group.Name,
				Notes = practicalSchedule.Subject?.ScheduleProtectionPracticals?.FirstOrDefault(x => x.Id == practicalSchedule.Id)?.Notes ?? Enumerable.Empty<Note>()
			};
        }

		private ScheduleModel LabScheduleToModel(ScheduleProtectionLabs labSchedule)
		{
			var subjectGroup = labSchedule.Subject?.SubjectGroups?.FirstOrDefault(x => x.SubjectId == labSchedule.SubjectId && x.SubGroups.Any(x => x.Id == labSchedule.SuGroupId));
			var group = subjectGroup?.Group;

			return new ScheduleModel
			{
				Audience = labSchedule.Audience,
				Building = labSchedule.Building,
				Color = labSchedule.Subject?.Color ?? string.Empty,
				End = labSchedule.EndTime,
				Start = labSchedule.StartTime,
				Name = labSchedule.Subject?.Name ?? string.Empty,
				ShortName = labSchedule.Subject?.ShortName ?? string.Empty,
				Teacher = labSchedule.SubjectId.HasValue ? SubjectManagementService.GetSubjectOwner((int)labSchedule.SubjectId) : null,
				SubjectId = labSchedule.SubjectId,
				Type = ClassType.Lab,
				Date = labSchedule.Date,
				Id = labSchedule.Id,
				Notes = labSchedule?.Subject?.ScheduleProtectionLabs?.FirstOrDefault(x => x.Id == labSchedule.Id)?.Notes ?? Enumerable.Empty<Note>(),
				GroupId = group == null ? new int?() : group.Id,
				GroupName = group == null ? string.Empty : group.Name,
				SubGroupId = labSchedule.SubGroup == null ? new int?() : labSchedule.SuGroupId,
				SubGroupName = labSchedule.SubGroup == null ? string.Empty : labSchedule.SubGroup.Name
			};
		}

		public IEnumerable<ScheduleModel> GetUserSchedule(int userId, DateTime startDate, DateTime endDate)
		{
			var scedule = GetScheduleBetweenDates(startDate, endDate);
			var subjects = SubjectManagementService.GetUserSubjects(userId);
			return scedule.Where(day => subjects.Any(subject => day.SubjectId == subject.Id));
		}

		public IEnumerable<ScheduleModel> GetScheduleBetweenTimes(DateTime date, TimeSpan startTime, TimeSpan endTime)
		{
			using (var repositoriesContainer = new LmPlatformRepositoriesContainer())
			{
				var lecturesSchedule = repositoriesContainer.RepositoryFor<LecturesScheduleVisiting>().GetAll(new Query<LecturesScheduleVisiting>(x => x.Date == date && x.StartTime >= startTime && x.EndTime <= endTime)
					.Include(x => x.Subject.LecturesScheduleVisitings.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group)))
					.ToList()
					.Select(x => LectureScheduleToModel(x))
					.ToList();

				var practicalsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionPractical>().GetAll(new Query<ScheduleProtectionPractical>(x => x.Date == date && x.StartTime >= startTime && x.EndTime <= endTime)
					.Include(x => x.Subject.ScheduleProtectionPracticals.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group)))
					.ToList()
					.Select(PracticalScheduleToModel)
					.ToList();

				var labsSchedule = repositoriesContainer.RepositoryFor<ScheduleProtectionLabs>().GetAll(new Query<ScheduleProtectionLabs>(x => x.Date == date && x.StartTime >= startTime && x.EndTime <= endTime)
					.Include(x => x.Subject.ScheduleProtectionLabs.Select(x => x.Notes))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.Group))
					.Include(x => x.Subject.SubjectGroups.Select(sg => sg.SubGroups)))
					.ToList()
					.Select(LabScheduleToModel)
					.ToList();

				return lecturesSchedule.Concat(practicalsSchedule).Concat(labsSchedule).OrderBy(x => x.Date);
			}
		}
    }
}
