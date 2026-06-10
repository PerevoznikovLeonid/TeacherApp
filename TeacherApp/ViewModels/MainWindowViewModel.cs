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
    private readonly IObservable<bool> _canClear;
    private readonly IObservable<bool> _canDelete;
    private readonly IObservable<bool> _canSave;
    private readonly IObservable<bool> _canClearSubject;
    private readonly IObservable<bool> _canSaveSubject;
    private readonly IObservable<bool> _canDeleteSubject;

    private readonly JsonDataService _jsonDataService = new("teachers.json");
    [Reactive] private string? _firstName;
    [Reactive] private Guid? _id;
    [Reactive] private string? _lastName;

    [Reactive] private Teacher? _selectedTeacher;
    [Reactive] private ObservableCollection<string> _subjects = [];
    
    [Reactive] private string? _selectedSubject;
    [Reactive] private string _editSubjectName = string.Empty;

    public MainWindowViewModel()
    {
        this.WhenAnyValue(p => p.SelectedTeacher)
            .Subscribe(st =>
            {
                Id = st?.Id;
                LastName = st?.LastName;
                FirstName = st?.FirstName;
                Subjects.Clear();
                if (st?.Subjects == null)
                {
                    return;
                }

                foreach (string subject in st.Subjects)
                {
                    Subjects.Add(subject);
                }
            });
        
        this.WhenAnyValue(p => p.SelectedSubject)
            .Subscribe(subj => EditSubjectName = subj ?? string.Empty);

        ObservableCollection<Teacher>? collection = _jsonDataService.LoadData<Teacher>();

        Teachers = collection is not null
            ? new ObservableCollection<Teacher>(collection)
            : [];

        IObservable<int> subjectsCount = this.WhenAnyValue(x => x.Subjects)
            .Select(subjects => Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => subjects.CollectionChanged += h,
                    h => subjects.CollectionChanged -= h)
                .Select(_ => subjects.Count)
                .StartWith(subjects.Count))
            .Switch();

        _canClear = this.WhenAnyValue(
                t => t.Id,
                t => t.LastName,
                t => t.FirstName,
                (id, lastName, firstName) => new { id, lastName, firstName })
            .CombineLatest(subjectsCount, (fields, count) =>
                fields.id is not null ||
                !string.IsNullOrWhiteSpace(fields.lastName) ||
                !string.IsNullOrWhiteSpace(fields.firstName) ||
                count > 0);

        _canSave = this.WhenAnyValue(
                t => t.LastName,
                t => t.FirstName,
                (lastName, firstName) => new { lastName, firstName })
            .CombineLatest(subjectsCount, (names, count) =>
                !string.IsNullOrWhiteSpace(names.lastName) &&
                !string.IsNullOrWhiteSpace(names.firstName) &&
                count > 0);

        _canDelete = this.WhenAnyValue(t => t.SelectedTeacher)
            .Select(st => st is not null);

        _canClearSubject = this.WhenAnyValue(s => s.EditSubjectName)
            .Select(esn => !string.IsNullOrWhiteSpace(esn));
        
        _canSaveSubject = this.WhenAnyValue(
                s => s.EditSubjectName,
                 s => s.Subjects,
                (editSubjectName, subjects) => !string.IsNullOrWhiteSpace(editSubjectName) &&
                                               !subjects.Contains(editSubjectName));
        
        _canDeleteSubject = this.WhenAnyValue(s => s.SelectedSubject)
            .Select(subj => subj is not null);
    }

    public ObservableCollection<Teacher> Teachers { get; }

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
            Teacher updatedTeacher = SelectedTeacher with
            {
                LastName = LastName!, FirstName = FirstName!, Subjects = new ObservableCollection<string>(Subjects)
            };
            int index = Teachers.IndexOf(SelectedTeacher);
            Teachers[index] = updatedTeacher;
            SelectedTeacher = updatedTeacher;
        }
        else
        {
            Teacher newTeacher = new()
            {
                LastName = LastName!, FirstName = FirstName!, Subjects = new ObservableCollection<string>(Subjects)
            };
            Teachers.Add(newTeacher);
            SelectedTeacher = newTeacher;
        }
    }

    [ReactiveCommand(CanExecute = nameof(_canClearSubject))]
    private void ClearSubject()
    {
        SelectedSubject = null;
        EditSubjectName = string.Empty;
    }

    [ReactiveCommand(CanExecute = nameof(_canSaveSubject))]
    private void SaveSubject()
    {
        var newName = EditSubjectName.Trim();

        if (SelectedSubject is not null)
        {
            var index = Subjects.IndexOf(SelectedSubject);
            if (index < 0)
            {
                return;
            }

            Subjects[index] = newName;
        }
        else
        {
            Subjects.Add(newName);
        }

        SelectedSubject = newName;
    }

    [ReactiveCommand(CanExecute = nameof(_canDeleteSubject))]
    private void DeleteSubject()
    {
        Subjects.Remove(SelectedSubject!);
        ClearSubject();
    }
}