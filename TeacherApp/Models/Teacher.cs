using System;
using System.Collections.ObjectModel;

namespace TeacherApp.Models;

public record Teacher
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string ShortId => $"{Id.ToString("D")[..12]}";
    public required string LastName { get; init; }
    public required string FirstName {get ;init;}
    public string FullName => $"{LastName} {FirstName}";
    public ObservableCollection<string> Subjects { get; init; } = [];
}