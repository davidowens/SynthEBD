﻿using Mutagen.Bethesda.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SynthEBD
{
    public class VM_BodyGenConfig : INotifyPropertyChanged
    {
        public VM_BodyGenConfig(ObservableCollection<VM_RaceGrouping> raceGroupingVMs)
        {
            this.Label = "";
            this.Gender = Gender.female;

            this.GroupMappingUI = new VM_BodyGenGroupMappingMenu(this.GroupUI, raceGroupingVMs);
            this.GroupUI = new VM_BodyGenGroupsMenu(this);
            this.DescriptorUI = new VM_BodyGenMorphDescriptorMenu();
            this.TemplateMorphUI = new VM_BodyGenTemplateMenu(this, raceGroupingVMs);
            this.DisplayedUI = this.TemplateMorphUI;
            this.AttributeGroupMenu = new VM_AttributeGroupMenu();

            ClickTemplateMenu = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.DisplayedUI = this.TemplateMorphUI
                );

            ClickGroupMappingMenu = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.DisplayedUI = this.GroupMappingUI
                );
            ClickDescriptorMenu = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.DisplayedUI = this.DescriptorUI
                );
            ClickGroupsMenu = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.DisplayedUI = this.GroupUI
                );
            ClickAttributeGroupsMenu = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => this.DisplayedUI = this.AttributeGroupMenu
                );

            ImportAttributeGroups = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ =>
                {
                    var alreadyContainedGroups = AttributeGroupMenu.Groups.Select(x => x.Label).ToHashSet();
                    foreach (var attGroup in VM_Settings_General.AttributeGroupMenu.Groups)
                    {
                        if (!alreadyContainedGroups.Contains(attGroup.Label))
                        {
                            AttributeGroupMenu.Groups.Add(VM_AttributeGroup.Copy(attGroup, AttributeGroupMenu));
                        }
                    }
                }
                );
        }

        public object DisplayedUI { get; set; }
        public VM_BodyGenGroupMappingMenu GroupMappingUI { get; set; }
        public VM_BodyGenGroupsMenu GroupUI { get; set; }
        public VM_BodyGenMorphDescriptorMenu DescriptorUI { get; set; }
        public VM_BodyGenTemplateMenu TemplateMorphUI { get; set; }

        public VM_AttributeGroupMenu AttributeGroupMenu { get; set; }

        public string SourcePath { get; set; }

        public ICommand ClickTemplateMenu { get; }
        public ICommand ClickGroupMappingMenu { get; }
        public ICommand ClickDescriptorMenu { get; }
        public ICommand ClickGroupsMenu { get; }
        public ICommand ClickAttributeGroupsMenu { get; }
        public RelayCommand ImportAttributeGroups { get; }

        public string Label { get; set; }
        public Gender Gender { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;

        public static VM_BodyGenConfig GetViewModelFromModel(BodyGenConfig model, ObservableCollection<VM_RaceGrouping> raceGroupingVMs)
        {
            VM_BodyGenConfig viewModel = new VM_BodyGenConfig(raceGroupingVMs);
            viewModel.Label = model.Label;
            viewModel.Gender = model.Gender;
            
            viewModel.GroupUI.TemplateGroups = new ObservableCollection<VM_CollectionMemberString>();
            foreach (string group in model.TemplateGroups)
            {
                viewModel.GroupUI.TemplateGroups.Add(new VM_CollectionMemberString(group, viewModel.GroupUI.TemplateGroups));
            }

            foreach (var RTG in model.RacialTemplateGroupMap)
            {
                viewModel.GroupMappingUI.RacialTemplateGroupMap.Add(VM_BodyGenRacialMapping.GetViewModelFromModel(RTG, viewModel.GroupUI, raceGroupingVMs));
            }

            viewModel.DescriptorUI.TemplateDescriptors = VM_BodyGenMorphDescriptorShell.GetViewModelsFromModels(model.TemplateDescriptors);

            foreach (var descriptor in model.TemplateDescriptors)
            {
                viewModel.DescriptorUI.TemplateDescriptorList.Add(VM_BodyGenMorphDescriptor.GetViewModelFromModel(descriptor));
            }

            foreach (var template in model.Templates)
            {
                var templateVM = new VM_BodyGenTemplate(viewModel.GroupUI.TemplateGroups, viewModel.DescriptorUI, raceGroupingVMs, viewModel.TemplateMorphUI.Templates, viewModel);
                VM_BodyGenTemplate.GetViewModelFromModel(template, templateVM, viewModel.DescriptorUI, raceGroupingVMs);
                viewModel.TemplateMorphUI.Templates.Add(templateVM);
            }

            VM_AttributeGroupMenu.GetViewModelFromModels(model.AttributeGroups, viewModel.AttributeGroupMenu);

            viewModel.SourcePath = model.FilePath;

            return viewModel;
        }

        public static BodyGenConfig DumpViewModelToModel(VM_BodyGenConfig viewModel)
        {
            BodyGenConfig model = new BodyGenConfig();
            model.Label = viewModel.Label;
            model.Gender = viewModel.Gender;
            model.TemplateGroups = viewModel.GroupUI.TemplateGroups.Select(x => x.Content).ToHashSet();
            foreach (var RTG in viewModel.GroupMappingUI.RacialTemplateGroupMap)
            {
                model.RacialTemplateGroupMap.Add(VM_BodyGenRacialMapping.DumpViewModelToModel(RTG));
            }
            model.TemplateDescriptors = VM_BodyGenMorphDescriptorShell.DumpViewModelsToModels(viewModel.DescriptorUI.TemplateDescriptors);
            foreach (var template in viewModel.TemplateMorphUI.Templates)
            {
                model.Templates.Add(VM_BodyGenTemplate.DumpViewModelToModel(template));
            }
            VM_AttributeGroupMenu.DumpViewModelToModels(viewModel.AttributeGroupMenu, model.AttributeGroups);
            model.FilePath = viewModel.SourcePath;
            return model;
        }
    }

    
    

    
}
