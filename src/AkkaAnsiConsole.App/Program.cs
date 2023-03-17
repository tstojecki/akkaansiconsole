using Akka.Hosting;
using AkkaAnsiConsole.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddAkka("MyActorSystem", (builder, sp) =>
    {
        builder
            .WithActors((system, registry, resolver) =>
            {
                var helloActor = system.ActorOf(Props.Create(() => new HelloActor()), "hello-actor");
                registry.Register<HelloActor>(helloActor);
            })
            .WithActors((system, registry, resolver) =>
            {
                var timerActorProps =
                    resolver.Props<TimerActor>(); // uses Msft.Ext.DI to inject reference to helloActor
                var timerActor = system.ActorOf(timerActorProps, "timer-actor");
                registry.Register<TimerActor>(timerActor);
            });
    });
});

var host = hostBuilder.Start();

var actorSystem = host.Services.GetRequiredService<ActorSystem>();
var actorRegistry = host.Services.GetRequiredService<ActorRegistry>();
var helloActor = await actorRegistry.GetAsync<HelloActor>();

var table = new Table().Centered();
AnsiConsole.Live(table)
    .Start(ctx =>
    {
        var liveConsoleActor = actorSystem.ActorOf(Props.Create(() => new LiveConsoleActor(table, ctx)));
        helloActor.Tell(new HelloActor.SubscribeActor(liveConsoleActor));

        while (true)
        {
            Thread.Sleep(1000);
        }
    });

await host.WaitForShutdownAsync();

public class LiveConsoleActor : ReceiveActor
{
    public LiveConsoleActor(Table table, LiveDisplayContext liveDisplayContext)
    {
        Receive<string>(message =>
        {
            table.AddColumn(message);
            liveDisplayContext.Refresh();
        });
    }
}