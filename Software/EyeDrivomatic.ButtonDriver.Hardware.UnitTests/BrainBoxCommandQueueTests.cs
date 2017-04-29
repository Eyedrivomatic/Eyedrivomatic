// Copyright (c) 2016 Eyedrivomatic Authors
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
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using NUnit.Framework;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using FakeItEasy;


namespace EyeDrivomatic.ButtonDriver.Hardware.UnitTests
{
    [TestFixture]
    class BrainBoxCommandQueueTests
    {
        [Test]
        public async Task Test_CanQueueCommands()
        {
            var command = A.Fake<IBrainBoxCommand>();
            var commandTcs = new TaskCompletionSource<bool>();

            var commandHandler = A.Fake<IBrainBoxCommandHandler>();
            A.CallTo(() => commandHandler.CommandTask).Returns(commandTcs.Task);
            A.CallTo(() => commandHandler.HandleResponse(true))
                .Invokes(a => commandTcs.SetResult((bool)a.Arguments[0]))
                .Returns(true);

            var commandHandlerFactory = new Func<IBrainBoxCommand, IBrainBoxCommandHandler>(cmd => commandHandler);
            var responseSource = new Subject<char>();
            var sut = new BrainBoxCommandQueue(commandHandlerFactory);
            
            sut.Attach(responseSource.AsObservable());
            var sendTask = sut.SendCommand(command);
            responseSource.OnNext(BrainBoxCommandQueue.Ack);
            var result = await sendTask;

            A.CallTo(() => commandHandler.HandleResponse(true)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Test_FailedCommandFailsTask()
        {
            var command = A.Fake<IBrainBoxCommand>();
            var commandTcs = new TaskCompletionSource<bool>();

            var commandHandler = A.Fake<IBrainBoxCommandHandler>();
            A.CallTo(() => commandHandler.CommandTask).Returns(commandTcs.Task);
            A.CallTo(() => commandHandler.HandleResponse(false))
                .Invokes(a => commandTcs.SetResult((bool)a.Arguments[0]))
                .Returns(true);

            var commandHandlerFactory = new Func<IBrainBoxCommand, IBrainBoxCommandHandler>(cmd => commandHandler);
            var responseSource = new Subject<char>();
            var sut = new BrainBoxCommandQueue(commandHandlerFactory);

            sut.Attach(responseSource.AsObservable());
            var sendTask = sut.SendCommand(command);
            responseSource.OnNext(BrainBoxCommandQueue.Nak);
            var result = await sendTask;

            A.CallTo(() => commandHandler.HandleResponse(false)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.That(result, Is.False);
        }


        [TestCase(2)]
        [TestCase(4)]
        public async Task Test_CanQueueMultipleCommands(int nCommands)
        {
            var commands = Enumerable.Range(0, nCommands).Select(i =>
            {
                var command = A.Fake<IBrainBoxCommand>();
                A.CallTo(() => command.Name).Returns($"Command {i}");
                return command;
            }).ToList();



            var responseSource = new Subject<char>();
            var commandHandlerFactory = new Func<IBrainBoxCommand, IBrainBoxCommandHandler>(cmd =>
            {
                var commandTcs = new TaskCompletionSource<bool>();

                var commandHandler = A.Fake<IBrainBoxCommandHandler>();
                    A.CallTo(() => commandHandler.CommandTask).Returns(commandTcs.Task);
                    A.CallTo(() => commandHandler.Send(A<TimeSpan>.Ignored, A<CancellationToken>.Ignored))
                        .Invokes(() => responseSource.OnNext(BrainBoxCommandQueue.Ack))
                        .Returns(commandTcs.Task);
                    A.CallTo(() => commandHandler.HandleResponse(A<bool>.Ignored))
                        .Invokes(a => commandTcs.SetResult((bool)a.Arguments[0]))
                        .Returns(true);
                    return commandHandler;
            });


            var sut = new BrainBoxCommandQueue(commandHandlerFactory);

            sut.Attach(responseSource.AsObservable());

            var sendTasks = commands.Select(command => new { Command = command, sendTask = sut.SendCommand(command) }).ToList();


            var iCommand = 0;
            while (sendTasks.Any())
            {
                var resultTask = await Task.WhenAny(sendTasks.Select(st => st.sendTask));
                var sendTask = sendTasks.Single(st => st.sendTask == resultTask);

                Assert.That(sendTask.Command.Name, Is.EqualTo($"Command {iCommand++}"));

                sendTasks.Remove(sendTask);
                Assert.That(resultTask.Result, Is.True);
            }
        }
    }
}
