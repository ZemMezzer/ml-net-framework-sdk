using Newtonsoft.Json;

namespace MLSDK.Data
{

    [Serializable]
    public class History
    {
        [Serializable]
        public struct HistoryMessage
        {
            [JsonProperty("role")] public string Role;
            [JsonProperty("content")] public string Content;

            public HistoryMessage(string role, string content)
            {
                Role = role;
                Content = content;
            }
        }

        private const string SYSTEM_ROLE = "system";
        private const string AI_ROLE = "assistant";
        private const string USER_ROLE = "user";

        [JsonProperty("messages")] private List<HistoryMessage> _messages;

        public IReadOnlyList<HistoryMessage> Messages => _messages;

        public History(string innerMessage = "", string context = "")
        {
            _messages = new List<HistoryMessage>();

            if (!string.IsNullOrEmpty(context))
            {
                _messages.Add(new HistoryMessage(SYSTEM_ROLE, context));
            }

            if (!string.IsNullOrEmpty(innerMessage))
            {
                _messages.Add(new HistoryMessage(AI_ROLE, innerMessage));
            }
        }

        public void AddPromt(string message)
        {
            _messages.Add(new HistoryMessage(USER_ROLE, message));
        }

        public void SetModelMessage(string message)
        {
            _messages.Add(new HistoryMessage(AI_ROLE, message));
        }

        public void RemoveLast()
        {
            _messages.RemoveAt(_messages.Count - 1);
        }

        public void RemoveAt(int index)
        {
            if (index >= _messages.Count || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            
            _messages.RemoveAt(index);
        }

        public void Clear(string innerMessage)
        {
            _messages.Clear();

            if (!string.IsNullOrEmpty(innerMessage))
            {
                _messages.Add(new HistoryMessage(AI_ROLE, innerMessage));
            }
        }

        public HistoryMessage GetLastPromt()
        {
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Role == USER_ROLE)
                    return _messages[i];
            }

            return default;
        }
    }
}