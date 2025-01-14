﻿using System.Collections.Generic;
using Application.Core.Data;
using LMPlatform.Models;
using System.Linq.Expressions;
using System;

namespace Application.Infrastructure.LecturerManagement
{
    public interface ILecturerManagementService
    {
        Lecturer GetLecturer(int userId);

        Lecturer GetLecturerBase(int userId);

        List<Lecturer> GetLecturers(Expression<Func<Lecturer, string>> order = null, bool lite = false);

        IPageableList<Lecturer> GetLecturersPageable(string searchString = null, IPageInfo pageInfo = null, IEnumerable<ISortCriteria> sortCriterias = null);

        Lecturer Save(Lecturer lecturer);

        Lecturer UpdateLecturer(Lecturer lecturer);

	    List<string> GetLecturesScheduleVisitings(int subjectId);

	    List<List<string>> GetLecturesScheduleMarks(int subjectId, int groupId);

        bool DeleteLecturer(int id);

		bool Join(int subjectId, int lectorId, int owner);

	    List<Lecturer> GetJoinedLector(int subjectId);

	    void DisjoinLector(int subjectId, int lectorId, int owner);

	    void DisjoinOwnerLector(int subjectId, int lectorId);

        bool IsLectorJoined(int subjectId, int lectorId);
        List<Lecturer> GetNoAdjointLectorers(int subjectId);
    }
}
