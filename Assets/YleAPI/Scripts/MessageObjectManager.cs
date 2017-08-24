using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YleAPI
{
    public enum eMessage
    {
        None = -1,
        SearchButtonClick,
        SearchProgramViewEndDrag,
        SearchProgramItemClick,
        ProgramViewBackButtonClick,

        TypeEnd
    }

    public interface IMessageObject
    {
        bool MessageProc(eMessage type, object value);
    }

    public class MessageObjectManager : Singleton<MessageObjectManager>
    {
        private List<IMessageObject>[] _messageObjects = new List<IMessageObject>[(int)eMessage.TypeEnd];
        private Dictionary<IMessageObject, List<eMessage>> _removeHelper = new Dictionary<IMessageObject, List<eMessage>>();

        public MessageObjectManager()
        {
            for (int i = 0; i < (int)eMessage.TypeEnd; ++i)
            {
                _messageObjects[i] = new List<IMessageObject>();
            }
        }

        public bool Add(eMessage type, IMessageObject messageObject)
        {
            if((uint)type >= (uint)eMessage.TypeEnd)
            {
                return false;
            }

            if (_messageObjects[(int)type].Contains(messageObject) == true)
            {
                return false;
            }        
                    
            _messageObjects[(int)type].Add(messageObject);

            if(_removeHelper.ContainsKey(messageObject) == false)
            {
                _removeHelper.Add(messageObject, new List<eMessage>());
            }

            _removeHelper[messageObject].Add(type);

            return true;
        }

        public bool Remove(IMessageObject messageObject)
        {
            if (_removeHelper.ContainsKey(messageObject) == false)
            {
                return false;
            }

            List<eMessage> messages = _removeHelper[messageObject];

            for (int i = 0; i < messages.Count; ++i)
            {
                _messageObjects[(int)messages[i]].Remove(messageObject);            
            }

            _removeHelper.Remove(messageObject);

            return true;
        }

        public bool SendMessageToAll(eMessage type, object value = null)
        {
            if ((uint)type >= (uint)eMessage.TypeEnd)
            {
                return false;
            }

            int count = _messageObjects[(int)type].Count;

            for (int i = 0; i < count; ++i)
            {
                _messageObjects[(int)type][i].MessageProc(type, value);
            }

            return true;
        }
    }
}