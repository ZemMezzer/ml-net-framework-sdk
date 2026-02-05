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

            public bool IsUserMessage => Role == USER_ROLE;
            
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
        private Dictionary<string, string> _historyBatches;
        
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> HistoryBatches => _historyBatches;

        [JsonIgnore]
        public IReadOnlyList<HistoryMessage> Messages => _messages;

        public History(string innerMessage = "")
        {
            _messages = new List<HistoryMessage>();
            _historyBatches = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(innerMessage))
            {
                _messages.Add(new HistoryMessage(AI_ROLE, innerMessage));
            }
        }

        public void AddPromt(string message)
        {
            _messages.Add(new HistoryMessage(USER_ROLE, message));
        }

        public void AddSystemMessage(string message)
        {
            _messages.Add(new HistoryMessage(SYSTEM_ROLE, message));
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

        public void Clear(string innerMessage = "", string context = "")
        {
            _messages.Clear();

            if (!string.IsNullOrEmpty(context))
            {
                _messages.Add(new HistoryMessage(SYSTEM_ROLE, context));
            }
            
            if (!string.IsNullOrEmpty(innerMessage))
            {
                _messages.Add(new HistoryMessage(AI_ROLE, innerMessage));
            }
        }

        public void ClearBatches()
        {
            _historyBatches.Clear();
        }

        public bool IsLastMessageResponseOnPromt()
        {
            if (_messages.Count <= 1)
                return false;

            return _messages[^1].Role == AI_ROLE && _messages[^2].IsUserMessage;
        }
        
        public HistoryMessage GetLastPromt()
        {
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Role == USER_ROLE)
                    return _messages[i];
            }

            return default;
        }

        public HistoryMessage GetLastResponse()
        {
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Role == AI_ROLE)
                    return _messages[i];
            }

            return default;
        }
        
        public HistoryMessage? GetLastMessage()
        {
            if (_messages.Count > 0)
                return _messages[^1];

            return default;
        }
        
        private void RemoveLastRoleMessage(string role)
        {
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Role == role)
                {
                    _messages.RemoveAt(i);
                    return;
                }
            }
        }
        
        private int GetMessagesCountFromRole(string role)
        {
            var count = 0;
            
            foreach (var message in _messages)
            {
                if (message.Role == role)
                {
                    count++;
                }
            }
            
            return count;
        }

        public void RemoveLastCharacterMessage() => RemoveLastRoleMessage(AI_ROLE);
        public void RemoveLastUserMessage() => RemoveLastRoleMessage(USER_ROLE);

        public int GetCharacterMessageCount() => GetMessagesCountFromRole(AI_ROLE);
        public int GetUserMessageCount() => GetMessagesCountFromRole(USER_ROLE);
 
        public History GetCopy()
        {
            var history = new History();

            foreach (var message in _messages)
            {
                history._messages.Add(message);
            }

            foreach (var batch in _historyBatches)
            {
                history._historyBatches.Add(batch.Key, batch.Value);
            }

            return history;
        }

        public bool IsEmpty(bool ignoreContext = true)
        {
            if (_messages.Count <= 0)
                return true;

            return _messages.Count <= 1 && _messages[0].Role == SYSTEM_ROLE && ignoreContext;
        }

        public string GetBatch(string key)
        {
            return _historyBatches.TryGetValue(key, out var batch) ? batch : string.Empty;
        }

        public void SetBatch(string key, string batch)
        {
            _historyBatches[key] = batch;
        }

        public void RemoveBatch(string key)
        {
            if (_historyBatches.ContainsKey(key))
            {
                _historyBatches.Remove(key);
            }
        }

        public string GetSummary(bool ignoreSystem = true)
        {
            var summary = string.Empty;

            foreach (var message in _messages)
            {
                if(message.Role == SYSTEM_ROLE && ignoreSystem)
                    continue;
                
                summary += $"{message.Role}: {message.Content}\n";
            }
            
            return summary;
        }
    }
}