﻿using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using ReactiveUI;

namespace SynthEBD;

public class VM_BodySlideSetting : INotifyPropertyChanged
{
    public VM_BodySlideSetting(VM_BodyShapeDescriptorCreationMenu BodyShapeDescriptors, ObservableCollection<VM_RaceGrouping> raceGroupingVMs, ObservableCollection<VM_BodySlideSetting> parentCollection, VM_SettingsOBody parentConfig)
    {
        this.Label = "";
        this.Notes = "";
        this.DescriptorsSelectionMenu = new VM_BodyShapeDescriptorSelectionMenu(BodyShapeDescriptors, raceGroupingVMs, parentConfig);
        this.AllowedRaces = new ObservableCollection<FormKey>();
        this.AllowedRaceGroupings = new VM_RaceGroupingCheckboxList(raceGroupingVMs);
        this.DisallowedRaces = new ObservableCollection<FormKey>();
        this.DisallowedRaceGroupings = new VM_RaceGroupingCheckboxList(raceGroupingVMs);
        this.AllowedAttributes = new ObservableCollection<VM_NPCAttribute>();
        this.DisallowedAttributes = new ObservableCollection<VM_NPCAttribute>();
        this.bAllowUnique = true;
        this.bAllowNonUnique = true;
        this.bAllowRandom = true;
        this.ProbabilityWeighting = 1;
        this.WeightRange = new NPCWeightRange();

        this.Caption_BodyShapeDescriptors = "";

        this.lk = PatcherEnvironmentProvider.Environment.LinkCache;
        this.RacePickerFormKeys = typeof(IRaceGetter).AsEnumerable();

        this.ParentConfig = parentConfig;
        this.ParentCollection = parentCollection;

        this.IsHidden = false;
        this.IsVisible = true;

        AddAllowedAttribute = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ => this.AllowedAttributes.Add(VM_NPCAttribute.CreateNewFromUI(this.AllowedAttributes, true, null, ParentConfig.AttributeGroupMenu.Groups))
        );

        AddDisallowedAttribute = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ => this.DisallowedAttributes.Add(VM_NPCAttribute.CreateNewFromUI(this.DisallowedAttributes, false, null, ParentConfig.AttributeGroupMenu.Groups))
        );

        DeleteMe = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ => this.ParentCollection.Remove(this)
        );

        HideButtonText = "Hide";

        ToggleHide = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ =>
            {
                if (IsHidden)
                {
                    IsHidden = false;
                    HideButtonText = "Hide";
                }
                else
                {
                    IsHidden = true;
                    HideButtonText = "Unhide";
                }
            }
        );

        this.WhenAnyValue(x => x.IsHidden).Subscribe(x =>
        {
            if (!parentConfig.BodySlidesUI.ShowHidden && IsHidden)
            {
                IsVisible = false;
            }
            else
            {
                IsVisible = true;
            }
            UpdateStatusDisplay();
        });

        Clone = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ => { 
                var cloneModel = VM_BodySlideSetting.DumpViewModelToModel(this);
                var cloneViewModel = new VM_BodySlideSetting(BodyShapeDescriptors, raceGroupingVMs, ParentCollection, ParentConfig);
                VM_BodySlideSetting.GetViewModelFromModel(cloneModel, cloneViewModel, BodyShapeDescriptors, raceGroupingVMs, ParentConfig);
                var index = parentCollection.IndexOf(this);
                parentCollection.Insert(index, cloneViewModel);
            }
        );

        DescriptorsSelectionMenu.WhenAnyValue(x => x.Header).Subscribe(x => UpdateStatusDisplay());
    }

    public string Label { get; set; }
    public string Notes { get; set; }
    public VM_BodyShapeDescriptorSelectionMenu DescriptorsSelectionMenu { get; set; }
    public ObservableCollection<FormKey> AllowedRaces { get; set; }
    public ObservableCollection<FormKey> DisallowedRaces { get; set; }
    public VM_RaceGroupingCheckboxList AllowedRaceGroupings { get; set; }
    public VM_RaceGroupingCheckboxList DisallowedRaceGroupings { get; set; }
    public ObservableCollection<VM_NPCAttribute> AllowedAttributes { get; set; } // keeping as array to allow deserialization of original zEBD settings files
    public ObservableCollection<VM_NPCAttribute> DisallowedAttributes { get; set; }
    public bool bAllowUnique { get; set; }
    public bool bAllowNonUnique { get; set; }
    public bool bAllowRandom { get; set; }
    public double ProbabilityWeighting { get; set; }
    public NPCWeightRange WeightRange { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    public string Caption_BodyShapeDescriptors { get; set; }
    public ILinkCache lk { get; set; }
    public IEnumerable<Type> RacePickerFormKeys { get; set; }

    public RelayCommand AddAllowedAttribute { get; }
    public RelayCommand AddDisallowedAttribute { get; }
    public RelayCommand DeleteMe { get; }
    public RelayCommand Clone { get; }
    public RelayCommand ToggleHide { get; }
    public VM_SettingsOBody ParentConfig { get; set; }
    public ObservableCollection<VM_BodySlideSetting> ParentCollection { get; set; }

    public SolidColorBrush BorderColor { get; set; }
    public bool IsVisible {  get; set; }
    public bool IsHidden { get; set; } // not the same as IsVisible. IsVisible can be set true if the "show hidden" button is checked.
    public string HideButtonText { get; set; }
    public string StatusHeader { get; set; }
    public string StatusText { get; set; }
    public bool ShowStatus { get; set; }

    public void UpdateStatusDisplay()
    {
        if (!ParentConfig.BodySlidesUI.CurrentlyExistingBodySlides.Contains(this.Label))
        {
            BorderColor = new SolidColorBrush(Colors.Red);
            StatusHeader = "Warning:";
            StatusText = "Source BodySlide XML files are missing. Will not be assigned.";
            ShowStatus = true;
        }
        else if (IsHidden)
        {
            BorderColor = new SolidColorBrush(Colors.LightSlateGray);
        }
        else if(!DescriptorsSelectionMenu.IsAnnotated())
        {
            BorderColor = new SolidColorBrush(Colors.Yellow);
            StatusHeader = "Warning:";
            StatusText = "Bodyslide has not been annotated with descriptors. May not pair correctly with textures.";
            ShowStatus = true;
        }
        else
        {
            BorderColor = new SolidColorBrush(Colors.LightGreen);
            StatusHeader = string.Empty;
            StatusText = string.Empty;
            ShowStatus = false;
        }
    }

    public static void GetViewModelFromModel(BodySlideSetting model, VM_BodySlideSetting viewModel, VM_BodyShapeDescriptorCreationMenu descriptorMenu, ObservableCollection<VM_RaceGrouping> raceGroupingVMs, IHasAttributeGroupMenu parentConfig)
    {
        viewModel.Label = model.Label;
        viewModel.Notes = model.Notes;
        viewModel.DescriptorsSelectionMenu = VM_BodyShapeDescriptorSelectionMenu.InitializeFromHashSet(model.BodyShapeDescriptors, descriptorMenu, raceGroupingVMs, parentConfig);
        viewModel.AllowedRaces = new ObservableCollection<FormKey>(model.AllowedRaces);
        viewModel.AllowedRaceGroupings = new VM_RaceGroupingCheckboxList(raceGroupingVMs);
        foreach (var grouping in viewModel.AllowedRaceGroupings.RaceGroupingSelections)
        {
            if (model.AllowedRaceGroupings.Contains(grouping.Label))
            {
                grouping.IsSelected = true;
            }
            else { grouping.IsSelected = false; }
        }

        viewModel.DisallowedRaces = new ObservableCollection<FormKey>(model.DisallowedRaces);
        viewModel.DisallowedRaceGroupings = new VM_RaceGroupingCheckboxList(raceGroupingVMs);

        foreach (var grouping in viewModel.DisallowedRaceGroupings.RaceGroupingSelections)
        {
            if (model.DisallowedRaceGroupings.Contains(grouping.Label))
            {
                grouping.IsSelected = true;
            }
            else { grouping.IsSelected = false; }
        }

        viewModel.AllowedAttributes = VM_NPCAttribute.GetViewModelsFromModels(model.AllowedAttributes, viewModel.ParentConfig.AttributeGroupMenu.Groups, true, null);
        viewModel.DisallowedAttributes = VM_NPCAttribute.GetViewModelsFromModels(model.DisallowedAttributes, viewModel.ParentConfig.AttributeGroupMenu.Groups, false, null);
        foreach (var x in viewModel.DisallowedAttributes) { x.DisplayForceIfOption = false; }
        viewModel.bAllowUnique = model.AllowUnique;
        viewModel.bAllowNonUnique = model.AllowNonUnique;
        viewModel.bAllowRandom = model.AllowRandom;
        viewModel.ProbabilityWeighting = model.ProbabilityWeighting;
        viewModel.WeightRange = model.WeightRange;

        viewModel.UpdateStatusDisplay();

        viewModel.DescriptorsSelectionMenu.WhenAnyValue(x => x.Header).Subscribe(x => viewModel.UpdateStatusDisplay());

        viewModel.IsHidden = model.HideInMenu;
    }

    public static BodySlideSetting DumpViewModelToModel(VM_BodySlideSetting viewModel)
    {
        BodySlideSetting model = new BodySlideSetting();
        model.Label = viewModel.Label;
        model.Notes = viewModel.Notes;
        model.BodyShapeDescriptors = VM_BodyShapeDescriptorSelectionMenu.DumpToHashSet(viewModel.DescriptorsSelectionMenu);
        model.AllowedRaces = viewModel.AllowedRaces.ToHashSet();
        model.AllowedRaceGroupings = viewModel.AllowedRaceGroupings.RaceGroupingSelections.Where(x => x.IsSelected).Select(x => x.Label).ToHashSet();
        model.DisallowedRaces = viewModel.DisallowedRaces.ToHashSet();
        model.DisallowedRaceGroupings = viewModel.DisallowedRaceGroupings.RaceGroupingSelections.Where(x => x.IsSelected).Select(x => x.Label).ToHashSet();
        model.AllowedAttributes = VM_NPCAttribute.DumpViewModelsToModels(viewModel.AllowedAttributes);
        model.DisallowedAttributes = VM_NPCAttribute.DumpViewModelsToModels(viewModel.DisallowedAttributes);
        model.AllowUnique = viewModel.bAllowUnique;
        model.AllowNonUnique = viewModel.bAllowNonUnique;
        model.AllowRandom = viewModel.bAllowRandom;
        model.ProbabilityWeighting = viewModel.ProbabilityWeighting;
        model.WeightRange = viewModel.WeightRange;
        model.HideInMenu = viewModel.IsHidden;
        return model;
    }
}