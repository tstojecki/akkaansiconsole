using static LiveConsoleActor;

namespace AkkaAnsiConsole.App;

public class HelloActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private int _helloCounter = 0;
    
    private IActorRef? subscriber = null;
    
    public HelloActor()
    {
        Receive<string>(message =>
        {
            var msg = $"{message} {_helloCounter++}";

            if (subscriber != null) 
            {
                subscriber?.Tell(msg);
                return;
            }
            
           _log.Info(msg);
        });

        Receive<SubscribeActor>(x =>
        {
            subscriber = x.Ref;
        });
    }

    public record SubscribeActor(IActorRef Ref);
}