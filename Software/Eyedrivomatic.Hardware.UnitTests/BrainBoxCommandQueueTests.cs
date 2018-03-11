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


using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Commands;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Hardware.Services;
using FakeItEasy;
using NUnit.Framework;

namespace Eyedrivomatic.Hardware.UnitTests
{
    [TestFixture]
    class BrainBoxCommandQueueTests
    {
        [Test]
        public async Task Test_CanQueueCommands()
        {
            var command = A.Fake<IDeviceCommand>();
            var commandTcs = new TaskCompletionSource<bool>();

            var commandHandler = A.Fake<IDeviceCommandHandler>();
            A.CallTo(() => commandHandler.CommandTask).Returns(commandTcs.Task);
            A.CallTo(() => commandHandler.HandleResponse(true))
                .Invokes(a => commandTcs.SetResult((bool)a.Arguments[0]))
                .Returns(true);

            var commandHandlerFactory = new Func<IDeviceCommand, Action<string>, IDeviceCommandHandler>((cmd, sink) => commandHandler);
            var responseSource = new Subject<char>();
            var sut = new DeviceCommandQueue(commandHandlerFactory);
            
            sut.Attach(responseSource.AsObservable(), new AnonymousObserver<string>(message => Console.WriteLine($"MESSAGE => {message}")));
            var sendTask = sut.SendCommand(command);
            responseSource.OnNext(DeviceCommandQueue.Ack);
            var result = await sendTask;

            A.CallTo(() => commandHandler.HandleResponse(true)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Test_FailedCommandFailsTask()
        {
            var command = A.Fake<IDeviceCommand>();
            var commandTcs = new TaskCompletionSource<bool>();

            var commandHandler = A.Fake<IDeviceCommandHandler>();
            A.CallTo(() => commandHandler.CommandTask).Returns(commandTcs.Task);
            A.CallTo(() => commandHandler.HandleResponse(false))
                .Invokes(a => commandTcs.SetResult((bool)a.Arguments[0]))
                .Returns(true);

            var commandHandlerFactory = new Func<IDeviceCommand, Action<string>, IDeviceCommandHandler>((cmd, sink) => commandHandler);
            var responseSource = new Subject<char>();
            var sut = new DeviceCommandQueue(commandHandlerFactory);

            sut.Attach(responseSource.AsObservable(), new AnonymousObserver<string>(message => Console.WriteLine($"MESSAGE => {message}")));
            var sendTask = sut.SendCommand(command);
            responseSource.OnNext(DeviceCommandQueue.Nak);
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
                var command = A.Fake<IDeviceCommand>();
                A.CallTo(() => command.Name).Returns($"Command {i}");
                return command;
            }).ToList();



            var responseSource = new Subject<char>();
            var commandHandlerFactory = new Func<IDeviceCommand, Action<string>, IDeviceCommandHandler>((cmd, sink) =>
            {
                var commandTcs = new TaskCompletionSource<bool>();

                var commandHandler = A.Fake<IDeviceCommandHandler>();
                    A.CallTo(() => commandHandler.CommandTask).Returns(commandTcs.Task);
                    A.CallTo(() => commandHandler.Send(A<TimeSpan>.Ignored, A<CancellationToken>.Ignored))
                        .Invokes(() => responseSource.OnNext(DeviceCommandQueue.Ack))
                        .Returns(commandTcs.Task);
                    A.CallTo(() => commandHandler.HandleResponse(A<bool>.Ignored))
                        .Invokes(a => commandTcs.SetResult((bool)a.Arguments[0]))
                        .Returns(true);
                    return commandHandler;
            });


            var sut = new DeviceCommandQueue(commandHandlerFactory);

            sut.Attach(responseSource.AsObservable(), new AnonymousObserver<string>(message => Console.WriteLine($"MESSAGE => {message}")));

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
