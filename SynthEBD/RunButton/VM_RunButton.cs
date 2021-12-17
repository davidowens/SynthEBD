﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Mutagen.Bethesda.WPF;

namespace SynthEBD
{
    public class VM_RunButton : INotifyPropertyChanged
    {
        public VM_RunButton(MainWindow_ViewModel parentWindow)
        {
            this.BackgroundColor = new SolidColorBrush(Colors.Green);
            this.ParentWindow = parentWindow;

            // synchronous version for debugging only
            
            ClickRun = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ =>
                {
                    ParentWindow.DisplayedViewModel = ParentWindow.LogDisplayVM;
                    ParentWindow.SyncModelsToViewModels();
                    Patcher.RunPatcher(
                        ParentWindow.AssetPacks, ParentWindow.BodyGenConfigs, ParentWindow.HeightConfigs, ParentWindow.Consistency, ParentWindow.SpecificNPCAssignments,
                        ParentWindow.BlockList, ParentWindow.LinkedNPCNameExclusions, ParentWindow.LinkedNPCGroups, ParentWindow.TrimPaths, ParentWindow.RecordTemplateLinkCache, ParentWindow.RecordTemplatePlugins);
                    VM_ConsistencyUI.GetViewModelsFromModels(ParentWindow.Consistency, ParentWindow.CUIVM.Assignments); // refresh consistency after running patcher. Otherwise the pre-patching consistency will get reapplied from the view model upon patcher exit
                }
                );


            /*
            ClickRun = ReactiveUI.ReactiveCommand.CreateFromTask(
                
                execute: async _ =>
                {
                    ParentWindow.DisplayedViewModel = ParentWindow.LogDisplayVM;
                    ParentWindow.SyncModelsToViewModels();
                    await Task.Run(() => MainLoop.RunPatcher(
                        ParentWindow.AssetPacks, ParentWindow.BodyGenConfigs, ParentWindow.HeightConfigs, ParentWindow.Consistency, ParentWindow.SpecificNPCAssignments,
                        ParentWindow.BlockList, ParentWindow.LinkedNPCNameExclusions, ParentWindow.LinkedNPCGroups, ParentWindow.TrimPaths, ParentWindow.RecordTemplateLinkCache, ParentWindow.RecordTemplatePlugins));
                    VM_ConsistencyUI.GetViewModelsFromModels(ParentWindow.Consistency, ParentWindow.CUIVM.Assignments); // refresh consistency after running patcher. Otherwise the pre-patching consistency will get reapplied from the view model upon patcher exit
                });
            */
        }
        public SolidColorBrush BackgroundColor { get; set; }

        public MainWindow_ViewModel ParentWindow { get; set; }

        // ReactiveUI.IReactiveCommand ClickRun { get; }
        public SynthEBD.RelayCommand ClickRun { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
