﻿using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.ObjectModel;
using Noggog.WPF;

namespace SynthEBD;

public class VM_RaceGrouping : ViewModel
{
    public VM_RaceGrouping(RaceGrouping raceGrouping, IGameEnvironmentState<ISkyrimMod, ISkyrimModGetter> env, VM_Settings_General parentVM)
    {
        this.Label = raceGrouping.Label;
        this.Races = new ObservableCollection<FormKey>(raceGrouping.Races);
        this.lk = env.LinkCache;

        DeleteCommand = new RelayCommand(canExecute: _ => true, execute: _ => parentVM.RaceGroupings.Remove(this));
    }
    public string Label { get; set; }
    public ObservableCollection<FormKey> Races { get; set; }
    public IEnumerable<Type> RacePickerFormKeys { get; set; } = typeof(IRaceGetter).AsEnumerable();
    public ILinkCache lk { get; set; }
    public VM_Settings_General ParentVM { get; set; }
    public RelayCommand DeleteCommand { get; }

    public static ObservableCollection<VM_RaceGrouping> GetViewModelsFromModels(List<RaceGrouping> models, IGameEnvironmentState<ISkyrimMod, ISkyrimModGetter> env, VM_Settings_General parentVM)
    {
        var RGVM = new ObservableCollection<VM_RaceGrouping>();

        foreach (var x in models)
        {
            var y = new VM_RaceGrouping(x, env, parentVM);
            RGVM.Add(y);
        }

        return RGVM;
    }
    public static RaceGrouping DumpViewModelToModel(VM_RaceGrouping viewModel)
    {
        RaceGrouping model = new RaceGrouping();
        model.Label = viewModel.Label;
        model.Races = viewModel.Races.ToHashSet();

        return model;
    }
}