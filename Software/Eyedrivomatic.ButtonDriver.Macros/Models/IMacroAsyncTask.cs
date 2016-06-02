﻿// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [ContractClass(typeof(Contracts.MacroAsyncTaskContract))]
    public interface IMacroAsyncTask
    {
        bool CanExecute();
        Task ExecuteAsync();
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IMacroAsyncTask))]
        public abstract class MacroAsyncTaskContract : IMacroAsyncTask
        {
            public abstract bool CanExecute();

            public Task ExecuteAsync()
            {
                Contract.Ensures(Contract.Result<Task>() != null);
                throw new NotImplementedException();
            }
        }
    }
}
