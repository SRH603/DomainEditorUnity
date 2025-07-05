using System;


namespace MessagePack
{
    public class MessagePackObjectAttribute : Attribute
    {  
    }

    public class KeyAttribute : Attribute
    {
        public KeyAttribute(int index)
        {
        }
    }

}
