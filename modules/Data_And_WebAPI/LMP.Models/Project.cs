﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LMP.Models.Interface;

namespace LMP.Models
{
    public class Project : ModelBase
    {
        public string Title { get; set; }

        public string Details { get; set; }

        public DateTime DateOfChange { get; set; }

        public int CreatorId { get; set; }

        public User Creator { get; set; }

        public string Attachments { get; set; }

        public ICollection<ProjectUser> ProjectUsers { get; set; }

        public ICollection<ProjectComment> ProjectComments { get; set; }

        public ICollection<Bug> Bugs { get; set; }

        //public ICollection<ProjectMatrixRequirement> MatrixRequirements { get; set; }
    }
}