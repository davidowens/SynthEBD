﻿using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthEBD
{
    public class VM_HeadPartCategoryRules : VM
    {
        public VM_HeadPartCategoryRules(ObservableCollection<VM_RaceGrouping> raceGroupingVMs, VM_Settings_Headparts parentConfig, VM_SettingsOBody oBody)
        {
            AllowedBodySlideDescriptors = new VM_BodyShapeDescriptorSelectionMenu(oBody.DescriptorUI, raceGroupingVMs, parentConfig);
            DisallowedBodySlideDescriptors = new VM_BodyShapeDescriptorSelectionMenu(oBody.DescriptorUI, raceGroupingVMs, parentConfig);
            AllowedRaceGroupings = new VM_RaceGroupingCheckboxList(raceGroupingVMs);
            DisallowedRaceGroupings = new VM_RaceGroupingCheckboxList(raceGroupingVMs);

            ParentConfig = parentConfig;

            PatcherEnvironmentProvider.Instance.WhenAnyValue(x => x.Environment.LinkCache)
                .Subscribe(x => lk = x)
                .DisposeWith(this);

            AddAllowedAttribute = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.AllowedAttributes.Add(VM_NPCAttribute.CreateNewFromUI(this.AllowedAttributes, true, null, ParentConfig.AttributeGroupMenu.Groups))
            );

            AddDisallowedAttribute = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.DisallowedAttributes.Add(VM_NPCAttribute.CreateNewFromUI(this.DisallowedAttributes, false, null, ParentConfig.AttributeGroupMenu.Groups))
            );

            AddDistributionWeighting = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ =>
                {
                    if (DistributionProbabilities.Any())
                    {
                        var existingWeights = DistributionProbabilities.Select(x => x.DistributionQuantity);
                        var max = existingWeights.Max();
                        var range = new HashSet<int>(Enumerable.Range(0, max));
                        range.ExceptWith(existingWeights); // now only missing values remain
                        var toAdd = max + 1;
                        if (range.Any()) { toAdd = range.Min(); }
                        DistributionProbabilities.Add(new VM_HeadPartQuantityDistributionWeighting(toAdd, 1, this));
                    }
                    else
                    {
                        DistributionProbabilities.Add(new VM_HeadPartQuantityDistributionWeighting(0, 1, this));
                    }

                    DistributionProbabilities = new(DistributionProbabilities.OrderBy(x => x.DistributionQuantity));
                }
            );
        }
        public bool bAllowFemale { get; set; } = true;
        public bool bAllowMale { get; set; } = true;
        public bool bRestrictToNPCsWithThisType { get; set; } = true;
        public ObservableCollection<FormKey> AllowedRaces { get; set; } = new();
        public ObservableCollection<FormKey> DisallowedRaces { get; set; } = new();
        public VM_RaceGroupingCheckboxList AllowedRaceGroupings { get; set; }
        public VM_RaceGroupingCheckboxList DisallowedRaceGroupings { get; set; }
        public ObservableCollection<VM_NPCAttribute> AllowedAttributes { get; set; } = new(); // keeping as array to allow deserialization of original zEBD settings files
        public ObservableCollection<VM_NPCAttribute> DisallowedAttributes { get; set; } = new();
        public bool bAllowUnique { get; set; } = true;
        public bool bAllowNonUnique { get; set; } = true;
        public bool bAllowRandom { get; set; } = true;
        public NPCWeightRange WeightRange { get; set; } = new();
        public ObservableCollection<VM_HeadPartQuantityDistributionWeighting> DistributionProbabilities { get; set; } = new();
        public RelayCommand AddDistributionWeighting { get; }
        public string Caption_BodyShapeDescriptors { get; set; } = "";
        public ILinkCache lk { get; private set; }
        public IEnumerable<Type> RacePickerFormKeys { get; set; } = typeof(IRaceGetter).AsEnumerable();
        public RelayCommand AddAllowedAttribute { get; }
        public RelayCommand AddDisallowedAttribute { get; }
        public VM_Settings_Headparts ParentConfig { get; set; }
        public VM_BodyShapeDescriptorSelectionMenu AllowedBodySlideDescriptors { get; set; }
        public VM_BodyShapeDescriptorSelectionMenu DisallowedBodySlideDescriptors { get; set; }

        public static VM_HeadPartCategoryRules GetViewModelFromModel(Settings_HeadPartType model, ObservableCollection<VM_RaceGrouping> raceGroupingVMs, VM_AttributeGroupMenu attributeGroupMenu, VM_Settings_Headparts parentConfig, VM_SettingsOBody oBody)
        {
            VM_HeadPartCategoryRules viewModel = new VM_HeadPartCategoryRules(raceGroupingVMs, parentConfig, oBody);
            viewModel.bAllowFemale = model.bAllowFemale;
            viewModel.bAllowMale = model.bAllowMale;
            viewModel.bRestrictToNPCsWithThisType = model.bRestrictToNPCsWithThisType;
            viewModel.AllowedRaces = new ObservableCollection<FormKey>(model.AllowedRaces);
            viewModel.AllowedRaceGroupings = VM_RaceGroupingCheckboxList.GetRaceGroupingsByLabel(model.AllowedRaceGroupings, raceGroupingVMs);
            viewModel.DisallowedRaces = new ObservableCollection<FormKey>(model.DisallowedRaces);
            viewModel.DisallowedRaceGroupings = VM_RaceGroupingCheckboxList.GetRaceGroupingsByLabel(model.DisallowedRaceGroupings, raceGroupingVMs);
            viewModel.AllowedAttributes = VM_NPCAttribute.GetViewModelsFromModels(model.AllowedAttributes, attributeGroupMenu.Groups, true, null);
            viewModel.DisallowedAttributes = VM_NPCAttribute.GetViewModelsFromModels(model.DisallowedAttributes, attributeGroupMenu.Groups, false, null);
            foreach (var x in viewModel.DisallowedAttributes) { x.DisplayForceIfOption = false; }
            viewModel.bAllowUnique = model.bAllowUnique;
            viewModel.bAllowNonUnique = model.bAllowNonUnique;
            viewModel.bAllowRandom = model.bAllowRandom;
            foreach (var distWeighting in model.DistributionProbabilities)
            {
                viewModel.DistributionProbabilities.Add(new VM_HeadPartQuantityDistributionWeighting(distWeighting.Quantity, distWeighting.ProbabilityWeighting, viewModel));
            }
            viewModel.WeightRange = model.WeightRange;
            viewModel.AllowedBodySlideDescriptors = VM_BodyShapeDescriptorSelectionMenu.InitializeFromHashSet(model.AllowedBodySlideDescriptors, oBody.DescriptorUI, raceGroupingVMs, parentConfig);
            viewModel.DisallowedBodySlideDescriptors = VM_BodyShapeDescriptorSelectionMenu.InitializeFromHashSet(model.DisallowedBodySlideDescriptors, oBody.DescriptorUI, raceGroupingVMs, parentConfig);
            return viewModel;
        }

        public void DumpToModel(Settings_HeadPartType model)
        {
            model.bAllowFemale = bAllowFemale;
            model.bAllowMale = bAllowMale;
            model.bRestrictToNPCsWithThisType = bRestrictToNPCsWithThisType;
            model.AllowedRaces = AllowedRaces.ToHashSet();
            model.AllowedRaceGroupings = AllowedRaceGroupings.RaceGroupingSelections.Where(x => x.IsSelected).Select(x => x.SubscribedMasterRaceGrouping.Label).ToHashSet();
            model.DisallowedRaces = DisallowedRaces.ToHashSet();
            model.DisallowedRaceGroupings = DisallowedRaceGroupings.RaceGroupingSelections.Where(x => x.IsSelected).Select(x => x.SubscribedMasterRaceGrouping.Label).ToHashSet();
            model.AllowedAttributes = VM_NPCAttribute.DumpViewModelsToModels(AllowedAttributes);
            model.DisallowedAttributes = VM_NPCAttribute.DumpViewModelsToModels(DisallowedAttributes);
            model.bAllowUnique = bAllowUnique;
            model.bAllowNonUnique = bAllowNonUnique;
            model.bAllowRandom = bAllowRandom;
            model.DistributionProbabilities = DistributionProbabilities.Select(x => x.DumpToModel()).ToList();
            model.WeightRange = WeightRange;
            model.AllowedBodySlideDescriptors = VM_BodyShapeDescriptorSelectionMenu.DumpToHashSet(AllowedBodySlideDescriptors);
            model.DisallowedBodySlideDescriptors = VM_BodyShapeDescriptorSelectionMenu.DumpToHashSet(DisallowedBodySlideDescriptors);
        }
    }

    public class VM_HeadPartQuantityDistributionWeighting : VM
    {
        public VM_HeadPartQuantityDistributionWeighting(int quantity, double weight, VM_HeadPartCategoryRules parentMenu)
        {
            ParentMenu = parentMenu;
            DistributionQuantity = quantity;
            DistributionWeight = weight;

            DeleteMe = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => ParentMenu.DistributionProbabilities.Remove(this)
            ) ;
        }
        public int DistributionQuantity { get; set; }
        public double DistributionWeight { get; set; }
        public RelayCommand DeleteMe { get; }
        VM_HeadPartCategoryRules ParentMenu { get; set; }   

        public void CopyInFromViewModel(HeadPartQuantityDistributionWeighting model)
        {
            DistributionQuantity = model.Quantity;
            DistributionWeight = model.ProbabilityWeighting;
        }

        public HeadPartQuantityDistributionWeighting DumpToModel()
        {
            return new HeadPartQuantityDistributionWeighting() { Quantity = DistributionQuantity, ProbabilityWeighting = DistributionWeight };
        }
    }
}
