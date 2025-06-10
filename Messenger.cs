using System;
using System.Collections.Generic;

// Общие модули
namespace UnifiedMessagingSystem
{
    // Пользователи
    class User
    {
        public string UserName { get; set; }
        public int UserID { get; set; }

        public User(string userName, int userID)
        {
            UserName = userName;
            UserID = userID;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"User: {UserName} (ID: {UserID})");
        }
    }

    class UserManager
    {
        private Dictionary<int, User> users = new Dictionary<int, User>();
        private MessengerProxy messengerProxy;

        public UserManager(MessengerProxy proxy)
        {
            messengerProxy = proxy;
        }

        public void AddUser(User user)
        {
            users[user.UserID] = user;
            Console.WriteLine($"User added: {user.UserName}");
        }

        public User GetUser(int userID)
        {
            return users.ContainsKey(userID) ? users[userID] : null;
        }

        public void DisplayUsers()
        {
            Console.WriteLine("\nList of Users:");
            foreach (var user in users.Values)
            {
                user.DisplayInfo();
            }
        }

        public void DisplayUserChats(int userID)
        {
            var chats = messengerProxy.GetUserChats(userID);
            Console.WriteLine($"Chats for user {userID}:");
            foreach (var chat in chats)
            {
                Console.WriteLine($"Chat ID: {chat.ChatID}");
            }
        }
    }

    // Сообщения
    public class Message
    {
        public int MessageID { get; private set; }
        public int SenderID { get; private set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Message(int messageID, int senderID, string content)
        {
            MessageID = messageID;
            SenderID = senderID;
            Content = content;
            Timestamp = DateTime.Now;
        }
    }

    public class Chat
    {
        public int ChatID { get; private set; }
        public List<int> Participants { get; private set; }
        public List<Message> Messages { get; private set; }
        public List<Message> PinnedMessages { get; private set; }
        private MessengerProxy messengerProxy;

        public Chat(int chatID, List<int> participants, MessengerProxy proxy)
        {
            ChatID = chatID;
            Participants = participants;
            Messages = new List<Message>();
            PinnedMessages = new List<Message>();
            messengerProxy = proxy;
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        public void PinMessage(int messageID)
        {
            messengerProxy.PinMessage(ChatID, messageID);
        }

        public void DisplayPinnedMessages()
        {
            messengerProxy.DisplayPinnedMessages(ChatID);
        }
    }

    public class Messenger
    {
        private static Messenger _instance;
        private Dictionary<int, Chat> Chats;

        private Messenger()
        {
            Chats = new Dictionary<int, Chat>();
        }

        public static Messenger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Messenger();
                }
                return _instance;
            }
        }

        public List<Chat> GetUserChats(int userID)
        {
            List<Chat> userChats = new List<Chat>();
            foreach (var chat in Chats.Values)
            {
                if (chat.Participants.Contains(userID))
                {
                    userChats.Add(chat);
                }
            }
            return userChats;
        }

        public List<Message> GetMessages(int chatID)
        {
            if (Chats.ContainsKey(chatID))
            {
                return Chats[chatID].Messages;
            }
            throw new Exception("Chat not found");
        }

        public void SendMessage(int chatID, int senderID, string messageContent)
        {
            if (Chats.ContainsKey(chatID))
            {
                var message = new Message(new Random().Next(1, 10000), senderID, messageContent);
                Chats[chatID].AddMessage(message);
                Console.WriteLine($"Message sent to chat {chatID} by user {senderID}");
            }
            else
            {
                throw new Exception("Chat not found");
            }
        }

        public int CreateChat(List<int> participants)
        {
            int newChatID = new Random().Next(1, 10000);
            Chats[newChatID] = new Chat(newChatID, participants, MessengerProxy.Instance);
            Console.WriteLine($"Chat {newChatID} created.");
            return newChatID;
        }

        public void PinMessage(int chatID, int messageID)
        {
            if (Chats.ContainsKey(chatID))
            {
                Chats[chatID].PinMessage(messageID);
            }
            else
            {
                Console.WriteLine("Chat not found.");
            }
        }

        public void DisplayPinnedMessages(int chatID)
        {
            if (Chats.ContainsKey(chatID))
            {
                Chats[chatID].DisplayPinnedMessages();
            }
            else
            {
                Console.WriteLine("Chat not found.");
            }
        }
    }

    public class MessengerProxy
    {
        private Messenger messenger;
        private Dictionary<int, List<Message>> cacheMessages;
        private Dictionary<int, List<Chat>> cacheChats;

        private static MessengerProxy _instance;

        private MessengerProxy()
        {
            messenger = Messenger.Instance;
            cacheMessages = new Dictionary<int, List<Message>>();
            cacheChats = new Dictionary<int, List<Chat>>();
        }

        public static MessengerProxy Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessengerProxy();
                }
                return _instance;
            }
        }

        public List<Chat> GetUserChats(int userID)
        {
            if (!cacheChats.ContainsKey(userID))
            {
                Console.WriteLine("Cache miss for user chats. Fetching from Messenger.");
                var chats = messenger.GetUserChats(userID);
                cacheChats[userID] = chats;
            }
            else
            {
                Console.WriteLine("Cache hit for user chats.");
            }
            return cacheChats[userID];
        }

        public List<Message> GetMessages(int chatID)
        {
            if (!cacheMessages.ContainsKey(chatID))
            {
                Console.WriteLine("Cache miss for chat messages. Fetching from Messenger.");
                var messages = messenger.GetMessages(chatID);
                cacheMessages[chatID] = messages;
            }
            else
            {
                Console.WriteLine("Cache hit for chat messages.");
            }
            return cacheMessages[chatID];
        }

        public void SendMessage(int chatID, int senderID, string messageContent)
        {
            messenger.SendMessage(chatID, senderID, messageContent);
            if (cacheMessages.ContainsKey(chatID))
            {
                Console.WriteLine("Invalidating cache for chat messages due to new message.");
                cacheMessages.Remove(chatID);
            }
        }

        public void PinMessage(int chatID, int messageID)
        {
            messenger.PinMessage(chatID, messageID);
        }

        public void DisplayPinnedMessages(int chatID)
        {
            messenger.DisplayPinnedMessages(chatID);
        }
    }

    // Основной класс программы
    class Program
    {
        static void Main(string[] args)
        {
            MessengerProxy messengerProxy = MessengerProxy.Instance;
            UserManager userManager = new UserManager(messengerProxy);

            userManager.AddUser(new User("Alice", 1));
            userManager.AddUser(new User("Bob", 2));
            userManager.DisplayUsers();

            // Создание чата
            var chatID = messengerProxy.Instance.CreateChat(new List<int> { 1, 2 });

            // Отправка сообщения
            messengerProxy.SendMessage(chatID, 1, "Hello!");

            // Получение сообщений (демонстрация кеша)
            var messages1 = messengerProxy.GetMessages(chatID); // Cache miss
            var messages2 = messengerProxy.GetMessages(chatID); // Cache hit

            // Закрепление сообщения
            messengerProxy.PinMessage(chatID, messages1[0].MessageID);
            messengerProxy.DisplayPinnedMessages(chatID);
        }
    }
}
