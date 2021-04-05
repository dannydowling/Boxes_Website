using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;


namespace Boxes
{       
    public class ChatClient 
    {        
        private readonly NavigationManager _navigationManager;

        public HubConnection _hubConnection;
               
        public ChatClient(string username, NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;

            // save username
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            _username = username;
        }
       
        private readonly string _username;
        
        private bool _started = false;

        public async Task StartAsync()
        {
            if (!_started)
            {

                using (var _hubConnection = new HubConnection("/Chat"))
                {
                    //create a HubConnection to the server  
                   
                    //make a proxy that can handle messages
                    IHubProxy messagingHubProxy = _hubConnection.CreateHubProxy("MessagingHub");

                    Console.WriteLine("ChatClient: calling Start()");

                    //when the proxy sees a message with a user and the message, handle it
                    messagingHubProxy.On<string, string>(MessageModel.accept, (user, message) =>
                    {
                        HandleReceiveMessage(user, message);
                    });

                    //start the underlying connection
                    await _hubConnection.Start();

                    Console.WriteLine("ChatClient: Start returned");
                    _started = true;
                }
                //send it
                await _hubConnection.Send((MessageModel.register, _username));

            }
        }
                
        private void HandleReceiveMessage(string username, string message)
        { MessageReceived?.Invoke(this, new MessageReceivedEventArgs(username, message));  }        
        
        public event MessageReceivedEventHandler MessageReceived;
       
        public async Task SendAsync(string message)
        {           
            if (!_started)
                throw new InvalidOperationException("Client not started");
            
            await _hubConnection.Send((MessageModel.send, _username, message));
        }
        
        public void Stop()
        {
            if (_started)
            {               
                _hubConnection.Stop();               
                _hubConnection.Dispose();
                _hubConnection = null;
                _started = false;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("ChatClient: Disposing");
            Stop();
        }
    }

   
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
   
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }
        
        public string Username { get; set; }       
        public string Message { get; set; }

    }

}
