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
        
        private void RemoveLastRoleMessage(string role)
        {
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Role == role)
                {
                    _messages.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveLastRequest()
        {
            if (_messages.Count <= 2)
            {
                throw new Exception($"History must contains at least 2 messages");
                return;
            }
            
            RemoveLastRoleMessage(AI_ROLE);
            RemoveLastRoleMessage(USER_ROLE);
        }

        public void RemoveRequestAtIndex(int promtIndex)
        {
            if (_messages.Count <= 0)
            {
                throw new Exception("History is empty!");
            }

            if (_messages.Count <= promtIndex || promtIndex<0)
            {
                throw new ArgumentOutOfRangeException(nameof(promtIndex), $"Promt index {promtIndex} is out of range");
            }

            int characterMessageId = promtIndex + 1;

            if (_messages.Count > characterMessageId)
            {
                _messages.RemoveAt(characterMessageId);
            }
            
            _messages.RemoveAt(promtIndex);
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