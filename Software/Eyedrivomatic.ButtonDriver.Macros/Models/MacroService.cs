//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class MacroService : BindableBase, IMacroService
    {
        private readonly IMacroSerializationService _macroSerializationService;
        private bool _hasChanges;
        private ObservableCollection<IMacro> _macros;

        [ImportingConstructor]
        public MacroService(IMacroSerializationService macroSerializationService)
        {
            _macroSerializationService = macroSerializationService;
        }


        public ObservableCollection<IMacro> Macros
        {
            get
            {
                if (_macros == null)
                {
                    _macros = new ObservableCollection<IMacro>(_macroSerializationService.LoadMacros());
                    _macros.CollectionChanged += MacrosCollectionChanged;
                }

                return _macros;
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set => SetProperty(ref _hasChanges, value);
        }

        private void MacrosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasChanges = true;
        }

        public void AddMacro(UserMacro macro)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            _macros.Clear();
            _macros.AddRange(_macroSerializationService.LoadMacros());
            HasChanges = false;
        }

        public void Save()
        {
            if (_macros == null) return;

            _macroSerializationService.SaveMacros(_macros);
            HasChanges = false;
        }
    }
}
