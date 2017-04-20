using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

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
            Contract.Requires<ArgumentNullException>(macroSerializationService != null, nameof(macroSerializationService));
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
            get { return _hasChanges; }
            set { SetProperty(ref _hasChanges, value); }
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
