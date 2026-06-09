using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TeacherApp.Models;
using TeacherApp.Services;

namespace TeacherApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Teacher> Teachers { get; }

    [Reactive] private Teacher? _selectedTeacher;
    [Reactive] private Guid? _id;
    [Reactive] private string? _lastName;
    [Reactive] private string? _firstName;
    [Reactive] private ObservableCollection<string> _subjects = [];
    
    [Reactive] private string _newSubjectName = string.Empty;
    [Reactive] private string _subjectNameToRemove = string.Empty;

    private readonly IObservable<bool> _canSave;
    private readonly IObservable<bool> _canDelete;
    private readonly IObservable<bool> _canClear;
    
    private readonly JsonDataService _jsonDataService = new("teachers.json");

    public MainWindowViewModel()
    {
        this.WhenAnyValue(p => p.SelectedTeacher)
            .Subscribe(st =>
            {
                Id = st?.Id;
                LastName = st?.LastName;
                FirstName = st?.FirstName;
                Subjects.Clear();
                if (st?.Subjects == null) return;
                foreach (var subject in st.Subjects)
                    Subjects.Add(subject);
            });
        
        var collection = _jsonDataService.LoadData<Teacher>();
       
        Teachers = collection is not null
            ? new ObservableCollection<Teacher>(collection)
            : [];
        
        var subjectsCount = this.WhenAnyValue(x => x.Subjects)
            .Select(subjects => Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => subjects.CollectionChanged += h,
                    h => subjects.CollectionChanged -= h)
                .Select(_ => subjects.Count)
                .StartWith(subjects.Count))
            .Switch();

        _canClear = this.WhenAnyValue(
                p => p.Id,
                p => p.LastName,
                p => p.FirstName,
                (id, lastName, firstName) => new { id, lastName, firstName })
            .CombineLatest(subjectsCount, (fields, count) =>
                fields.id is not null ||
                !string.IsNullOrWhiteSpace(fields.lastName) ||
                !string.IsNullOrWhiteSpace(fields.firstName) ||
                count > 0);

        _canSave = this.WhenAnyValue(
                p => p.LastName,
                p=> p.FirstName,
                (lastName, firstName) => new { lastName, firstName })
            .CombineLatest(subjectsCount, (names, count) =>
                !string.IsNullOrWhiteSpace(names.lastName) &&
                !string.IsNullOrEmpty(names.firstName) &&
                count > 0);
        
        _canDelete = this.WhenAnyValue(p => p.SelectedTeacher)
            .Select(sp => sp is not null);
        Console.WriteLine($"Teachers count after load: {Teachers.Count}");
    }

    public void SaveTeachersToFile()
    {
        _jsonDataService.SaveData(Teachers);
    }

    [ReactiveCommand(CanExecute = nameof(_canClear))]
    private void Clear()
    {
        Id = null;
        LastName = null;
        FirstName = null;
        Subjects.Clear();
        SelectedTeacher = null;
        NewSubjectName = string.Empty;
        SubjectNameToRemove = string.Empty;
    }

    [ReactiveCommand(CanExecute = nameof(_canDelete))]
    private void Delete()
    {
        Teachers.Remove(SelectedTeacher!);
        Clear();
    }

    [ReactiveCommand(CanExecute = nameof(_canSave))]
    private void Save()
    {
        if (SelectedTeacher is not null)
        {
            var updatedTeacher = SelectedTeacher with
            {
                LastName = LastName!,
                FirstName = FirstName!,
                Subjects = new ObservableCollection<string>(Subjects)
            };
            var index = Teachers.IndexOf(SelectedTeacher);
            Teachers[index] = updatedTeacher;
            SelectedTeacher = updatedTeacher;
        }
        else
        {
            var newTeacher = new Teacher
            {
                LastName = LastName!,
                FirstName = FirstName!,
                Subjects = new ObservableCollection<string>(Subjects)
            };
            Teachers.Add(newTeacher);
            SelectedTeacher = newTeacher;
        }
    }
    
    [ReactiveCommand]
    private void AddSubject()
    {
        var trimmedName = NewSubjectName.Trim(); 
        if (string.IsNullOrWhiteSpace(trimmedName)
            || Subjects.Contains(trimmedName))
            return;
        
        Subjects.Add(trimmedName);
        NewSubjectName = string.Empty;
    }
    
    [ReactiveCommand]
    private void RemoveSubject()
    {
        var trimmedName = SubjectNameToRemove.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return;

        if (Subjects.Remove(trimmedName)) 
            SubjectNameToRemove = string.Empty;
    }
}