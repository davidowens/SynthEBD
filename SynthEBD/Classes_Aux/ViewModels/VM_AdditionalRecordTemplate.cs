﻿using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SynthEBD;

public class VM_AdditionalRecordTemplate : INotifyPropertyChanged
{
    public VM_AdditionalRecordTemplate(ILinkCache<ISkyrimMod, ISkyrimModGetter> recordTemplateLinkCache, ObservableCollection<VM_AdditionalRecordTemplate> parentCollection)
    {
        this.RaceFormKeys = new ObservableCollection<FormKey>();
        this.TemplateNPC = new FormKey();
        this.AdditionalRacesPaths = new ObservableCollection<VM_CollectionMemberString>();
        this.RecordTemplateLinkCache = recordTemplateLinkCache;
        this.NPCFormKeyTypes = typeof(INpcGetter).AsEnumerable();
        this.lk = PatcherEnvironmentProvider.Environment.LinkCache;
        this.RacePickerTypes = typeof(IRaceGetter).AsEnumerable();
        this.ParentCollection = parentCollection;

        AddAdditionalRacesPath = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ => { this.AdditionalRacesPaths.Add(new VM_CollectionMemberString("", this.AdditionalRacesPaths)); }
        );

        DeleteCommand = new SynthEBD.RelayCommand(
            canExecute: _ => true,
            execute: _ => { this.ParentCollection.Remove(this); }
        );
    }

    public static AdditionalRecordTemplate DumpViewModelToModel(VM_AdditionalRecordTemplate viewModel)
    {
        return new AdditionalRecordTemplate() { Races = viewModel.RaceFormKeys.ToHashSet(), TemplateNPC = viewModel.TemplateNPC, AdditionalRacesPaths = viewModel.AdditionalRacesPaths.Select(x => x.Content).ToHashSet() };
    }

    public ObservableCollection<FormKey> RaceFormKeys { get; set; }

    public ILinkCache lk { get; set; }
    public IEnumerable<Type> RacePickerTypes { get; set; }

    public FormKey TemplateNPC { get; set; }

    public ObservableCollection<VM_CollectionMemberString> AdditionalRacesPaths { get; set; }

    public IEnumerable<Type> NPCFormKeyTypes { get; set; }

    public ILinkCache<ISkyrimMod, ISkyrimModGetter> RecordTemplateLinkCache { get; set; }

    public ObservableCollection<VM_AdditionalRecordTemplate> ParentCollection { get; set; }
    public RelayCommand AddAdditionalRacesPath { get; }
    public RelayCommand DeleteCommand { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}