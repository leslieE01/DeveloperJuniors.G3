﻿namespace Developers.Models;

public class Course : EntityBase
{
    public int CourseId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Hours { get; set; }
}
